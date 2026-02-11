namespace Coupa.InvoiceSync.Api.Infrastructure.Options;

/// <summary>
/// Defines configurable source-to-target mapping profiles.
/// </summary>
public sealed class MappingOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "Mapping";

    /// <summary>
    /// Gets or sets named profiles for source-to-target mapping.
    /// </summary>
    public Dictionary<string, MappingProfile> Profiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Represents one mapping profile containing field-level mappings.
/// </summary>
public sealed class MappingProfile
{
    /// <summary>
    /// Gets or sets the profile name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets configured field mappings.
    /// </summary>
    public List<FieldMapping> FieldMappings { get; set; } = [];
}

/// <summary>
/// Represents a source field to target field mapping.
/// </summary>
public sealed class FieldMapping
{
    /// <summary>
    /// Gets or sets source field name from Coupa payload.
    /// </summary>
    public string SourceField { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets target field name for canonical model.
    /// </summary>
    public string TargetField { get; set; } = string.Empty;
}
