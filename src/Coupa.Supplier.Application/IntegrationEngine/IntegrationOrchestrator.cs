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

        var data = await sourceProvider.GetDataAsync(config.Source, cancellationToken);

        var errors = new List<RecordError>();
        var success = 0;

        if (config.ProcessingMode.Equals("Single", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var record in data)
            {
                if (record is null)
                {
                    continue;
                }

                var failed = await ProcessRecordAsync(record, config, targetProvider, errors, cancellationToken);
                if (!failed)
                {
                    success++;
                }
            }
        }
        else
        {
            var batches = data.Chunk(config.BatchSize).ToArray();
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Math.Max(1, config.DegreeOfParallelism)
            };

            await Parallel.ForEachAsync(batches, parallelOptions, async (batch, ct) =>
            {
                foreach (var record in batch)
                {
                    if (record is null)
                    {
                        continue;
                    }

                    var failed = await ProcessRecordAsync(record, config, targetProvider, errors, ct);
                    if (!failed)
                    {
                        Interlocked.Increment(ref success);
                    }
                }
            });
        }

        return new IntegrationRunResult(config.IntegrationName, data.Count, success, errors.Count, errors);
    }

    private async Task<bool> ProcessRecordAsync(
        JsonNode record,
        IntegrationConfiguration config,
        ITargetProvider targetProvider,
        List<RecordError> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            var mappedData = mappingEngine.Transform(record, config.Mapping);

            if (config.ProcessingMode.Equals("Bulk", StringComparison.OrdinalIgnoreCase))
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

            lock (errors)
            {
                errors.Add(error);
            }

            logger.LogError(ex, "Record processing failed for {IntegrationName}", config.IntegrationName);
            await errorLogService.LogAsync(error, cancellationToken);
            return true;
        }
    }
}
