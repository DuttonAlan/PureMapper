# Troubleshoot mapping errors

Find the message or symptom you see, and follow the fix. [Exceptions](../reference/exceptions.md) has the complete list of what is thrown and when.

## When the configuration is created

| Message | Cause | Fix |
|---|---|---|
| `A map from A to B is defined more than once.` | Two `CreateMap` or `ReverseMap` calls for the same pair, or a profile added twice. | [Remove the duplicate](organize-maps-with-profiles.md#avoid-duplicates). |
| `X has no public constructor. Use ConstructUsing to create it.` | The destination is an interface, or a class without a public constructor. | [Add `ConstructUsing`](map-to-records-and-immutable-types.md#call-the-constructor-yourself). |
| `Can't compile a NewExpression with a constructor declared on an abstract class` | The destination is an abstract class with a public constructor. | [Add `ConstructUsing`](map-to-records-and-immutable-types.md#call-the-constructor-yourself). |
| `X has no public property named 'y'.` | `ForMember` names a property that does not exist, or a constructor parameter without a matching property. | Fix the name, or [use `ConstructUsing`](map-to-records-and-immutable-types.md#call-the-constructor-yourself) for the parameter. |
| `'…' must select a property of the destination directly, e.g. d => d.Name.` | The `ForMember` lambda goes through a nested object, e.g. `d => d.Address.City`. | Select the outer property, or [use `AfterMap`](customize-a-member.md#run-code-before-or-after-the-mapping). |

## When you call `AssertConfigurationIsValid`

See [Fix what it reports](validate-your-configuration.md#fix-what-it-reports).

## When you call `AddPureMapper`

| Message | Fix |
|---|---|
| `PureMapper is already registered…` | [Call it only once](register-with-dependency-injection.md#call-it-once) and pass all assemblies to that call. |

## When you map

| Message | Cause | Fix |
|---|---|---|
| `No map from A to B is configured. Add CreateMap<A, B>().` | There is no map for the pair and no conversion applies. | Add the map. For types without their own properties, such as `Guid`, [add a conversion](convert-between-types.md). |
| `No map from A to Type is configured.` | You called `mapper.Map(source, typeof(B))`, and the compiler chose the overload that maps onto an existing object. | [Cast the source to `object`](#mapsource-type-treats-the-type-as-the-destination). |
| `Cannot create collection X…` | PureMapper cannot create this collection type. | [Add a map for the collection](map-collections.md#map-to-a-type-puremapper-cannot-create). |
| `Error mapping A -> B.X: …` | Reading the source value, converting it or setting property `X` failed. | The rest of the message and `InnerException` have the reason. |

Failures in these places surface as the original exception, without a `MappingException` around them or the name of the member:

| Exception | Likely cause |
|---|---|
| `FormatException`, `InvalidCastException`, `OverflowException` | A conversion failed in a constructor parameter or in a direct call like `mapper.Map<int>("abc")`. |
| `ArgumentException`: `Requested value 'X' was not found.` | The same, for an enum name that does not exist in the destination enum. |
| Any other | Thrown by your own `ConstructUsing`, `Condition`, `BeforeMap` or `AfterMap`, or a `MapFrom` for a constructor parameter. |

## `Map(source, type)` treats the type as the destination

To choose the destination type at runtime, the source has to have the static type `object`:

```csharp
object? dto = mapper.Map((object)order, typeof(OrderDto));
```

Without the cast, C# prefers `Map<TSource, TDestination>(source, destination)` with `TDestination` being `System.Type`, and PureMapper looks for a map from `Order` to `Type`.

## The process crashes with `Stack overflow.`

The object graph contains a cycle. See [Handle circular references](handle-circular-references.md).

## A property stays empty

No error, but the value is missing. Check these in order:

1. Run [validation](validate-your-configuration.md). If nothing has a matching name, it reports the property, while `Map` itself skips it without an error.
2. Does the property have a public setter or `init`? Get-only and private setters are never mapped, and validation does not report them.
3. Is the source a property? Public fields and methods are not used as sources.
4. Is a `Condition` false, or is the member ignored?
5. Is the source value `null`? In that case, the property is overwritten with `null`, or `0` and the like for value types.

## A value set in `ConstructUsing` is overwritten

Settable and `init` properties are still mapped after the factory ran. [Add `Ignore`](map-to-records-and-immutable-types.md#call-the-constructor-yourself) for them.

## Changing the destination also changes the source

The two share an object: with the same class on both sides and no map for it, the reference is copied. [Add a map from the type to itself](convert-between-types.md#get-an-independent-copy-of-an-object). Collections do not have this problem, because they are always copied, but their elements are shared in the same way.

## A profile's maps are missing

`AddMaps` skips abstract profiles and those without a public parameterless constructor, without an error. [Add them with `AddProfile`](organize-maps-with-profiles.md#pass-values-to-a-profile).
