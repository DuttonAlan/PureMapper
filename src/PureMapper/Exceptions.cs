namespace PureMapper;

/// <summary>
/// Thrown when the mapping configuration is invalid.
/// </summary>
public class MappingConfigurationException(string message) : Exception(message);

/// <summary>
/// Thrown when an object cannot be mapped at runtime.
/// </summary>
public class MappingException(string message, Exception? innerException = null) : Exception(message, innerException);
