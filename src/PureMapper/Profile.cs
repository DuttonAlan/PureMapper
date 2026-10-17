using PureMapper.Internal;

namespace PureMapper;

/// <summary>
/// Groups related maps. Derive from it and call <see cref="CreateMap{TSource, TDestination}"/> in the constructor.
/// </summary>
public abstract class Profile
{
    private readonly List<TypeMapConfiguration> _typeMapConfigurations = [];

    internal IReadOnlyList<TypeMapConfiguration> TypeMapConfigurations => _typeMapConfigurations;

    /// <summary>
    /// Creates a map from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
    /// </summary>
    public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var expression = new MappingExpression<TSource, TDestination>(this);
        _typeMapConfigurations.Add(expression);
        return expression;
    }
}
