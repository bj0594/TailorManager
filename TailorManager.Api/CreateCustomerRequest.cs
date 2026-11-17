namespace TailorManager.Api;

public record CreateCustomerRequest(
    string Name,
    string Email,
    string PhoneNumber,
    string Address);
