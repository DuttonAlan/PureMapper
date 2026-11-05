# Map in both directions

This guide shows how to create a map for the opposite direction with `ReverseMap`, and what to add when conventions alone do not work backwards.

## Add the reverse map

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ReverseMap();
```

This is a shorter way of writing two `CreateMap` calls. Do not also write `CreateMap<CustomerDto, Customer>()`, because a pair of types may only be defined once.

## Configure the reverse map

The reverse map works by convention only: **it does not inherit anything from the forward map.** A `MapFrom`, `Ignore` or `Condition` applies only to the direction it was written for.

`ReverseMap` returns the new map, so everything after it configures the reverse direction:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ForMember(d => d.Contact, o => o.MapFrom(s => s.Email)) // Customer -> CustomerDto
    .ReverseMap()
    .ForMember(s => s.Email, o => o.MapFrom(d => d.Contact)) // CustomerDto -> Customer
    .ForMember(s => s.Address, o => o.Ignore());
```

Put the forward configuration before `ReverseMap`.

## Rebuild flattened properties

Flattening does not work backwards: `CustomerName` is not split up into `Customer.Name`. Without configuration, validation reports `Customer` as unmapped and the property stays empty. Build the nested object yourself:

```csharp
public class Order
{
    public Customer? Customer { get; set; }
}

public class OrderDto
{
    public string? CustomerName { get; set; }
}

cfg.CreateMap<Order, OrderDto>()
    .ReverseMap()
    .ForMember(s => s.Customer, o => o.MapFrom(d => new Customer { Name = d.CustomerName ?? "" }));
```

`Customer` then maps to `Customer`, and with no map for that pair the new object is used as it is.

## Check the result

The forward map being complete says nothing about the reverse one. Run [validation](validate-your-configuration.md): it checks both directions as separate maps. Destination properties missing on the other side show up as unmapped, and each needs a `MapFrom` or an `Ignore`.

## Related

- [`ReverseMap`](../reference/api.md#imappingexpressiontsource-tdestination)
