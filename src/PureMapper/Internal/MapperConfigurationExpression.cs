using System.Reflection;

namespace PureMapper.Internal;

internal sealed class MapperConfigurationExpression : Profile, IMapperConfigurationExpression
{
    private readonly List<Profile> _profiles = [];

    public void AddProfile(Profile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        _profiles.Add(profile);
    }

    public void AddProfile<TProfile>() where TProfile : Profile, new() => AddProfile(new TProfile());

    public void AddMaps(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        var profileTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false, IsGenericTypeDefinition: false }
                           && type.IsSubclassOf(typeof(Profile))
                           && type.GetConstructor(Type.EmptyTypes) is not null);

        foreach (var profileType in profileTypes)
        {
            AddProfile((Profile)Activator.CreateInstance(profileType)!);
        }
    }

    public IEnumerable<TypeMapConfiguration> GetAllTypeMapConfigurations() =>
        TypeMapConfigurations.Concat(_profiles.SelectMany(profile => profile.TypeMapConfigurations));
}
