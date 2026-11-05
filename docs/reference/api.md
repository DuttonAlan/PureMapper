# API

Every public type of the `PureMapper` package. All are in the namespace `PureMapper`, except `ServiceCollectionExtensions`.

## `MapperConfiguration`

```csharp
public sealed class MapperConfiguration
```

Holds all maps. Immutable and thread-safe once created.

| Member | Description |
|---|---|
| `MapperConfiguration(Action<IMapperConfigurationExpression> configure)` | Runs `configure` and builds all maps. Throws `ArgumentNullException` and, [for an invalid map](exceptions.md#when-the-configuration-is-created), `MappingConfigurationException`. |
| `IMapper CreateMapper()` | Returns a new `Mapper` for this configuration. |
| `void AssertConfigurationIsValid()` | Throws `MappingConfigurationException` listing [every problem found](exceptions.md#when-validating). |

## `IMapperConfigurationExpression`

```csharp
public interface IMapperConfigurationExpression
```

The `cfg` parameter of the `MapperConfiguration` constructor and of `AddPureMapper`.

| Member | Description |
|---|---|
| `IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()` | Creates a map. Each pair of types can be mapped once. |
| `void AddProfile(Profile profile)` | Adds the maps of an existing profile instance. |
| `void AddProfile<TProfile>() where TProfile : Profile, new()` | Creates a profile and adds its maps. |
| `void AddMaps(params Assembly[] assemblies)` | Adds every non-abstract, non-generic `Profile` with a public parameterless constructor, of any visibility, found in `assemblies`. Others are skipped. |

## `IMappingExpression<TSource, TDestination>`

```csharp
public interface IMappingExpression<TSource, TDestination>
```

Returned by `CreateMap`. All methods except `ReverseMap` return the same expression for chaining.

| Member | Description |
|---|---|
| `ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSource>> options)` | Configures the destination member selected by the lambda. It has to select a property directly, or `ArgumentException` is thrown. |
| `ForMember(string destinationMemberName, Action<IMemberConfigurationExpression<TSource>> options)` | Configures a member by name, not case-sensitive. Throws `MappingConfigurationException` if the destination has no public readable property with this name. |
| `ConstructUsing(Func<TSource, TDestination> factory)` | Uses `factory` to create the destination. Not called when mapping onto an existing object. Members are still mapped afterwards. |
| `BeforeMap(Action<TSource, TDestination> action)` | Runs `action` after the destination is created and before its properties are mapped. |
| `AfterMap(Action<TSource, TDestination> action)` | Runs `action` after all properties are mapped. |
| `IMappingExpression<TDestination, TSource> ReverseMap()` | Creates the map for the opposite direction by convention, and returns it. Nothing is inherited from this map. |

## `IMemberConfigurationExpression<TSource>`

```csharp
public interface IMemberConfigurationExpression<TSource>
```

The `o` parameter of `ForMember`.

| Member | Description |
|---|---|
| `void MapFrom<TSourceMember>(Func<TSource, TSourceMember> resolver)` | Takes the value from `resolver` instead of a matching source property. |
| `void Ignore()` | Skips the member. A constructor parameter gets its declared default, or otherwise the default of its type. Takes precedence over the other options. |
| `void Condition(Func<TSource, bool> condition)` | Maps the member only if `condition` returns `true`. If not, a property keeps its value and a constructor parameter gets the default of its type. |
| `void NullSubstitute(object? value)` | Uses `value` if the source value is `null`. The value is then mapped to the member's type. |

## `Profile`

```csharp
public abstract class Profile
```

Base class for grouping maps. Call `CreateMap` in the constructor of your derived class.

| Member | Description |
|---|---|
| `IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()` | Creates a map in this profile. |

## `IMapper`

```csharp
public interface IMapper
```

Maps objects. All overloads look for a map using the [runtime type of the source](mapping-rules.md#finding-the-map).

| Member | Description |
|---|---|
| `TDestination Map<TDestination>(object? source)` | Maps to a new `TDestination`. If `source` is `null`, returns the default of `TDestination`. |
| `TDestination Map<TSource, TDestination>(TSource source)` | The same, with the source type stated explicitly. |
| `TDestination Map<TSource, TDestination>(TSource source, TDestination destination)` | Maps onto `destination` and returns it. If `source` is `null`, returns `destination` unchanged. If there is no map for the pair, [`destination` is not used](mapping-rules.md#order-of-operations). |
| `object? Map(object? source, Type destinationType)` | Maps to a new instance of `destinationType`. Throws `ArgumentNullException` if `destinationType` is `null`. |

> **Calling the `Type` overload:** it is only chosen if the argument for `source` has the static type `object`. For any other type, C# prefers the overload with `TDestination destination`, which then looks for a map to `System.Type` and fails. Write `mapper.Map((object)order, typeof(OrderDto))`.

## `Mapper`

```csharp
public sealed class Mapper(MapperConfiguration configuration) : IMapper
```

The default implementation of `IMapper`. It has no state of its own, so it is thread-safe and creating one is cheap. Throws `ArgumentNullException` if `configuration` is `null`.

## `ServiceCollectionExtensions`

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
```

| Member | Description |
|---|---|
| `IServiceCollection AddPureMapper(this IServiceCollection services, params Assembly[] assemblies)` | Registers the profiles found in `assemblies`. |
| `IServiceCollection AddPureMapper(this IServiceCollection services, Action<IMapperConfigurationExpression>? configure, params Assembly[] assemblies)` | Runs `configure`, then adds the profiles found in `assemblies`. |

Both overloads:

- build the `MapperConfiguration` immediately, so it [throws here](exceptions.md#when-the-configuration-is-created) for an invalid map;
- register `MapperConfiguration` and `IMapper` as singletons;
- throw `InvalidOperationException` if `MapperConfiguration` is already registered;
- do not call `AssertConfigurationIsValid`.

## Exceptions

See [Exceptions](exceptions.md) for `MappingConfigurationException` and `MappingException`.
