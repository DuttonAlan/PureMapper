namespace PureMapper;

/// <summary>
/// Options for mapping a single destination member.
/// </summary>
public interface IMemberConfigurationExpression<TSource>
{
    /// <summary>
    /// Takes the member's value from <paramref name="resolver"/> instead of a same-named source member.
    /// </summary>
    void MapFrom<TSourceMember>(Func<TSource, TSourceMember> resolver);

    /// <summary>
    /// Leaves the member untouched.
    /// </summary>
    void Ignore();

    /// <summary>
    /// Only maps the member when <paramref name="condition"/> returns <see langword="true"/>.
    /// </summary>
    void Condition(Func<TSource, bool> condition);

    /// <summary>
    /// Uses <paramref name="value"/> when the source value is <see langword="null"/>.
    /// </summary>
    void NullSubstitute(object? value);
}
