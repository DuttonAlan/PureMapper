namespace PureMapper.Tests;

public enum OrderStatus { Pending, Shipped }

public enum OrderStatusDto { Pending, Shipped }

public class Address
{
    public string City { get; set; } = "";
    public string Street { get; set; } = "";
}

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Email { get; set; }
    public Address? Address { get; set; }
}

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

public class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Email { get; set; }
}

public class OrderLineDto
{
    public string Product { get; set; } = "";
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string? CustomerFirstName { get; set; }
    public string? CustomerAddressCity { get; set; }
    public OrderLineDto[] Lines { get; set; } = [];
    public OrderStatusDto Status { get; set; }
    public string Total { get; set; } = "";
}

public record CustomerRecord(int Id, string FirstName, string LastName);

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ReverseMap()
            .ForMember(c => c.Address, o => o.Ignore());
    }
}
