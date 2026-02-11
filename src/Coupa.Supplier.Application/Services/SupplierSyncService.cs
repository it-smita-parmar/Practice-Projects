using Coupa.Supplier.Application.Configuration;
using Coupa.Supplier.Application.DTOs;
using Coupa.Supplier.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Coupa.Supplier.Application.Services;

public sealed class SupplierSyncService(
    ICoupaSupplierClient coupaSupplierClient,
    IServiceScopeFactory scopeFactory,
    ILogger<SupplierSyncService> logger,
    IOptions<SyncOptions> syncOptions) : ISupplierSyncService
{
    public async Task<SyncSuppliersResult> SyncAsync(SyncSuppliersRequest request, CancellationToken cancellationToken)
    {
        var suppliers = await coupaSupplierClient.GetSuppliersAsync(cancellationToken);
        var batchSize = request.BatchSize.GetValueOrDefault(syncOptions.Value.BatchSize);
        var validSuppliers = suppliers.Where(x => x.SupplierId > 0 && !string.IsNullOrWhiteSpace(x.Name)).ToList();
        var invalidSuppliers = suppliers.Except(validSuppliers).ToList();

        if (invalidSuppliers.Count > 0)
        {
            using var scope = scopeFactory.CreateScope();
            var invalidTarget = scope.ServiceProvider.GetRequiredService<ITargetRepositoryStrategy>().Resolve(request.Target);
            foreach (var invalid in invalidSuppliers)
            {
                await invalidTarget.LogErrorAsync(invalid.Source, invalid.RawJson, "Required fields missing: Supplier ID/Name", cancellationToken);
            }
        }

        var useBulkInsert = request.UseBulkInsert || syncOptions.Value.UseBulkInsertDefault;
        var totalInserted = 0;
        var totalFailedBatches = 0;
        var batches = validSuppliers.Chunk(batchSize).ToList();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = request.DegreeOfParallelism > 0 ? request.DegreeOfParallelism : syncOptions.Value.DegreeOfParallelism
        };

        await Parallel.ForEachAsync(batches, parallelOptions, async (batch, ct) =>
        {
            using var scope = scopeFactory.CreateScope();
            var targetRepository = scope.ServiceProvider.GetRequiredService<ITargetRepositoryStrategy>().Resolve(request.Target);

            try
            {
                var inserted = await targetRepository.InsertBatchAsync(batch, useBulkInsert, ct);
                Interlocked.Add(ref totalInserted, inserted);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Batch insert failed for target {TargetName}", targetRepository.TargetName);
                Interlocked.Add(ref totalFailedBatches, batch.Length);
                foreach (var failed in batch)
                {
                    await targetRepository.LogErrorAsync(failed.Source, failed.RawJson, ex.Message, ct);
                }
            }
        });

        var totalFailed = invalidSuppliers.Count + totalFailedBatches + Math.Max(validSuppliers.Count - totalInserted - totalFailedBatches, 0);
        return new SyncSuppliersResult(suppliers.Count, totalInserted, totalFailed);
    }
}
