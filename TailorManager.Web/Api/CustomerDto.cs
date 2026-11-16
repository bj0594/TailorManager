namespace TailorManager.Web.Api;

// One <thing> as the API sends it. The fields are the ones in the response in api.md.
public record CustomerDto(Guid Id, string Name, string Phone, string Email, string Address);
