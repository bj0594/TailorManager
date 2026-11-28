namespace TailorManager.Core;

// The order has an outstanding balance, so it cannot be delivered (§4). The API answers 409.
public class PaymentRequiredException(string message) : InvalidOperationException(message);
