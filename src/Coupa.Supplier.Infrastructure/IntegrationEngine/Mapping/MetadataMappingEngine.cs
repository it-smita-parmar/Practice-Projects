using System.Text;
using System.Text.Json.Nodes;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Mapping;

public sealed class MetadataMappingEngine(ISequenceGenerator sequenceGenerator) : IMappingEngine
{
    public Dictionary<string, TableBatchData> Transform(JsonNode sourceJson, MappingConfiguration mappingConfig)
    {
        var result = new Dictionary<string, TableBatchData>(StringComparer.OrdinalIgnoreCase);
        var orderedTables = OrderTables(mappingConfig.Tables);
        var parentKeys = new Dictionary<string, List<object?>>();

        foreach (var table in orderedTables)
        {
            var selectedNodes = EvaluatePath(sourceJson, table.CollectionPath).ToList();
            if (selectedNodes.Count == 0 && string.Equals(table.CollectionPath, "$", StringComparison.Ordinal))
            {
                selectedNodes.Add(sourceJson);
            }

            var batchData = new TableBatchData { TableName = table.TableName };

            foreach (var column in table.Fields.Select(x => x.TargetColumn).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                batchData.ColumnData[column] = [];
            }

            if (!string.IsNullOrWhiteSpace(table.ParentKeyColumn))
            {
                batchData.ColumnData.TryAdd(table.ParentKeyColumn!, []);
            }

            if (!string.IsNullOrWhiteSpace(table.PrimaryKey))
            {
                batchData.ColumnData.TryAdd(table.PrimaryKey!, []);
            }

            foreach (var node in selectedNodes)
            {
                foreach (var field in table.Fields)
                {
                    var value = EvaluateValue(node, field.SourcePath);
                    if (field.IsRequired && value is null)
                    {
                        throw new InvalidOperationException($"Required mapping value missing for {field.TargetColumn} in table {table.TableName}");
                    }

                    batchData.ColumnData[field.TargetColumn].Add(Coerce(value, field));
                }

                if (!string.IsNullOrWhiteSpace(table.ParentAlias) && !string.IsNullOrWhiteSpace(table.ParentKeyColumn))
                {
                    var parentAlias = table.ParentAlias!;
                    var parentKey = parentKeys.TryGetValue(parentAlias, out var keyValues) && keyValues.Count > 0
                        ? keyValues[batchData.RowCount % keyValues.Count]
                        : null;
                    batchData.ColumnData[table.ParentKeyColumn!].Add(parentKey);
                }

                if (!string.IsNullOrWhiteSpace(table.PrimaryKey))
                {
                    var generated = EvaluateValue(node, table.GeneratePrimaryKey ?? $"guid()", true);
                    batchData.ColumnData[table.PrimaryKey!].Add(generated);
                }

                batchData.RowCount++;
            }

            if (!string.IsNullOrWhiteSpace(table.Alias) && !string.IsNullOrWhiteSpace(table.PrimaryKey) && batchData.ColumnData.TryGetValue(table.PrimaryKey!, out var keys))
            {
                parentKeys[table.Alias] = keys;
            }

            result[table.TableName] = batchData;
        }

        return result;
    }

    private object? EvaluateValue(JsonNode? node, string expressionOrPath, bool allowExpressionOnly = false)
    {
        if (string.IsNullOrWhiteSpace(expressionOrPath))
        {
            return null;
        }

        if (IsExpression(expressionOrPath))
        {
            return EvaluateExpression(node, expressionOrPath);
        }

        if (allowExpressionOnly)
        {
            return null;
        }

        return EvaluatePath(node, expressionOrPath).FirstOrDefault()?.GetValue<object?>();
    }

    private static bool IsExpression(string value) => value.Contains('(') && value.EndsWith(')');

    private object? EvaluateExpression(JsonNode? node, string expression)
    {
        if (expression.StartsWith("constant(", StringComparison.OrdinalIgnoreCase))
        {
            return TrimArg(expression);
        }

        if (expression.StartsWith("now(", StringComparison.OrdinalIgnoreCase))
        {
            return DateTime.UtcNow;
        }

