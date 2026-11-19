namespace TailorManager.Core;

public class InvalidOrderStatusException(string message) : InvalidOperationException(message);
