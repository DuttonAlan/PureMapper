# Convert between types

PureMapper converts between numbers, strings, dates and enums on its own. This guide shows what to do when the [built-in conversions](../reference/mapping-rules.md#value-conversion) are not what you want.

## Convert one member

Use `MapFrom` and convert the value yourself:

```csharp
cfg.CreateMap<OrderRow, Order>()
    .ForMember(d => d.Id, o => o.MapFrom(s => Guid.Parse(s.Id)))
    .ForMember(d => d.PlacedOn, o => o.MapFrom(s => DateOnly.FromDateTime(s.PlacedAt)));
```

You need this for types that do not implement `IConvertible`, such as `Guid`, `DateOnly`, `TimeOnly`, `DateTimeOffset` and `TimeSpan`. Without it, validation reports `cannot map` and mapping throws a `MappingException`.

## Convert a type everywhere

To apply a conversion wherever a pair of types occurs, add a map with `ConstructUsing`:

```csharp
cfg.CreateMap<string, Guid>().ConstructUsing(Guid.Parse);
cfg.CreateMap<DateTime, DateOnly>().ConstructUsing(DateOnly.FromDateTime);
```

This works for types without settable properties. If the destination has some, they are still mapped afterwards, as with any map.

The destination type has to match exactly, so `Guid?` needs a map of its own.

## Format a value for a culture

Conversions always use the invariant culture, so `12.5m` becomes `"12.5"`. For another format, use `MapFrom`:

```csharp
var german = CultureInfo.GetCultureInfo("de-DE");

cfg.CreateMap<Order, OrderDto>()
    .ForMember(d => d.Total, o => o.MapFrom(s => s.Total.ToString("C", german)));
```

## Map enums with different names

Enums are matched by name, not case-sensitively. A name missing from the destination enum makes the mapping fail when that value is mapped. Validation cannot find this, because it depends on the value. Write the mapping out in `MapFrom`:

```csharp
cfg.CreateMap<Order, OrderDto>()
    .ForMember(d => d.Status, o => o.MapFrom(s => s.Status switch
    {
        OrderStatus.Pending => OrderStatusDto.Open,
        OrderStatus.Shipped => OrderStatusDto.Closed,
        _ => throw new ArgumentOutOfRangeException(nameof(s.Status), s.Status, null),
    }));
```

A number is converted to an enum without a check that the value is defined: mapping `99` to an enum gives the value `99`. Use `Enum.IsDefined` in `MapFrom` if the number comes from outside your application.

## Get an independent copy of an object

If the source and destination property have the same class and there is no map, the reference is copied and both objects share the instance. For a separate copy, add a map from the type to itself:

```csharp
cfg.CreateMap<Customer, Customer>();
cfg.CreateMap<Address, Address>();
```

## Related

- [Value conversion in the mapping rules](../reference/mapping-rules.md#value-conversion)
