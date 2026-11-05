# Getting started

In this tutorial you build a small console application that maps a customer and an order to DTOs. Along the way you use PureMapper's conventions, customize a member, and let validation find mistakes for you. It takes about fifteen minutes.

When you are done, you will have a program that prints a mapped order, and you will have seen the two error messages you are most likely to meet. The [next tutorial](use-in-a-web-api.md) moves on to a web API.

## What you need

- The [.NET 10 SDK](https://dotnet.microsoft.com/download) (`dotnet --version` prints `10.0` or later)

## 1. Create the project

```sh
dotnet new console -n MapperTour
cd MapperTour
dotnet add package PureMapper
```

## 2. Map your first object

Create a file `Models.cs` with a customer and the DTO you want to turn it into:

```csharp
public class Address
{
    public string City { get; set; } = "";
}

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public Address? Address { get; set; }
}

public class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
```

Replace the contents of `Program.cs`:

```csharp
using PureMapper;

var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Customer, CustomerDto>();
});

IMapper mapper = config.CreateMapper();

var customer = new Customer
{
    Id = 1,
    FirstName = "Ada",
    LastName = "Lovelace",
    Address = new Address { City = "London" },
};

CustomerDto dto = mapper.Map<CustomerDto>(customer);

Console.WriteLine($"{dto.Id}: {dto.FirstName} {dto.LastName}");
```

Run it:

```sh
dotnet run
```

You should see:

```text
1: Ada Lovelace
```

You never told PureMapper how to copy `Id`, `FirstName` or `LastName`. The properties have the same names, so the convention does the work. `CreateMap` only says that this pair of types may be mapped.

## 3. Flatten a nested property

Add a property to `CustomerDto`:

```csharp
public string? AddressCity { get; set; }
```

Change the last line of `Program.cs`:

```csharp
Console.WriteLine($"{dto.Id}: {dto.FirstName} {dto.LastName}, {dto.AddressCity}");
```

Run it again:

```text
1: Ada Lovelace, London
```

`CustomerDto` has `AddressCity`, `Customer` has no property with that name, so PureMapper split the name and followed `Customer.Address.City`. This is called *flattening*.

Now see what happens when the customer has no address. Remove the line `Address = new Address { City = "London" },` from `Program.cs` and run it:

```text
1: Ada Lovelace, 
```

There is no `NullReferenceException`: a `null` along the path gives `null`. Put the line back before you continue.

## 4. Customize a member

The DTO should also show the full name, and no property with that name exists on `Customer`. Add a property to `CustomerDto`:

```csharp
public string FullName { get; set; } = "";
```

Tell PureMapper where the value comes from by changing the map in `Program.cs`:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));
```

And print it:

```csharp
Console.WriteLine($"{dto.Id}: {dto.FullName}, {dto.AddressCity}");
```

```text
1: Ada Lovelace, London
```

## 5. Let validation find a mistake

So far every destination property had a source. Now add one that does not. Add a property to `CustomerDto`:

```csharp
public string Notes { get; set; } = "";
```

Run the program. It still prints the same line, and `Notes` stays empty. PureMapper does not stop you, and in a real project a typo in a property name could go unnoticed like this.

To catch it, add this line to `Program.cs`, directly after the configuration is created:

```csharp
config.AssertConfigurationIsValid();
```

Run it, and the program now stops. The output starts with:

```text
Unhandled exception. PureMapper.MappingConfigurationException: The mapping configuration is invalid:
Customer -> CustomerDto: unmapped property 'Notes'.
```

The message names the map and the property. `Notes` is meant to stay empty, so say so:

```csharp
cfg.CreateMap<Customer, CustomerDto>()
    .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName))
    .ForMember(d => d.Notes, o => o.Ignore());
```

Run it, and the program works again. From now on, a destination property without a source stops the program when it starts, not when a customer notices the missing data.

## 6. Map nested objects and collections

Add an order to `Models.cs`:

```csharp
public enum OrderStatus { Pending, Shipped }

public class OrderLine
{
    public string Product { get; set; } = "";
    public int Quantity { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public Customer? Customer { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
}

public class OrderLineDto
{
    public string Product { get; set; } = "";
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public CustomerDto? Customer { get; set; }
    public OrderLineDto[] Lines { get; set; } = [];
    public string Status { get; set; } = "";
    public string Total { get; set; } = "";
}
```

Add a map for the order to the configuration in `Program.cs`, after the customer map:

```csharp
cfg.CreateMap<Order, OrderDto>();
```

Run it. Validation stops the program again, for a different reason:

```text
Unhandled exception. PureMapper.MappingConfigurationException: The mapping configuration is invalid:
Order -> OrderDto: cannot map 'Lines' from List<OrderLine> to OrderLineDto[]. Is a CreateMap missing?
```

PureMapper knows how to turn a list into an array, but not how to turn one `OrderLine` into an `OrderLineDto`. Every pair of your own classes needs its own map, so add it:

```csharp
cfg.CreateMap<OrderLine, OrderLineDto>();
```

At the end of `Program.cs`, create an order and map it:

```csharp
var order = new Order
{
    Id = 7,
    Customer = customer,
    Lines = [new OrderLine { Product = "Pen", Quantity = 2 }, new OrderLine { Product = "Ink", Quantity = 1 }],
    Status = OrderStatus.Shipped,
    Total = 12.5m,
};

OrderDto orderDto = mapper.Map<OrderDto>(order);

Console.WriteLine($"Order {orderDto.Id} for {orderDto.Customer?.FullName}: {orderDto.Status}, {orderDto.Total}");

foreach (var line in orderDto.Lines)
{
    Console.WriteLine($"  {line.Quantity} x {line.Product}");
}
```

Run it:

```text
1: Ada Lovelace, London
Order 7 for Ada Lovelace: Shipped, 12.5
  2 x Pen
  1 x Ink
```

Look at what happened to each property of the order:

| Property | From | To | How |
|---|---|---|---|
| `Customer` | `Customer` | `CustomerDto` | through its map, including `FullName` |
| `Lines` | `List<OrderLine>` | `OrderLineDto[]` | a new array, each element through its map |
| `Status` | `OrderStatus` | `string` | the name of the enum value |
| `Total` | `decimal` | `string` | converted with the invariant culture |

## What you have learned

- `CreateMap` declares a pair of types, and the conventions handle properties with matching names.
- `ForMember` customizes a member: `MapFrom` provides the value and `Ignore` skips it.
- `AssertConfigurationIsValid` finds destination members without a source and missing maps before anything is mapped.
- You create `MapperConfiguration` once and get the `IMapper` from it.

## Next steps

- [Use PureMapper in a web API](use-in-a-web-api.md) continues with dependency injection and profiles.
- The [how-to guides](../README.md#how-to-guides) cover specific jobs, such as [mapping to records](../how-to/map-to-records-and-immutable-types.md).
- [Mapping rules](../reference/mapping-rules.md) lists everything the conventions do.
