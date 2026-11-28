namespace TailorManager.Core;

// The amount is not the outstanding balance (§4). The API answers 409.
public class PaymentAmountMismatchException(string message) : InvalidOperationException(message);
