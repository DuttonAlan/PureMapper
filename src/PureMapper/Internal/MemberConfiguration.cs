namespace PureMapper.Internal;

/// <summary>
/// The user's settings for one destination member, collected by <c>ForMember</c>.
/// </summary>
internal sealed class MemberConfiguration
{
    public bool Ignored { get; set; }
    public Func<object, object?>? Resolver { get; set; }
    public Type? ResolverType { get; set; }
    public Func<object, bool>? Condition { get; set; }
    public bool HasNullSubstitute { get; set; }
    public object? NullSubstitute { get; set; }
}

internal sealed class MemberConfigurationExpression<TSource>(MemberConfiguration member)
    : IMemberConfigurationExpression<TSource>
{
    public void MapFrom<TSourceMember>(Func<TSource, TSourceMember> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        member.Resolver = source => resolver((TSource)source);
        member.ResolverType = typeof(TSourceMember);
    }

    public void Ignore() => member.Ignored = true;

    public void Condition(Func<TSource, bool> condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        member.Condition = source => condition((TSource)source);
    }

    public void NullSubstitute(object? value)
    {
        member.HasNullSubstitute = true;
        member.NullSubstitute = value;
    }
}
