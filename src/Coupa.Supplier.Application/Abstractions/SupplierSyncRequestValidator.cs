using Coupa.Supplier.Application.DTOs;
using FluentValidation;

namespace Coupa.Supplier.Application.Abstractions;

public sealed class SupplierSyncRequestValidator : AbstractValidator<SyncSuppliersRequest>
{
    public SupplierSyncRequestValidator()
    {
        RuleFor(x => x.BatchSize).GreaterThan(0).When(x => x.BatchSize.HasValue);
        RuleFor(x => x.DegreeOfParallelism).InclusiveBetween(1, 32);
    }
}
