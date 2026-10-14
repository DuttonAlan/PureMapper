using System.Linq.Expressions;
using System.Reflection;

namespace PureMapper.Internal;

internal sealed class MappingExpression<TSource, TDestination>(Profile profile)
    : TypeMapConfiguration(typeof(TSource), typeof(TDestination)), IMappingExpression<TSource, TDestination>
{
    public IMappingExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource>> options)
    {
        ArgumentNullException.ThrowIfNull(destinationMember);
        return ForMember(GetMemberName(destinationMember), options);
    }

    public IMappingExpression<TSource, TDestination> ForMember(
        string destinationMemberName,
        Action<IMemberConfigurationExpression<TSource>> options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentNullException.ThrowIfNull(options);

        var exists = TypeHelper.GetReadableProperties(DestinationType)
            .Any(p => string.Equals(p.Name, destinationMemberName, StringComparison.OrdinalIgnoreCase));
        if (!exists)
        {
            throw new MappingConfigurationException(
                $"{DestinationType.GetFriendlyName()} has no public property named '{destinationMemberName}'.");
        }

        options(new MemberConfigurationExpression<TSource>(GetOrAddMember(destinationMemberName)));
        return this;
    }

    public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        CustomConstructor = source => factory((TSource)source)!;
        return this;
    }

    public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        BeforeMaps.Add((source, destination) => action((TSource)source, (TDestination)destination));
        return this;
    }

    public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        AfterMaps.Add((source, destination) => action((TSource)source, (TDestination)destination));
        return this;
    }

    public IMappingExpression<TDestination, TSource> ReverseMap() => profile.CreateMap<TDestination, TSource>();

    private static string GetMemberName(LambdaExpression expression)
    {
        var body = expression.Body is UnaryExpression { NodeType: ExpressionType.Convert } convert
            ? convert.Operand
            : expression.Body;

        if (body is MemberExpression { Member: PropertyInfo property } member && member.Expression == expression.Parameters[0])
        {
            return property.Name;
        }

        throw new ArgumentException(
            $"'{expression}' must select a property of the destination directly, e.g. d => d.Name.", nameof(expression));
    }
}
