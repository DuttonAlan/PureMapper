# Customize a member

This guide shows how to change the way a single destination member is filled in when the conventions do not do what you need. You do all of it with `ForMember`.

## Take the value from somewhere else

Use `MapFrom` for a computed value or a source property with a different name:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName))
    .ForMember(d => d.Contact, o => o.MapFrom(s => s.Email));
```

The result then goes through the normal mapping, so a `MapFrom` can return a `decimal` for a `string` member, or an object that has its own map.

`MapFrom` takes a plain delegate, so it has no protection against `null` the way flattening does. Write `s.Address?.City`, not `s.Address.City`.

## Leave a member alone

Use `Ignore` for a member that should keep its value:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ForMember(d => d.InternalNotes, o => o.Ignore());
```

An ignored member no longer counts as unmapped in [validation](validate-your-configuration.md). If you also configure `MapFrom` for the member, `Ignore` wins.

## Map a member only sometimes

Use `Condition` with a test on the source object:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ForMember(d => d.Email, o => o.Condition(s => s.ShareEmail));
```

When the condition is false, a property keeps the value it has, and a constructor parameter gets the default value of its type.

## Use a default for null

Use `NullSubstitute` to replace a `null` source value:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ForMember(d => d.Email, o => o.NullSubstitute("n/a"));
```

The substitute goes through the normal mapping as well, so `NullSubstitute("7")` on an `int` member gives `7`. It also applies when a flattened path gives `null` because of a missing object in the middle.

## Combine options

To set several options for a member, put them in the same lambda:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ForMember(d => d.Contact, o =>
    {
        o.MapFrom(s => s.Email);
        o.NullSubstitute("n/a");
    });
```

Calling `ForMember` twice for the same member has the same effect: the options are added together.

## Refer to a member by name

`ForMember` also accepts the name of the member as a string, which is not case-sensitive:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ForMember("Email", o => o.Ignore());
```

Either form has to select a public property of the destination directly. `d => d.Address.City` throws an `ArgumentException`, and an unknown name throws a `MappingConfigurationException`. To set a nested value, use a [map for the nested type](../reference/mapping-rules.md#finding-the-map) or `AfterMap`.

## Run code before or after the mapping

For anything that does not fit one member, use `BeforeMap` and `AfterMap`:

```csharp
cfg.CreateMap<Order, OrderDto>()
    .AfterMap((source, destination) => destination.LineCount = destination.Lines.Length);
```

`BeforeMap` runs after the destination is created but before its properties are set, and `AfterMap` runs after that. You can call each of them more than once; the actions run in the order you added them.

A property set only in `AfterMap` still counts as unmapped, so also add `Ignore` for it if you validate your configuration:

```csharp
cfg.CreateMap<Order, OrderDto>()
    .ForMember(d => d.LineCount, o => o.Ignore())
    .AfterMap((source, destination) => destination.LineCount = destination.Lines.Length);
```

## Related

- [`IMemberConfigurationExpression<TSource>`](../reference/api.md#imemberconfigurationexpressiontsource)
- [Order of operations](../reference/mapping-rules.md#order-of-operations)
