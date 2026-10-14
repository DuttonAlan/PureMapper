using System.Reflection;
using PureMapper;

// Same namespace as AddLogging, AddOptions, ... so AddPureMapper shows up without an extra using.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers PureMapper in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="MapperConfiguration"/> and <see cref="IMapper"/> using every
    /// <see cref="Profile"/> found in <paramref name="assemblies"/>.
    /// </summary>
    /// <example><c>services.AddPureMapper(typeof(Program).Assembly);</c></example>
    public static IServiceCollection AddPureMapper(this IServiceCollection services, params Assembly[] assemblies) =>
        services.AddPureMapper(configure: null, assemblies);

    /// <summary>
    /// Registers a singleton <see cref="MapperConfiguration"/> and <see cref="IMapper"/> built by
    /// <paramref name="configure"/>, plus every <see cref="Profile"/> found in <paramref name="assemblies"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">PureMapper is already registered.</exception>
    public static IServiceCollection AddPureMapper(
        this IServiceCollection services,
        Action<IMapperConfigurationExpression>? configure,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        // A second call would silently be ignored or replace the first configuration, so fail loudly instead.
        if (services.Any(descriptor => descriptor.ServiceType == typeof(MapperConfiguration)))
        {
            throw new InvalidOperationException(
                "PureMapper is already registered. Call AddPureMapper once and pass all assemblies and maps to it.");
        }

        // Build the configuration now so configuration errors surface at startup, not on first resolve.
        var configuration = new MapperConfiguration(cfg =>
        {
            configure?.Invoke(cfg);
            cfg.AddMaps(assemblies);
        });

        services.AddSingleton(configuration);
        services.AddSingleton<IMapper>(new Mapper(configuration));
        return services;
    }
}
