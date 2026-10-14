using System.Reflection;

namespace PureMapper.Internal;

/// <summary>
/// The compiled, immutable plan for mapping one source type to one destination type.
/// </summary>
internal sealed class TypeMap
{
    public required Type SourceType { get; init; }
    public required Type DestinationType { get; init; }

    // Exactly one of these three is used to create the destination.
    public Func<object, object>? CustomConstructor { get; init; }
    public Func<object>? Factory { get; init; }
    public ConstructorInfo? Constructor { get; init; }

    public MemberMap[] ConstructorParameters { get; init; } = [];
    public MemberMap[] PropertyMaps { get; init; } = [];
    public Action<object, object>[] BeforeMaps { get; init; } = [];
    public Action<object, object>[] AfterMaps { get; init; } = [];
    public string[] UnmappedMembers { get; init; } = [];
}

/// <summary>
/// How to produce the value for one destination property or constructor parameter.
/// </summary>
internal sealed class MemberMap
{
    public required string Name { get; init; }
    public required Type SourceType { get; init; }
    public required Type DestinationType { get; init; }
    public required Func<object, object?> Resolver { get; init; }

    /// <summary>Null for constructor parameters.</summary>
    public Action<object, object?>? Setter { get; init; }

    public Func<object, bool>? Condition { get; init; }
    public bool HasNullSubstitute { get; init; }
    public object? NullSubstitute { get; init; }
}
