# Changelog

All notable changes to PureMapper are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-10-14

The first public release of PureMapper: a small, convention-based object-to-object mapper for .NET 10.

### Added

#### Mapping conventions

- Properties are matched by name, case-insensitively, without any configuration.
- **Flattening**: `Customer.Address.City` maps to `CustomerAddressCity`; a `null` along the path yields `null`
  instead of an exception.
- **Nested objects** are mapped through their own `CreateMap`.
- **Collections**: arrays, `List<T>`, `IEnumerable<T>`, `IList<T>`, `IReadOnlyList<T>`, `HashSet<T>` and other
  collections with an `IEnumerable<T>` constructor.
- **Enums** are mapped by name, from strings and from numbers.
- **Type conversions** between simple types via `Convert.ChangeType` (invariant culture), and any value to `string`.
- **Records and immutable types**: types without a parameterless constructor are created through their largest
  public constructor, matching parameters by name. `init`-only properties are supported.
- **Inheritance**: a map for a base class or interface is also used for derived source types.

#### Configuration

- `MapperConfiguration` with `CreateMap<TSource, TDestination>()`.
- `ForMember` options: `MapFrom`, `Ignore`, `Condition` and `NullSubstitute`.
- `ConstructUsing`, `BeforeMap`, `AfterMap` and `ReverseMap`.
- `Profile` base class, `AddProfile` and assembly scanning with `AddMaps`.
- `AssertConfigurationIsValid()` reports unmapped members and missing nested maps at startup.

#### Mapping API

- `IMapper.Map<TDestination>(source)`, `Map<TSource, TDestination>(source)`, mapping onto an existing object
  with `Map(source, destination)`, and the non-generic `Map(source, destinationType)`.
- Clear exceptions: `MappingConfigurationException` for configuration errors and `MappingException` for runtime
  errors, including which member failed.

#### Dependency injection

- `services.AddPureMapper(...)` registers `MapperConfiguration` and `IMapper` as singletons, from profiles in
  assemblies, inline configuration, or both.

#### Performance

- Property getters, setters and constructors are compiled once when the configuration is created.
- The configuration and the mapper are immutable and thread-safe.

[Unreleased]: https://github.com/DuttonAlan/PureMapper/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/DuttonAlan/PureMapper/releases/tag/v1.0.0
