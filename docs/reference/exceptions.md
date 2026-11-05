# Exceptions

PureMapper defines two exception types, both derived directly from `Exception`.

| Type | Meaning |
|---|---|
| `MappingConfigurationException` | The configuration is invalid. |
| `MappingException` | An object could not be mapped at runtime. Wraps the original exception in `InnerException` where there is one. |

In the tables, `A` is the source type and `B` the destination type of a map.

## When the configuration is created

Thrown by the `MapperConfiguration` constructor or `AddPureMapper`, and by the configuration calls made inside them.

| Exception | Message | Cause |
|---|---|---|
| `MappingConfigurationException` | `A map from A to B is defined more than once.` | Two maps for the same pair of types. |
| `MappingConfigurationException` | `B has no public constructor. Use ConstructUsing to create it.` | The destination cannot be created. |
| `MappingConfigurationException` | `B has no public property named 'x'.` | `ForMember` with an unknown name. |
| `ArgumentException` | `'…' must select a property of the destination directly, e.g. d => d.Name.` | `ForMember` with a lambda that does not select a property of the destination. |
| `ArgumentException` | | `ForMember` with an empty name. |
| `ArgumentNullException` | | A required argument is `null`. |
| `InvalidOperationException` | `PureMapper is already registered. Call AddPureMapper once and pass all assemblies and maps to it.` | A second `AddPureMapper` call. |
| `InvalidOperationException` | `Can't compile a NewExpression with a constructor declared on an abstract class` | The destination is an abstract class with a public constructor and has no `ConstructUsing`. This message comes from .NET. |

## When validating

`AssertConfigurationIsValid` throws one `MappingConfigurationException`, whose message has a first line and then one line for each problem:

```text
The mapping configuration is invalid:
A -> B: unmapped property 'X'.
A -> B: unmapped constructor parameter 'x'.
A -> B: cannot map 'X' from S to T. Is a CreateMap missing?
```

| Line | Reported when |
|---|---|
| `unmapped property 'X'` | A [destination property](mapping-rules.md#which-members-are-mapped) has no source and is not ignored. |
| `unmapped constructor parameter 'x'` | The same for a constructor parameter without a declared default. |
| `cannot map 'X' from S to T` | The source and destination type of a member can be neither mapped nor converted. |

A pair of types counts as mappable if any of these is true:

- The source type is `object`, so it is only known at runtime.
- There is a [map for the pair](mapping-rules.md#finding-the-map).
- Both are generic collections or arrays, and their element types are mappable. Whether the collection type can be created is not checked.
- The destination can be assigned from the source, or is `string`.
- The destination is an enum, and the source is an enum, `string` or `IConvertible`.
- Both implement `IConvertible`.

The type of the member is used for this, with `Nullable<T>` treated as `T`. See also [what validation does not check](../how-to/validate-your-configuration.md#know-what-it-does-not-check).

## When mapping

| Exception | Message | Cause |
|---|---|---|
| `MappingException` | `No map from A to B is configured. Add CreateMap<A, B>().` | No map, and no conversion applies. |
| `MappingException` | `Cannot create collection X: it needs a public constructor that accepts IEnumerable<T>.` | See [Collections](mapping-rules.md#collections). |
| `MappingException` | `Error mapping A -> B.X: …` | An exception occurred while getting, converting or setting property `X`. The original is in `InnerException`. |
| `ArgumentNullException` | | `Map(source, null)` for the destination type. |

A `MappingException` from a nested object is passed on as it is, so the `Error mapping` message names the innermost map and property.

### What is not wrapped

Only the mapping of properties is wrapped in `Error mapping`. Exceptions from these reach you unchanged:

| Where | Examples |
|---|---|
| Conversions of a value passed directly to `Map` | `mapper.Map<int>("abc")` throws `FormatException` |
| Constructor parameters, both reading the value and converting it | A `record R(int V)` with a source `V` of `"abc"` throws `FormatException` |
| The `ConstructUsing` factory and the constructor itself | |
| `Condition`, `BeforeMap` and `AfterMap` | |

To handle all failures of a `Map` call, catch `Exception` rather than `MappingException`, or check for these cases separately.

## Stack overflow

A [circular reference](../how-to/handle-circular-references.md) overflows the stack. .NET ends the process, so it cannot be caught.
