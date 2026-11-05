# Map inherited types

This guide shows how maps behave with class hierarchies, and how to get a derived destination type for a derived source type.

## Reuse a map for derived source types

A map for a base class or interface also applies to everything derived from it, without configuration. PureMapper looks at the runtime type of the source:

```csharp
cfg.CreateMap<Customer, CustomerDto>();

CustomerDto dto = mapper.Map<CustomerDto>(new VipCustomer { FirstName = "Ada" });
```

Only the members of the destination are mapped, so properties that exist only on `VipCustomer` are not used.

## Treat one derived type differently

Add a map for the derived type. The most specific map wins, even if the variable has the base type:

```csharp
cfg.CreateMap<Customer, CustomerDto>();
cfg.CreateMap<VipCustomer, CustomerDto>()
    .ForMember(d => d.FirstName, o => o.MapFrom(s => "VIP " + s.FirstName));

Customer customer = new VipCustomer { FirstName = "Ada" };

var dto = mapper.Map<CustomerDto>(customer); // dto.FirstName is "VIP Ada"
```

This also applies to each element of a collection and to each nested object.

## Create a derived destination

The destination type is always exactly the one you ask for: a map from `Card` to `CardDto` is not used for `mapper.Map<PaymentDto>(card)`, even if `CardDto` derives from `PaymentDto`.

To get a different derived destination for each source type, choose it in `ConstructUsing` on the base map. Members of the base type are then mapped by convention, and the factory sets those of the derived type:

```csharp
public class Payment { public decimal Amount { get; set; } }
public class Card : Payment { public string Last4 { get; set; } = ""; }

public abstract class PaymentDto { public decimal Amount { get; set; } }
public class CardDto : PaymentDto { public string Last4 { get; set; } = ""; }
public class OtherPaymentDto : PaymentDto;

cfg.CreateMap<Payment, PaymentDto>()
    .ConstructUsing(s => s switch
    {
        Card card => new CardDto { Last4 = card.Last4 },
        _ => new OtherPaymentDto(),
    });

List<PaymentDto> dtos = mapper.Map<List<PaymentDto>>(payments);
```

## Related

- [Finding the map](../reference/mapping-rules.md#finding-the-map)
