using System.Linq.Expressions;
using System.Reflection;

namespace PureMapper.Internal;

/// <summary>
/// Compiles property getters, setters and constructors into delegates so mapping avoids slow reflection calls.
/// </summary>
internal static class Accessors
{
    public static Func<object, object?> CreateGetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var body = Expression.Convert(
            Expression.Property(Expression.Convert(instance, property.DeclaringType!), property),
            typeof(object));

        return Expression.Lambda<Func<object, object?>>(body, instance).Compile();
    }

    /// <summary>
    /// Creates a getter for a chain like <c>Customer.Address.City</c> that returns null if any link is null.
    /// </summary>
    public static Func<object, object?> CreateGetter(IReadOnlyList<PropertyInfo> path)
    {
        if (path.Count == 1)
        {
            return CreateGetter(path[0]);
        }

        var getters = path.Select(CreateGetter).ToArray();
        return source =>
        {
            object? current = source;
            foreach (var getter in getters)
            {
                if (current is null)
                {
                    return null;
                }

                current = getter(current);
            }

            return current;
        };
    }

    public static Action<object, object?> CreateSetter(PropertyInfo property)
    {
        // A compiled setter would modify an unboxed copy of a struct; reflection modifies the boxed instance.
        if (property.DeclaringType!.IsValueType)
        {
            return property.SetValue;
        }

        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");
        var body = Expression.Assign(
            Expression.Property(Expression.Convert(instance, property.DeclaringType), property),
            Expression.Convert(value, property.PropertyType));

        return Expression.Lambda<Action<object, object?>>(body, instance, value).Compile();
    }

    public static Func<object> CreateFactory(Type type)
    {
        var body = Expression.Convert(Expression.New(type), typeof(object));
        return Expression.Lambda<Func<object>>(body).Compile();
    }
}
