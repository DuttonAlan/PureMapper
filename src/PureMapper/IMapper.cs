namespace PureMapper;

/// <summary>
/// Maps objects from one type to another using the maps defined in a <see cref="MapperConfiguration"/>.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps <paramref name="source"/> to a new <typeparamref name="TDestination"/>, using the runtime type of the source.
    /// </summary>
    TDestination Map<TDestination>(object? source);

    /// <summary>
    /// Maps <paramref name="source"/> to a new <typeparamref name="TDestination"/>.
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Maps <paramref name="source"/> onto an existing <paramref name="destination"/> and returns it.
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    /// <summary>
    /// Maps <paramref name="source"/> to a new instance of <paramref name="destinationType"/>.
    /// </summary>
    object? Map(object? source, Type destinationType);
}
