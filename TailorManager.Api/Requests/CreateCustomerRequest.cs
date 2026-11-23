namespace TailorManager.Api;

// POST /customers, with the fields in the order of §4.
public record CreateCustomerRequest(string Name, string Phone, string Email, string Address);
