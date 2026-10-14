using System.Collections;
using System.Globalization;
using PureMapper.Internal;

namespace PureMapper;

/// <summary>
/// Default <see cref="IMapper"/> implementation. Stateless and thread-safe.
/// </summary>
public sealed class Mapper(MapperConfiguration configuration) : IMapper
{
    private readonly MapperConfiguration _configuration =
        configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <inheritdoc />
    public TDestination Map<TDestination>(object? source) =>
        MapCore(source, typeof(TDestination), null) is TDestination result ? result : default!;

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(TSource source) =>
        MapCore(source, typeof(TDestination), null) is TDestination result ? result : default!;

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) =>
        MapCore(source, typeof(TDestination), destination) is TDestination result ? result : default!;

    /// <inheritdoc />
    public object? Map(object? source, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(destinationType);
        return MapCore(source, destinationType, null);
    }

    private object? MapCore(object? source, Type destinationType, object? destination)
    {
        if (source is null)
        {
            return destination ?? TypeHelper.GetDefaultValue(destinationType);
        }

        var typeMap = _configuration.FindTypeMap(source.GetType(), destinationType);
        return typeMap is not null
            ? MapObject(source, typeMap, destination)
            : ConvertValue(source, destinationType);
    }

    private object MapObject(object source, TypeMap typeMap, object? destination)
    {
        destination ??= CreateDestination(source, typeMap);

        foreach (var beforeMap in typeMap.BeforeMaps)
        {
            beforeMap(source, destination);
        }

        foreach (var member in typeMap.PropertyMaps)
        {
            if (member.Condition is not null && !member.Condition(source))
            {
                continue;
            }

            try
            {
                member.Setter!(destination, MapMember(member, source));
            }
            catch (Exception ex) when (ex is not MappingException)
            {
                throw new MappingException(
                    $"Error mapping {typeMap.SourceType.GetFriendlyName()} -> " +
                    $"{typeMap.DestinationType.GetFriendlyName()}.{member.Name}: {ex.Message}", ex);
            }
        }

        foreach (var afterMap in typeMap.AfterMaps)
        {
            afterMap(source, destination);
        }

        return destination;
    }

    private object CreateDestination(object source, TypeMap typeMap)
    {
        if (typeMap.CustomConstructor is not null)
        {
            return typeMap.CustomConstructor(source);
        }

        if (typeMap.Factory is not null)
        {
            return typeMap.Factory();
        }

        var arguments = typeMap.ConstructorParameters
            .Select(parameter => parameter.Condition is null || parameter.Condition(source)
                ? MapMember(parameter, source)
                : TypeHelper.GetDefaultValue(parameter.DestinationType))
            .ToArray();

        return typeMap.Constructor!.Invoke(arguments);
    }

    private object? MapMember(MemberMap member, object source)
    {
        var value = member.Resolver(source);
        if (value is null && member.HasNullSubstitute)
        {
            value = member.NullSubstitute;
        }

        return MapCore(value, member.DestinationType, null);
    }

    private object? ConvertValue(object source, Type destinationType)
    {
        if (TryMapCollection(source, destinationType, out var collection))
        {
            return collection;
        }

        var targetType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

        if (targetType.IsInstanceOfType(source))
        {
            return source;
        }

        if (targetType == typeof(string))
        {
            return Convert.ToString(source, CultureInfo.InvariantCulture);
        }

        if (targetType.IsEnum)
        {
            return source is string or Enum
                ? Enum.Parse(targetType, source.ToString()!, ignoreCase: true)
                : Enum.ToObject(targetType, source);
        }

        if (source is IConvertible && TypeHelper.IsConvertible(targetType))
        {
            return Convert.ChangeType(source, targetType, CultureInfo.InvariantCulture);
        }

        throw new MappingException(
            $"No map from {source.GetType().GetFriendlyName()} to {destinationType.GetFriendlyName()} is configured. " +
            $"Add CreateMap<{source.GetType().GetFriendlyName()}, {destinationType.GetFriendlyName()}>().");
    }

    private bool TryMapCollection(object source, Type destinationType, out object? result)
    {
        result = null;
        var elementType = TypeHelper.GetElementType(destinationType);
        if (elementType is null || source is string || source is not IEnumerable items)
        {
            return false;
        }

        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
        foreach (var item in items)
        {
            list.Add(MapCore(item, elementType, null));
        }

        if (destinationType.IsArray)
        {
            var array = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(array, 0);
            result = array;
        }
        else if (destinationType.IsInstanceOfType(list))
        {
            // Covers List<T>, IEnumerable<T>, IList<T>, ICollection<T>, IReadOnlyList<T>, ...
            result = list;
        }
        else
        {
            // Other collections (HashSet<T>, Collection<T>, ...) usually accept an IEnumerable<T> in their constructor.
            try
            {
                result = Activator.CreateInstance(destinationType, list);
            }
            catch (MissingMethodException ex)
            {
                throw new MappingException(
                    $"Cannot create collection {destinationType.GetFriendlyName()}: it needs a public constructor " +
                    $"that accepts IEnumerable<{elementType.GetFriendlyName()}>.", ex);
            }
        }

        return true;
    }
}
