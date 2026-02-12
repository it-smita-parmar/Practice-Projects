namespace Coupa.Supplier.Shared.Models;

public sealed record OperationResult(bool Success, string? Message = null)
{
    public static OperationResult Ok(string? message = null) => new(true, message);
    public static OperationResult Fail(string message) => new(false, message);
}
