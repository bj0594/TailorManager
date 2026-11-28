namespace TailorManager.Core;

// The transaction reference is already registered on an order (§4). The API answers 409.
public class DuplicatePaymentException(string message) : InvalidOperationException(message);
