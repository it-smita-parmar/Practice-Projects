using Coupa.Supplier.Application.Interfaces;
using Coupa.Supplier.Domain.Entities;
using Coupa.Supplier.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Coupa.Supplier.Infrastructure.Targets;

public sealed class OracleTargetRepository(
    SupplierDbContext dbContext,
    IConfiguration configuration,
    IUnitOfWork unitOfWork) : ITargetSupplierRepository
{
    public string TargetName => "oracle";

    public async Task<int> InsertBatchAsync(IReadOnlyCollection<SupplierAggregate> suppliers, bool useBulkInsert, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var inserted = useBulkInsert
                ? await BulkInsertAsync(suppliers, cancellationToken)
                : await RowByRowInsertAsync(suppliers, cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);
            return inserted;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task LogErrorAsync(string source, string rawJson, string errorMessage, CancellationToken cancellationToken)
    {
        var nextId = await dbContext.ErrorLogs.Select(x => x.Id).DefaultIfEmpty(0).MaxAsync(cancellationToken) + 1;
        dbContext.ErrorLogs.Add(new CoupaErrorLogRow
        {
            Id = nextId,
            Source = source,
            RawJson = rawJson,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> RowByRowInsertAsync(IReadOnlyCollection<SupplierAggregate> suppliers, CancellationToken cancellationToken)
    {
        foreach (var supplier in suppliers)
        {
            var stgId = await GetNextSequenceValueAsync("XXFIN_SUPP_STG_SEQ", cancellationToken);
            dbContext.Suppliers.Add(ToSupplierRow(supplier, stgId));
            await dbContext.SaveChangesAsync(cancellationToken);
            await InsertChildrenRowByRowAsync(supplier, stgId, cancellationToken);
        }

        return suppliers.Count;
    }

    private async Task<int> BulkInsertAsync(IReadOnlyCollection<SupplierAggregate> suppliers, CancellationToken cancellationToken)
    {
        await using var connection = new OracleConnection(configuration.GetConnectionString("OraclePrimary"));
        await connection.OpenAsync(cancellationToken);

        var stgIds = new List<long>(suppliers.Count);
        foreach (var _ in suppliers)
        {
            stgIds.Add(await GetNextSequenceValueFromConnectionAsync("XXFIN_SUPP_STG_SEQ", connection, cancellationToken));
        }

        using var command = connection.CreateCommand();
        command.BindByName = true;
        command.CommandText = @"
            INSERT INTO xxfin.xxfin_coupa_suppliers
            (STG_ID,SOURCE,LAST_UPDATED_BY,LAST_UPDATE_DATE,CREATED_BY,CREATION_DATE,INTEGRATION_STATUS,SUPPLIER_ID,NAME,STATUS)
            VALUES (:stgId,:source,:lastUpdatedBy,:lastUpdateDate,:createdBy,:creationDate,:integrationStatus,:supplierId,:name,:status)";

        command.ArrayBindCount = suppliers.Count;
        command.Parameters.Add(":stgId", OracleDbType.Int64, stgIds.ToArray(), ParameterDirection.Input);
        command.Parameters.Add(":source", OracleDbType.Varchar2, suppliers.Select(x => x.Source).ToArray(), ParameterDirection.Input);
        command.Parameters.Add(":lastUpdatedBy", OracleDbType.Varchar2, suppliers.Select(_ => "SYSTEM").ToArray(), ParameterDirection.Input);
        command.Parameters.Add(":lastUpdateDate", OracleDbType.Date, suppliers.Select(_ => DateTime.UtcNow).ToArray(), ParameterDirection.Input);
        command.Parameters.Add(":createdBy", OracleDbType.Varchar2, suppliers.Select(_ => "SYSTEM").ToArray(), ParameterDirection.Input);
        command.Parameters.Add(":creationDate", OracleDbType.Date, suppliers.Select(_ => DateTime.UtcNow).ToArray(), ParameterDirection.Input);
        command.Parameters.Add(":integrationStatus", OracleDbType.Varchar2, suppliers.Select(_ => "NEW").ToArray(), ParameterDirection.Input);
        command.Parameters.Add(":supplierId", OracleDbType.Int64, suppliers.Select(x => x.SupplierId).ToArray(), ParameterDirection.Input);
        command.Parameters.Add(":name", OracleDbType.Varchar2, suppliers.Select(x => x.Name).ToArray(), ParameterDirection.Input);
        command.Parameters.Add(":status", OracleDbType.Varchar2, suppliers.Select(x => x.Status).ToArray(), ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);

        for (var i = 0; i < suppliers.Count; i++)
        {
            await InsertChildrenRowByRowAsync(suppliers.ElementAt(i), stgIds[i], cancellationToken);
        }

        return suppliers.Count;
    }

    private async Task InsertChildrenRowByRowAsync(SupplierAggregate supplier, long stgId, CancellationToken cancellationToken)
    {
        foreach (var site in supplier.Sites)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO xxfin.xxfin_coupa_supp_sites
                (STG_ID,SOURCE,SUPPLIER_ID,SITE_ID,CODE,NAME,PO_METHOD,ACTIVE,PO_CHANGE_METHOD,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,INTEGRATION_STATUS)
                VALUES ({stgId},{supplier.Source},{supplier.SupplierId},{site.SiteId},{site.Code},{site.Name},{site.PoMethod},{(site.Active ? "true" : "false")},{site.PoChangeMethod},{"SYSTEM"},{DateTime.UtcNow},{"SYSTEM"},{DateTime.UtcNow},{"NEW"})", cancellationToken);
        }

        foreach (var contact in supplier.Contacts)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO xxfin.xxfin_coupa_supp_contacts
                (STG_ID,SOURCE,SUPPLIER_ID,CONTACT_ID,KIND,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,INTEGRATION_STATUS)
                VALUES ({stgId},{supplier.Source},{supplier.SupplierId},{contact.ContactId},{contact.Kind},{"SYSTEM"},{DateTime.UtcNow},{"SYSTEM"},{DateTime.UtcNow},{"NEW"})", cancellationToken);
        }

        foreach (var address in supplier.Addresses)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO xxfin.xxfin_coupa_supp_adds
                (STG_ID,SOURCE,SUPPLIER_ID,ADDRESS_ID,LOCATION_CODE,ACTIVE,STATE_ISO_CODE,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,INTEGRATION_STATUS)
                VALUES ({stgId},{supplier.Source},{supplier.SupplierId},{address.AddressId ?? 0},{address.LocationCode},{(address.Active ? "true" : "false")},{address.StateIsoCode},{"SYSTEM"},{DateTime.UtcNow},{"SYSTEM"},{DateTime.UtcNow},{"NEW"})", cancellationToken);
        }

        foreach (var address in supplier.RemitToAddresses)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO xxfin.xxfin_coupa_supp_remit_to_adds
                (STG_ID,SOURCE,SUPPLIER_ID,REMIT_TO_ADDRESS_ID,REMIT_TO_CODE,NAME,ACTIVE,STREET1,STREET2,CITY,STATE,POSTAL_CODE,COUNTRY_CODE,COUNTRY_NAME,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,INTEGRATION_STATUS)
                VALUES ({stgId},{supplier.Source},{supplier.SupplierId},{address.AddressId},{address.RemitToCode},{address.Name},{(address.Active ? "true" : "false")},{address.Street1},{address.Street2},{address.City},{address.State},{address.PostalCode},{address.Country?.Code},{address.Country?.Name},{"SYSTEM"},{DateTime.UtcNow},{"SYSTEM"},{DateTime.UtcNow},{"NEW"})", cancellationToken);
        }
    }

    private static CoupaSupplierRow ToSupplierRow(SupplierAggregate supplier, long stgId) => new()
    {
        StgId = stgId,
        Source = supplier.Source,
        SupplierId = supplier.SupplierId,
        Name = supplier.Name,
        Status = supplier.Status,
        CreationDate = DateTime.UtcNow,
        CreatedBy = "SYSTEM",
        LastUpdateDate = DateTime.UtcNow,
        LastUpdatedBy = "SYSTEM",
        IntegrationStatus = "NEW"
    };

    private async Task<long> GetNextSequenceValueAsync(string sequenceName, CancellationToken cancellationToken)
    {
        await using var connection = new OracleConnection(configuration.GetConnectionString("OraclePrimary"));
        await connection.OpenAsync(cancellationToken);
        return await GetNextSequenceValueFromConnectionAsync(sequenceName, connection, cancellationToken);
    }

    private static async Task<long> GetNextSequenceValueFromConnectionAsync(string sequenceName, OracleConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(value);
    }
}
