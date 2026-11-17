namespace TailorManager.Api;

public record CreateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    string Address);
