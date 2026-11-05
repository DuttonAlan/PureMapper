# Validate your configuration

`AssertConfigurationIsValid` checks every map and throws a `MappingConfigurationException` that lists all problems at once. This guide shows where to run it and how to fix what it reports.

## Run it in a unit test

This finds problems before you deploy, and it needs nothing but the configuration:

```csharp
public class MappingConfigurationTests
{
    [Fact]
    public void Configuration_is_valid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(SalesProfile).Assembly));

        config.AssertConfigurationIsValid();
    }
}
```

## Run it at startup

This also protects you if the test is not run, at the cost of checking all maps on each start:

```csharp
var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Program).Assembly));
config.AssertConfigurationIsValid();
```

With dependency injection:

```csharp
var app = builder.Build();

app.Services.GetRequiredService<MapperConfiguration>().AssertConfigurationIsValid();
```

## Fix what it reports

```text
The mapping configuration is invalid:
Customer -> CustomerDto: unmapped property 'Notes'.
Customer -> CustomerRecord: unmapped constructor parameter 'Name'.
Order -> OrderDto: cannot map 'Lines' from List<OrderLine> to OrderLineDto[]. Is a CreateMap missing?
```

| Message | Fix |
|---|---|
| `unmapped property 'X'` | Rename one of the properties, provide the value with [`MapFrom`](customize-a-member.md#take-the-value-from-somewhere-else), or add [`Ignore`](customize-a-member.md#leave-a-member-alone) if it should stay empty. |
| `unmapped constructor parameter 'x'` | The same options, or use [`ConstructUsing`](map-to-records-and-immutable-types.md#call-the-constructor-yourself) if the parameter has no property with the same name. |
| `cannot map 'X' from A to B` | Add `CreateMap<A, B>()`, or for a collection, a map for its elements. For types PureMapper cannot convert, see [Convert between types](convert-between-types.md). |

## Know what it does not check

Validation looks at types, not values, and only at the maps you defined. These are not reported:

- Conversions that can fail for a specific value or pair of types: `"abc"` to `int`, a `DateTime` to `int`, an enum name missing from the destination enum.
- Collections that cannot be created, such as `ISet<T>`. See [Map collections](map-collections.md#map-to-a-type-puremapper-cannot-create).
- Circular references. See [Handle circular references](handle-circular-references.md).
- A pair of types you map at runtime but never created a map for.
- Source properties that nothing uses. Only destination members have to be accounted for.
- Get-only destination properties and public fields, which are never mapped.
- Members whose `MapFrom` returns `object`, because the type is only known when mapping.

To test these, map a realistic object in a unit test and check the result.

## Handle a false alarm

A non-generic source collection, such as an `ArrayList`, is reported as `cannot map` although it works at runtime. To satisfy validation, tell it the element type with `MapFrom`:

```csharp
cfg.CreateMap<Legacy, ModernDto>()
    .ForMember(d => d.Numbers, o => o.MapFrom(s => s.Numbers.Cast<int>()));
```

## Related

- [Exceptions](../reference/exceptions.md)
- [Why validation is a separate step](../explanation/design-decisions.md#validation-is-a-separate-step)
