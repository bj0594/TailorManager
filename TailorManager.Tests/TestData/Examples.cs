namespace TailorManager.Tests;

// The example values from §4 in Specification.md (the list "Examples"), in one place.
// Both test projects use this file: TailorManager.Web.Tests links to it in its .csproj.
// A new example in §4 goes here first, and the tests use the name, not the text.
public static class Examples
{
    // Customer: AddCustomer(name, phone, email, address)
    public const string CustomerName = "Anna Hansen";
    public const string CustomerPhone = "98765432";
    public const string CustomerEmail = "anna@example.com";
    public const string CustomerAddress = "Storgata 1, 0155 Oslo";

    // A second customer, for tests that need two (like GetCustomer and the search).
    public const string OtherCustomerName = "Ola Nordmann";

    // Order: AddOrder(customerId, OrderType.NewGarment, workDescription, agreedPrice, relatedOrderId, employee)
    public const string WorkDescription = "Shorten the trousers by 3 cm";
    public const decimal AgreedPrice = 450m;

    // A second order, for tests that need two (like GetOrder and the filters).
    public const string OtherWorkDescription = "Take in the waist by 2 cm";

    // The logged-in employee, shown as "Kari N." in the history.
    public const string EmployeeFirstName = "Kari";
    public const string EmployeeLastName = "Nordmann";
    public const string EmployeeDisplayName = "Kari N.";

    // The password every test user gets.
    public const string Password = "Test-password-1";

    // The examples for the later cycles, in the order of §4.
    public const string ApprovalDocumentReference = "FORM-1001";
    public const decimal InspectionFeeAmount = 150m;
    public const string InspectionFeeDocumentReference = "FORM-1002";
    public const string WorkNote = "Hem pinned, ready to sew";
    public const string ProposedWorkChange = "Also take in the waist by 2 cm";
    public const decimal ProposedPriceAdjustment = 150m;
    public const string FittingOutcome = "The length is right";
    public const string CommunicationContent = "Yes, please take in the waist as well.";
    public const decimal PaymentAmount = 450m;
    public const string TransactionReference = "TX-1001";
}
