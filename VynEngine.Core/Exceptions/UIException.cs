namespace VynEngine.Core.Exceptions;

/// <summary>
/// The exception that is thrown when an error occurs in the UI system.
/// </summary>
/// <param name="message">The exception message.</param>
/// <param name="inner">The inner exception, if any.</param>
public class UIException(string message = "", Exception? inner = null) : VynException(message, inner);