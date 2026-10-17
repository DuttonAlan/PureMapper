using System.Collections.Concurrent;
using PureMapper.Internal;

namespace PureMapper;

/// <summary>
/// Holds all configured maps. Create it once at startup and share it; it is immutable and thread-safe.
/// </summary>
public sealed class MapperConfiguration
{
    private readonly Dictionary<(Type Source, Type Destination), TypeMap> _typeMaps = [];
    private readonly ConcurrentDictionary<(Type Source, Type Destination), TypeMap?> _resolvedTypeMaps = new();

    /// <summary>
    /// Creates a configuration from the maps defined in <paramref name="configure"/>.
    /// </summary>
    public MapperConfiguration(Action<IMapperConfigurationExpression> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var expression = new MapperConfigurationExpression();
        configure(expression);

        foreach (var configuration in expression.GetAllTypeMapConfigurations())
        {
            var typeMap = configuration.Build();
            if (!_typeMaps.TryAdd((typeMap.SourceType, typeMap.DestinationType), typeMap))
            {
                throw new MappingConfigurationException(
                    $"A map from {typeMap.SourceType.GetFriendlyName()} to {typeMap.DestinationType.GetFriendlyName()} is defined more than once.");
            }
        }
    }

    /// <summary>
    /// Creates a mapper that uses this configuration.
    /// </summary>
    public IMapper CreateMapper() => new Mapper(this);

    /// <summary>
    /// Throws a <see cref="MappingConfigurationException"/> listing every destination member that has no source
    /// and every member whose type cannot be mapped (usually a missing nested <c>CreateMap</c>).
    /// </summary>
    public void AssertConfigurationIsValid()
    {
        var errors = new List<string>();

        foreach (var typeMap in _typeMaps.Values)
        {
            var prefix = $"{typeMap.SourceType.GetFriendlyName()} -> {typeMap.DestinationType.GetFriendlyName()}";

            errors.AddRange(typeMap.UnmappedMembers.Select(member => $"{prefix}: unmapped {member}."));

            foreach (var member in typeMap.ConstructorParameters.Concat(typeMap.PropertyMaps))
            {
                if (!CanMap(member.SourceType, member.DestinationType))
                {
                    errors.Add($"{prefix}: cannot map '{member.Name}' from {member.SourceType.GetFriendlyName()} " +
                               $"to {member.DestinationType.GetFriendlyName()}. Is a CreateMap missing?");
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new MappingConfigurationException(
                "The mapping configuration is invalid:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
        }
    }

    internal TypeMap? FindTypeMap(Type sourceType, Type destinationType) =>
        _resolvedTypeMaps.GetOrAdd((sourceType, destinationType), key =>
        {
            // Walk the source hierarchy so a map for a base class or interface also applies to derived types.
            for (var type = key.Source; type is not null; type = type.BaseType)
            {
                if (_typeMaps.TryGetValue((type, key.Destination), out var typeMap))
                {
                    return typeMap;
                }
            }

            return key.Source.GetInterfaces()
                .Select(i => _typeMaps.GetValueOrDefault((i, key.Destination)))
                .FirstOrDefault(m => m is not null);
        });

    private bool CanMap(Type sourceType, Type destinationType)
    {
        // The runtime type is unknown (e.g. a MapFrom returning object), so it can only be checked when mapping.
        if (sourceType == typeof(object) || FindTypeMap(sourceType, destinationType) is not null)
        {
            return true;
        }

        var sourceElement = TypeHelper.GetElementType(sourceType);
        var destinationElement = TypeHelper.GetElementType(destinationType);
        if (sourceElement is not null && destinationElement is not null)
        {
            return CanMap(sourceElement, destinationElement);
        }

        var source = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
        var destination = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

        return destination.IsAssignableFrom(source)
               || destination == typeof(string)
               || (destination.IsEnum && (source.IsEnum || source == typeof(string) || TypeHelper.IsConvertible(source)))
               || (TypeHelper.IsConvertible(source) && TypeHelper.IsConvertible(destination));
    }
}
