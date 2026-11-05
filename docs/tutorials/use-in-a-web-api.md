# Use PureMapper in a web API

In this tutorial you build a small ASP.NET Core API for customers. You keep your maps in a profile, get the mapper through dependency injection, and use it for responses, for creating objects and for updating them. It takes about fifteen minutes.

It assumes you know the basics from [Getting started](getting-started.md).

## What you need

- The [.NET 10 SDK](https://dotnet.microsoft.com/download)
- `curl`, or another way to send HTTP requests

## 1. Create the project

```sh
dotnet new web -n MapperApi
cd MapperApi
dotnet add package PureMapper
```

## 2. Define the types

The API stores a `Customer`, but clients never see that class. They send requests and receive a response DTO, all of them records. Create `Models.cs`:

```csharp
public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Email { get; set; }
}

public record CustomerResponse(int Id, string FullName, string? Email);

public record CreateCustomerRequest(string FirstName, string LastName, string? Email);

public record UpdateCustomerRequest(string? FirstName, string? LastName, string? Email);
```

## 3. Put the maps in a profile

A profile groups related maps in a class that PureMapper can find on its own. Create `CustomerProfile.cs`:

```csharp
using PureMapper;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerResponse>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        CreateMap<CreateCustomerRequest, Customer>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<UpdateCustomerRequest, Customer>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.FirstName, o => o.Condition(s => s.FirstName is not null))
            .ForMember(d => d.LastName, o => o.Condition(s => s.LastName is not null))
            .ForMember(d => d.Email, o => o.Condition(s => s.Email is not null));
    }
}
```

There is a lot in here, so let us go through it:

- `CustomerResponse` is a record without a parameterless constructor, so PureMapper calls its constructor and matches the parameters by name. `FullName` has no matching source, so `MapFrom` provides it.
- The requests have no `Id`. The API assigns it, so the maps say to leave it alone.
- `Condition` makes the update a partial one: a property the client did not send stays as it is.

## 4. Register the mapper

Replace the contents of `Program.cs`:

```csharp
using PureMapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPureMapper(typeof(Program).Assembly);

var app = builder.Build();

app.Services.GetRequiredService<MapperConfiguration>().AssertConfigurationIsValid();

var customers = new List<Customer>
{
    new() { Id = 1, FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com" },
};

app.MapGet("/customers/{id:int}", (int id, IMapper mapper) =>
{
    var customer = customers.Find(c => c.Id == id);
    return customer is null
        ? Results.NotFound()
        : Results.Ok(mapper.Map<CustomerResponse>(customer));
});

app.Run();
```

`AddPureMapper` searches the assembly you pass for profiles and registers `IMapper` as a singleton, so any endpoint or service can ask for it. The next line checks the configuration as soon as the application starts.

Start the application:

```sh
dotnet run --urls http://localhost:5050
```

In a second terminal, ask for the customer:

```sh
curl http://localhost:5050/customers/1
```

```json
{"id":1,"fullName":"Ada Lovelace","email":"ada@example.com"}
```

The `Customer` became a `CustomerResponse` record, with `FullName` filled in by your `MapFrom`.

## 5. Create a customer

Stop the application with <kbd>Ctrl</kbd>+<kbd>C</kbd>. Add this endpoint before `app.Run();`:

```csharp
app.MapPost("/customers", (CreateCustomerRequest request, IMapper mapper) =>
{
    var customer = mapper.Map<Customer>(request);
    customer.Id = customers.Max(c => c.Id) + 1;
    customers.Add(customer);

    return Results.Created($"/customers/{customer.Id}", mapper.Map<CustomerResponse>(customer));
});
```

Start the application again and send a request:

```sh
curl -X POST http://localhost:5050/customers \
  -H 'Content-Type: application/json' \
  -d '{"firstName":"Grace","lastName":"Hopper","email":"grace@example.com"}'
```

```json
{"id":2,"fullName":"Grace Hopper","email":"grace@example.com"}
```

## 6. Update a customer

So far, every `Map` call has created a new object. To change an object you already have, pass it as the second argument. Stop the application and add this endpoint before `app.Run();`:

```csharp
app.MapPatch("/customers/{id:int}", (int id, UpdateCustomerRequest request, IMapper mapper) =>
{
    var customer = customers.Find(c => c.Id == id);
    if (customer is null)
    {
        return Results.NotFound();
    }

    mapper.Map(request, customer);

    return Results.Ok(mapper.Map<CustomerResponse>(customer));
});
```

Start the application again and change only the email address:

```sh
curl -X PATCH http://localhost:5050/customers/1 \
  -H 'Content-Type: application/json' \
  -d '{"email":"countess@example.com"}'
```

```json
{"id":1,"fullName":"Ada Lovelace","email":"countess@example.com"}
```

The request had no first or last name, so the conditions were false and the name stayed. Without the conditions, `null` would have overwritten both.

## 7. Watch the startup check work

Add a property to `Customer` in `Models.cs`:

```csharp
public DateTime CreatedAt { get; set; }
```

Stop the application and start it again. It now refuses to start, and the output contains:

```text
PureMapper.MappingConfigurationException: The mapping configuration is invalid:
CreateCustomerRequest -> Customer: unmapped property 'CreatedAt'.
UpdateCustomerRequest -> Customer: unmapped property 'CreatedAt'.
```

Validation lists all problems at once, so a new property cannot quietly go unmapped. In a real application, you would now decide what each map should do with it. For this tutorial, remove the property again.

## What you have learned

- `AddPureMapper` finds your profiles and registers `IMapper` for injection.
- Calling `AssertConfigurationIsValid` right after `builder.Build()` makes the application refuse to start with an invalid configuration.
- Records work as destinations without extra configuration.
- `Map(source, destination)` updates an existing object, and `Condition` turns that into a partial update.

## Next steps

- [Update an existing object](../how-to/update-an-existing-object.md) covers what happens to nested objects and collections.
- [Validate your configuration](../how-to/validate-your-configuration.md) shows how to check it in a unit test as well.
- [Register with dependency injection](../how-to/register-with-dependency-injection.md) covers the other ways to call `AddPureMapper`.
