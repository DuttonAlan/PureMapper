# Map collections

This guide shows how to map collections, both on their own and as properties, and what to do with the collection types PureMapper cannot create.

## Map a collection directly

You need a map for the element type, not for the collection. Name the collection type you want as the destination:

```csharp
cfg.CreateMap<Order, OrderDto>();

List<OrderDto> list = mapper.Map<List<OrderDto>>(orders);
OrderDto[] array = mapper.Map<OrderDto[]>(orders);
```

The source can be anything enumerable, and it is enumerated once.

## Map a collection property

The same applies to properties, and the collection types may differ:

```csharp
public class Order
{
    public List<OrderLine> Lines { get; set; } = [];
}

public class OrderDto
{
    public OrderLineDto[] Lines { get; set; } = [];
}

cfg.CreateMap<Order, OrderDto>();
cfg.CreateMap<OrderLine, OrderLineDto>();
```

## Keep a collection empty instead of null

A `null` source collection sets the destination to `null`, even if the destination property starts out as an empty list. To get an empty collection, use `NullSubstitute`:

```csharp
cfg.CreateMap<Order, OrderDto>()
    .ForMember(d => d.Lines, o => o.NullSubstitute(Array.Empty<OrderLine>()));
```

## Map to a type PureMapper cannot create

PureMapper can create arrays, anything a `List<T>` can be assigned to, and classes with a public constructor that accepts an `IEnumerable<T>`. This leaves out, among others, `ISet<T>`, `IDictionary<TKey, TValue>` and the immutable collections. They fail at runtime with `Cannot create collection`, and validation does not warn about it.

Using `MapFrom` is not enough, because its result would be converted in the same way. Add a map from the source collection to the destination type instead:

```csharp
cfg.CreateMap<Article, ArticleDto>();

cfg.CreateMap<IEnumerable<string>, ISet<string>>()
    .ConstructUsing(s => new HashSet<string>(s));

cfg.CreateMap<IEnumerable<string>, ImmutableList<string>>()
    .ConstructUsing(s => s.ToImmutableList());
```

Because a map also applies to anything that implements its source type, a map from `IEnumerable<string>` covers lists and arrays of strings. A factory has no access to the mapper, so if the elements need mapping, either build them in the factory yourself or choose a destination type PureMapper can create, such as `HashSet<T>`.

## Map a dictionary

A `Dictionary<TKey, TValue>` is copied if the keys and values have the same types on both sides. For different types, add a map as above:

```csharp
cfg.CreateMap<Dictionary<string, int>, Dictionary<string, string>>()
    .ConstructUsing(s => s.ToDictionary(p => p.Key, p => p.Value.ToString()));
```

## Update a collection in place

You can't: a mapped collection always replaces the existing one. See [Update an existing object](update-an-existing-object.md#nested-objects-and-collections-are-replaced).

## Related

- [Collections in the mapping rules](../reference/mapping-rules.md#collections)
