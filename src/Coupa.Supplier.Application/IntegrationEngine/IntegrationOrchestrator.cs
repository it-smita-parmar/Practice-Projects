using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Microsoft.Extensions.Logging;

namespace Coupa.Supplier.Application.IntegrationEngine;

public sealed class IntegrationOrchestrator(
    IIntegrationConfigProvider integrationConfigProvider,
    ISourceProviderFactory sourceProviderFactory,
    ITargetProviderFactory targetProviderFactory,
    IMappingEngine mappingEngine,
    IErrorLogService errorLogService,
    ILogger<IntegrationOrchestrator> logger) : IIntegrationOrchestrator
{
    public async Task<IntegrationRunResult> RunAsync(string integrationName, CancellationToken cancellationToken)
    {
        var config = await integrationConfigProvider.GetByNameAsync(integrationName, cancellationToken);
        var sourceProvider = sourceProviderFactory.Resolve(config.Source.Type);
        var targetProvider = targetProviderFactory.Resolve(config.Target.Type);
        var records = await sourceProvider.GetDataAsync(config.Source, cancellationToken);

        var errors = new ConcurrentBag<RecordError>();
        var successCount = 0;

        if (config.ProcessingMode.Equals("Single", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var record in records.Where(r => r is not null))
            {
                if (!await TryProcessRecordAsync(record!, config, targetProvider, errors, cancellationToken))
                {
                    successCount++;
                }
            }
        }
        else
        {
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Math.Max(1, config.DegreeOfParallelism)
            };

            await Parallel.ForEachAsync(records.Where(r => r is not null).Chunk(Math.Max(1, config.BatchSize)), parallelOptions,
                async (batch, ct) =>
                {
                    foreach (var record in batch)
                    {
                        if (!await TryProcessRecordAsync(record!, config, targetProvider, errors, ct))
                        {
                            Interlocked.Increment(ref successCount);
                        }
                    }
                });
        }

        return new IntegrationRunResult(config.IntegrationName, records.Count, successCount, errors.Count, errors.ToArray());
    }

    private async Task<bool> TryProcessRecordAsync(
        JsonNode record,
        IntegrationConfiguration config,
        ITargetProvider targetProvider,
        ConcurrentBag<RecordError> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            var mappedData = mappingEngine.Transform(record, config.Mapping);
            var useBulk = config.ProcessingMode.Equals("Bulk", StringComparison.OrdinalIgnoreCase);

            if (useBulk)
            {
                await targetProvider.InsertBulkAsync(config.Target, mappedData, cancellationToken);
            }
            else
            {
                await targetProvider.InsertSingleAsync(config.Target, mappedData, cancellationToken);
            }

            return false;
        }
        catch (Exception ex)
        {
            var error = new RecordError
            {
                IntegrationName = config.IntegrationName,
                TargetType = config.Target.Type,
                TableName = string.Join(',', config.Mapping.Tables.Select(x => x.TableName)),
                RawPayload = record.ToJsonString(),
                ErrorMessage = ex.Message,
                StackTrace = ex.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            errors.Add(error);
            logger.LogError(ex, "Record processing failed for {IntegrationName}", config.IntegrationName);
            await errorLogService.LogAsync(error, cancellationToken);
            return true;
        }
    }
}
