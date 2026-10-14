using System.Linq.Expressions;

namespace PureMapper;

/// <summary>
/// Fluent configuration for a map from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
/// </summary>
public interface IMappingExpression<TSource, TDestination>
{
    /// <summary>
    /// Customizes how a destination member is mapped, e.g. <c>ForMember(d =&gt; d.FullName, o =&gt; o.MapFrom(s =&gt; s.First + " " + s.Last))</c>.
    /// </summary>
    IMappingExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource>> options);

    /// <summary>
    /// Customizes how a destination member is mapped, identified by name.
    /// </summary>
    IMappingExpression<TSource, TDestination> ForMember(
        string destinationMemberName,
        Action<IMemberConfigurationExpression<TSource>> options);

    /// <summary>
    /// Uses <paramref name="factory"/> to create the destination instead of a constructor.
    /// </summary>
    IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> factory);

    /// <summary>
    /// Runs <paramref name="action"/> after the destination is created but before any member is mapped.
    /// </summary>
    IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> action);

    /// <summary>
    /// Runs <paramref name="action"/> after all members have been mapped.
    /// </summary>
    IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> action);

    /// <summary>
    /// Creates a convention-based map in the opposite direction and returns it for further configuration.
    /// </summary>
    IMappingExpression<TDestination, TSource> ReverseMap();
}
