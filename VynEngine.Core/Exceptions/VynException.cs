namespace VynEngine.Core.Exceptions;

/// <summary>
/// The base exception for all VynEngine-specific exceptions. All custom exceptions in the engine should inherit from this class.
/// </summary>
/// <param name="message">The exception message.</param>
/// <param name="innerException">The inner exception, if any.</param>
public abstract class VynException(string message = "", Exception? innerException = null) : Exception(message, innerException);