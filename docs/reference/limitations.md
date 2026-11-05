# Requirements and limitations

## Requirements

| | |
|---|---|
| Target framework | .NET 10 or later |
| Dependencies | `Microsoft.Extensions.DependencyInjection.Abstractions` |
| Package | [`PureMapper`](https://www.nuget.org/packages/PureMapper) |

## Not supported

| Feature | What happens | Alternative |
|---|---|---|
| `IQueryable` projection (`ProjectTo`) | No such API. | Write a `Select` in your query, or map after loading. |
| Circular references | Stack overflow that ends the process. | [Break the cycle](../how-to/handle-circular-references.md). |
| Unflattening in `ReverseMap` (`CustomerName` → `Customer.Name`) | The nested object stays empty, and validation reports it. | [Build it with `MapFrom`](../how-to/map-in-both-directions.md#rebuild-flattened-properties). |
| Mapping into existing nested objects or collections | They are replaced with new ones. | [Map them separately](../how-to/update-an-existing-object.md#nested-objects-and-collections-are-replaced). |
| Native AOT and trimming | Mapping relies on reflection and compiled expressions. | None. |

Also not available, because the API has no such feature:

- Automatically created maps: every pair of your own types needs a `CreateMap`.
- Choosing a derived destination through a declared hierarchy: [use `ConstructUsing`](../how-to/map-inherited-types.md#create-a-derived-destination).
- Options applying to all members of a map: each member is configured with its own `ForMember`.
- Mapping from or to fields, or from methods: [only properties are members](mapping-rules.md#which-members-are-mapped).
- Dependency injection into callbacks: [see the guide](../how-to/register-with-dependency-injection.md#use-services-in-a-map).
- `async` callbacks: `MapFrom`, `Condition`, `BeforeMap` and `AfterMap` are synchronous delegates.

## Collection types

`ISet<T>`, `IDictionary<TKey, TValue>`, the immutable collections and other types without a suitable public constructor need [a map of their own](../how-to/map-collections.md#map-to-a-type-puremapper-cannot-create).

## Related

- [Design decisions](../explanation/design-decisions.md)
