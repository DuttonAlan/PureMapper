# Mapping rules

The exact rules PureMapper follows. All name comparisons are case-insensitive.

## Order of operations

For each object being mapped:

1. **Null check.** If the source is `null`, the result is the destination passed in, or otherwise the default of the destination type. Nothing else runs.
2. **Find the map**, see [below](#finding-the-map). If there is none, the value goes to [value conversion](#value-conversion), and a destination passed in is not used.
3. **Create the destination**, see [below](#creating-the-destination), unless one was passed in.
4. Run the `BeforeMap` actions, in the order they were added.
5. For each property in the map:
   1. If there is a `Condition` and it is false, skip the property.
   2. Get the value from `MapFrom` or the matched source path.
   3. If the value is `null` and there is a `NullSubstitute`, use the substitute.
   4. Map the value to the property's type, starting again at step 1.
   5. Set the property.
6. Run the `AfterMap` actions, in the order they were added.

## Finding the map

A map is looked up with the **runtime type of the source** and the **exact destination type** requested, in this order:

1. A map for the source type itself.
2. A map for each base class, starting with the nearest.
3. A map for an interface the source type implements. If several match, which one is used is not defined.

Derived destination types are never considered, and neither `Nullable<T>` nor `T` stands in for the other: a map to `Guid` is not used for a `Guid?` member.

## Which members are mapped

Destination members are:

- public instance properties with a public setter or `init`, without an index parameter, and
- with [constructor creation](#creating-the-destination), the parameters of the constructor. A property with the same name as a parameter is not set again.

These are **never** mapped, and validation does not report them: get-only properties, properties with a non-public setter, and fields.

## Finding the source of a member

The first rule that applies gives the source of a member:

1. `Ignore`: no source.
2. `MapFrom`: the given function.
3. A public, readable source property with the same name.
4. Flattening: a source property whose name is a prefix of the member's name, followed by these same rules 3 and 4 on the type of that property, for the rest of the name. Longer prefixes are tried first.
5. Nothing: the member is *unmapped*. It is skipped when mapping, and reported by validation.

For example, the member `CustomerAddressCity` matches `CustomerAddressCity`, otherwise `Customer.AddressCity`, otherwise `Customer.Address.City`.

If an object along a flattened path is `null`, the value is `null`.

Only properties can be sources; fields and methods cannot.

## Creating the destination

The first rule that applies is used:

1. `ConstructUsing`, if configured. Constructor parameters are then not considered. Properties are still mapped afterwards.
2. For a value type, its default value. Constructors with parameters are not used.
3. A public parameterless constructor.
4. The public constructor with the most parameters. Parameters are filled in as [members](#finding-the-source-of-a-member), and each value is [mapped to the parameter's type](#value-conversion). If several have the same number of parameters, the first one reflection returns is used.

If none applies, creating the configuration throws a `MappingConfigurationException`. An abstract class with a public constructor also fails when creating the configuration, with an `InvalidOperationException`.

Values for constructor parameters:

| Situation | Value |
|---|---|
| Has a source | The source value, with `NullSubstitute` applied |
| `Condition` is false | The default of the parameter's type, not its declared default |
| No source or ignored, declared default | The declared default |
| No source or ignored, no declared default | The default of the parameter's type. Without `Ignore`, validation reports it. |

## Value conversion

When no map applies, the first rule that matches is used. `Nullable<T>` destinations are treated as `T`.

| # | Source | Destination | Result |
|---|---|---|---|
| 1 | Enumerable, except `string` | Array or `IEnumerable<T>` | A new [collection](#collections) |
| 2 | Any | A type it can be assigned to | The same instance |
| 3 | Any | `string` | `Convert.ToString` with the invariant culture. For an enum, its name. |
| 4 | `string` or enum | Enum | `Enum.Parse` by name, not case-sensitive. Also accepts a numeric string. Throws `ArgumentException` for an unknown name. |
| 5 | Other | Enum | `Enum.ToObject`. The value is not checked, so undefined values are possible. |
| 6 | `IConvertible` | `IConvertible` type | `Convert.ChangeType` with the invariant culture |
| 7 | Any | Any | Throws `MappingException`: `No map from A to B is configured.` |

Rule 2 means that an object with the same class on both sides and no map is shared, not copied.

Rule 6 can fail for a specific value or pair of types, with a `FormatException`, `InvalidCastException` or `OverflowException`. Numbers are rounded when converted to integers, following `Convert`: `2.5m` to `int` gives `2`. These common types do not implement `IConvertible`, and so need a [conversion of your own](../how-to/convert-between-types.md): `Guid`, `DateOnly`, `TimeOnly`, `DateTimeOffset`, `TimeSpan`.

## Collections

A destination counts as a collection if it is an array or implements `IEnumerable<T>`, except `string`. The source has to be enumerable. A map for the pair of types takes precedence over these rules.

Each element is mapped to `T`, then the result is created as follows:

| Destination | Result |
|---|---|
| Array | A new array |
| A type `List<T>` can be assigned to: `List<T>`, `IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>` | A new `List<T>` |
| Other type with a public constructor accepting `IEnumerable<T>`: e.g. `HashSet<T>`, `Queue<T>`, `LinkedList<T>`, `Collection<T>`, `ReadOnlyCollection<T>` (through `IList<T>`), `Dictionary<TKey, TValue>` | A new instance from that constructor |
| Others: e.g. `ISet<T>`, `IDictionary<TKey, TValue>`, `ImmutableList<T>` | Throws `MappingException`: `Cannot create collection X…` |

Collections are always new, even when source and destination have the same type. Elements follow the normal rules, so they are shared if there is no map for them. The result of a `null` source is `null`, not an empty collection.

A `string` is not treated as a collection of characters.

## Null values

| Situation | Result |
|---|---|
| `Map` with a `null` source | The default of the destination type: `null`, or `0` and the like for value types |
| `Map` with a `null` source and a destination | The destination, unchanged |
| `null` source value for a reference type or `Nullable<T>` member | The member is set to `null`, overwriting any initial value |
| `null` source value for a value type member | The member is set to the default of its type |
| `null` source element in a collection | The same rules, for the element type |

Non-nullable reference type annotations have no effect: a `string` property can be set to `null`.

## Related

- [Exceptions](exceptions.md)
- [How mapping works](../explanation/how-mapping-works.md)
