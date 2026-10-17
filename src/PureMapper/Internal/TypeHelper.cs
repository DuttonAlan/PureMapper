using System.Reflection;

namespace PureMapper.Internal;

internal static class TypeHelper
{
    public static PropertyInfo[] GetReadableProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetMethod is { IsPublic: true } && p.GetIndexParameters().Length == 0)
            .DistinctBy(p => p.Name)
            .ToArray();

    public static PropertyInfo[] GetWritableProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod is { IsPublic: true } && p.GetIndexParameters().Length == 0)
            .DistinctBy(p => p.Name)
            .ToArray();

    /// <summary>
    /// Finds the source property chain for a destination member name, case-insensitively.
    /// "Name" matches <c>Name</c>; "CustomerName" falls back to <c>Customer.Name</c> (flattening).
    /// </summary>
    public static PropertyInfo[]? FindSourcePath(Type sourceType, string name)
    {
        var properties = GetReadableProperties(sourceType);

        var exact = properties.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
        {
            return [exact];
        }

        var prefixes = properties
            .Where(p => name.Length > p.Name.Length && name.StartsWith(p.Name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Name.Length);

        foreach (var prefix in prefixes)
        {
            if (FindSourcePath(prefix.PropertyType, name[prefix.Name.Length..]) is { } rest)
            {
                return [prefix, .. rest];
            }
        }

        return null;
    }

    /// <summary>
    /// Returns T for arrays and anything implementing IEnumerable&lt;T&gt; (except string), otherwise null.
    /// </summary>
    public static Type? GetElementType(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return type.GetGenericArguments()[0];
        }

        return type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            ?.GetGenericArguments()[0];
    }

    public static bool IsConvertible(Type type) => typeof(IConvertible).IsAssignableFrom(type);

    public static object? GetDefaultValue(Type type) =>
        type.IsValueType && Nullable.GetUnderlyingType(type) is null ? Activator.CreateInstance(type) : null;

    public static string GetFriendlyName(this Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
        {
            return underlying.GetFriendlyName() + "?";
        }

        if (type.IsArray)
        {
            return type.GetElementType()!.GetFriendlyName() + "[]";
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var name = type.Name[..type.Name.IndexOf('`')];
        return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName))}>";
    }
}
