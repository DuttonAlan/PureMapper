# Map to records and immutable types

This guide shows how to map to types you cannot set properties on: positional records, classes with only a parameterized constructor, and `init`-only properties.

## Map to a positional record

No configuration is needed. PureMapper calls the constructor and matches each parameter to a source property by name, including [flattening](../reference/mapping-rules.md#finding-the-source-of-a-member):

```csharp
public record CustomerRecord(int Id, string FirstName, string LastName);

cfg.CreateMap<Customer, CustomerRecord>();

var record = mapper.Map<CustomerRecord>(customer);
```

`init`-only properties, including extra ones on a record, are set in the same way as normal properties.

## Provide a value for a constructor parameter

`ForMember` works on a constructor parameter when the type has a public property with the same name, as records do:

```csharp
public record CustomerSummary(int Id, string Name);

cfg.CreateMap<Customer, CustomerSummary>()
    .ForMember(d => d.Name, o => o.MapFrom(s => s.FirstName + " " + s.LastName));
```

A parameter with no source and no `ForMember` gets its default value if it declares one. If not, it gets the default of its type and validation reports it as `unmapped constructor parameter`. `Ignore` removes that report.

## Call the constructor yourself

Use `ConstructUsing` when any of these is true:

- A constructor parameter has no property with the same name, so `ForMember` cannot refer to it.
- PureMapper would pick the wrong constructor. It uses a public parameterless constructor if there is one, and otherwise the public constructor with the most parameters.
- The destination is an interface or has no public constructor.

```csharp
public class Money(decimal value, string currency)
{
    public decimal Amount { get; } = value;
    public string Code { get; } = currency;
}

cfg.CreateMap<Order, Money>()
    .ConstructUsing(s => new Money(s.Total, s.Currency));
```

`ConstructUsing` only replaces the creation. **Settable and `init` properties are still mapped afterwards** and overwrite what your factory set. Add `Ignore` for those that your factory should decide:

```csharp
public record Audit(string User);

cfg.CreateMap<Customer, Audit>()
    .ConstructUsing(s => new Audit("system"))
    .ForMember(d => d.User, o => o.Ignore());
```

## Map to a struct

A struct is always created with its default constructor and then filled through its settable properties. This works for `record struct` and `readonly record struct`, whose properties are settable or `init`.

A struct with get-only properties set in its constructor stays empty, and validation does not report it because get-only properties are not considered. Use `ConstructUsing`:

```csharp
public readonly struct Point(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;
}

cfg.CreateMap<Location, Point>()
    .ConstructUsing(s => new Point(s.X, s.Y));
```

## Related

- [Creating the destination](../reference/mapping-rules.md#creating-the-destination)
- [Convert between types](convert-between-types.md)
