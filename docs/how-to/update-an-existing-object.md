# Update an existing object

This guide shows how to map onto an object you already have, for example an entity loaded from a database, instead of creating a new one.

## Map onto the object

Pass the object as the second argument. You need a map for the pair of types, as usual:

```csharp
cfg.CreateMap<UpdateCustomerRequest, Customer>()
    .ForMember(d => d.Id, o => o.Ignore());

var customer = await db.Customers.FindAsync(id);

mapper.Map(request, customer);
```

No constructor or `ConstructUsing` factory is called. `BeforeMap` and `AfterMap` run as usual.

Add `Ignore` for every property the source must not change, such as the `Id` here. Properties with no matching source are skipped, but [validation](validate-your-configuration.md) reports them until you ignore them.

## Update only what was sent

Every mapped property is overwritten, also with `null`. For a partial update, add a `Condition` for each property that may be missing:

```csharp
public record UpdateCustomerRequest(string? FirstName, string? Email);

cfg.CreateMap<UpdateCustomerRequest, Customer>()
    .ForMember(d => d.Id, o => o.Ignore())
    .ForMember(d => d.FirstName, o => o.Condition(s => s.FirstName is not null))
    .ForMember(d => d.Email, o => o.Condition(s => s.Email is not null));
```

There is no setting that applies a condition to all members, so each one needs its own.

## Nested objects and collections are replaced

Only the top-level object is reused. A nested object or collection is created new and assigned to the property, even if there is already one there:

```csharp
var existingAddress = customer.Address;

mapper.Map(request, customer);

Console.WriteLine(ReferenceEquals(existingAddress, customer.Address)); // False
```

This matters for tracked entities: an ORM such as Entity Framework Core may treat the new objects as added and the old ones as removed. To update a nested object in place, ignore the member and map it yourself:

```csharp
cfg.CreateMap<UpdateCustomerRequest, Customer>()
    .ForMember(d => d.Id, o => o.Ignore())
    .ForMember(d => d.Address, o => o.Ignore());

cfg.CreateMap<AddressRequest, Address>();

mapper.Map(request, customer);
customer.Address = mapper.Map(request.Address, customer.Address);
```

Assigning the result covers both special cases: if `customer.Address` is `null`, `Map` creates a new object and returns it, and if `request.Address` is `null`, `Map` returns the destination unchanged.

## Use the return value for structs

A struct is copied when you pass it, so your variable stays as it was. Use the value that `Map` returns:

```csharp
point = mapper.Map(source, point);
```

## A map is required for reuse

Without a map for the pair of types, the object you passed in is not used at all. For example, `mapper.Map(numbers, existingList)` gives you a new list and leaves `existingList` unchanged.

## Related

- [Order of operations](../reference/mapping-rules.md#order-of-operations)
- [Customize a member](customize-a-member.md)