        if (expression.StartsWith("guid(", StringComparison.OrdinalIgnoreCase))
        {
            return Guid.NewGuid().ToString("N");
        }

        if (expression.StartsWith("sequence(", StringComparison.OrdinalIgnoreCase) || expression.StartsWith("sequence:", StringComparison.OrdinalIgnoreCase))
        {
            var sequenceName = expression.Contains(':')
                ? expression[(expression.IndexOf(':') + 1)..].Trim()
                : TrimArg(expression);
            return sequenceGenerator.Next(sequenceName);
        }

        if (expression.StartsWith("upper(", StringComparison.OrdinalIgnoreCase))
        {
            var inner = TrimArg(expression);
            var raw = EvaluateValue(node, inner)?.ToString();
            return raw?.ToUpperInvariant();
        }

        if (expression.StartsWith("concat(", StringComparison.OrdinalIgnoreCase))
        {
            var args = SplitArguments(TrimArg(expression));
            var sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.Append(EvaluateValue(node, arg)?.ToString());
            }

            return sb.ToString();
        }

        return null;
    }

    private static string TrimArg(string expression)
    {
        var start = expression.IndexOf('(');
        if (start < 0)
        {
            return expression.Replace("'", string.Empty, StringComparison.Ordinal).Trim();
        }

        var content = expression[(start + 1)..^1].Trim();
        return content.Trim('\'', '"');
    }

    private static IReadOnlyCollection<string> SplitArguments(string args)
        => args.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim('\'', '"'))
            .ToArray();

    private static IEnumerable<JsonNode> EvaluatePath(JsonNode? node, string path)
    {
        if (node is null)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(path) || path == "$")
        {
            return [node];
        }

        var normalized = path.StartsWith("$.", StringComparison.Ordinal) ? path[2..] : path.TrimStart('$');
        var segments = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);

        IEnumerable<JsonNode> current = [node];
        foreach (var segment in segments)
        {
            var isArray = segment.EndsWith("[*]", StringComparison.Ordinal);
            var propertyName = isArray ? segment[..^3] : segment;
            var next = new List<JsonNode>();

            foreach (var item in current)
            {
                if (item is not JsonObject obj || !obj.TryGetPropertyValue(propertyName, out var child) || child is null)
                {
                    continue;
                }

                if (isArray)
                {
                    if (child is JsonArray array)
                    {
                        foreach (var element in array)
                        {
                            if (element is not null)
                            {
                                next.Add(element);
                            }
                        }
                    }
                }
                else
                {
                    next.Add(child);
                }
            }

            current = next;
        }

        return current;
    }

    private static object? Coerce(object? value, FieldMappingConfiguration field)
    {
        if (value is null)
        {
            return DBNull.Value;
        }

        return field.DataType.ToLowerInvariant() switch
        {
            "number" => long.TryParse(value.ToString(), out var l) ? l : value,
            "datetime" => DateTime.TryParse(value.ToString(), out var dt) ? dt : value,
            "string" => ApplyLength(value.ToString() ?? string.Empty, field.MaxLength),
            _ => value
        };
    }

    private static string ApplyLength(string value, int? maxLength)
        => maxLength.HasValue && value.Length > maxLength.Value ? value[..maxLength.Value] : value;

    private static IReadOnlyCollection<TableMappingConfiguration> OrderTables(IReadOnlyCollection<TableMappingConfiguration> tables)
    {
        var ordered = new List<TableMappingConfiguration>();
        var pending = new List<TableMappingConfiguration>(tables);

        while (pending.Count > 0)
        {
            var next = pending.FirstOrDefault(t => string.IsNullOrWhiteSpace(t.ParentAlias)
                                                   || ordered.Any(x => x.Alias.Equals(t.ParentAlias, StringComparison.OrdinalIgnoreCase)));

            if (next is null)
            {
                throw new InvalidOperationException("Table mapping contains cyclic parent dependencies.");
            }

            ordered.Add(next);
            pending.Remove(next);
        }

        return ordered;
    }
}
