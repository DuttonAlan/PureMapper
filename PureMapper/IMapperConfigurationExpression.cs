using System.Reflection;

namespace PureMapper;

/// <summary>
/// Entry point for configuring maps when creating a <see cref="MapperConfiguration"/>.
/// </summary>
public interface IMapperConfigurationExpression
{
    /// <summary>
    /// Creates a map from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
    /// </summary>
    IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();

    /// <summary>
    /// Adds the maps defined in <paramref name="profile"/>.
    /// </summary>
    void AddProfile(Profile profile);

    /// <summary>
    /// Adds the maps defined in a new <typeparamref name="TProfile"/>.
    /// </summary>
    void AddProfile<TProfile>() where TProfile : Profile, new();

    /// <summary>
    /// Adds every concrete <see cref="Profile"/> with a parameterless constructor found in <paramref name="assemblies"/>.
    /// </summary>
    void AddMaps(params Assembly[] assemblies);
}
