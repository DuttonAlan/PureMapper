using System.Reflection;

namespace PureMapper.Internal;

/// <summary>
/// Non-generic state collected by <see cref="MappingExpression{TSource, TDestination}"/>, turned into a
/// <see cref="TypeMap"/> when the <see cref="MapperConfiguration"/> is created.
/// </summary>
internal abstract class TypeMapConfiguration(Type sourceType, Type destinationType)
{
    private readonly Dictionary<string, MemberConfiguration> _members = new(StringComparer.OrdinalIgnoreCase);

    public Type SourceType { get; } = sourceType;
    public Type DestinationType { get; } = destinationType;

    protected Func<object, object>? CustomConstructor { get; set; }
    protected List<Action<object, object>> BeforeMaps { get; } = [];
    protected List<Action<object, object>> AfterMaps { get; } = [];

    protected MemberConfiguration GetOrAddMember(string name)
    {
        if (!_members.TryGetValue(name, out var member))
        {
            member = new MemberConfiguration();
            _members[name] = member;
        }

        return member;
    }

    public TypeMap Build()
    {
        var unmapped = new List<string>();
        var constructorParameters = new List<MemberMap>();
        Func<object>? factory = null;
        ConstructorInfo? constructor = null;

        if (CustomConstructor is null)
        {
            if (DestinationType.IsValueType || DestinationType.GetConstructor(Type.EmptyTypes) is not null)
            {
                factory = Accessors.CreateFactory(DestinationType);
            }
            else
            {
                // No parameterless constructor (e.g. positional records): use the most complete public one.
                constructor = DestinationType.GetConstructors().MaxBy(c => c.GetParameters().Length)
                    ?? throw new MappingConfigurationException(
                        $"{DestinationType.GetFriendlyName()} has no public constructor. Use ConstructUsing to create it.");

                foreach (var parameter in constructor.GetParameters())
                {
                    var member = CreateMemberMap(parameter.Name!, parameter.ParameterType, setter: null);
                    if (member is null)
                    {
                        if (!parameter.HasDefaultValue && !IsIgnored(parameter.Name!))
                        {
                            unmapped.Add($"constructor parameter '{parameter.Name}'");
                        }

                        var defaultValue = parameter.HasDefaultValue ? parameter.DefaultValue : null;
                        member = new MemberMap
                        {
                            Name = parameter.Name!,
                            SourceType = typeof(object),
                            DestinationType = parameter.ParameterType,
                            Resolver = _ => defaultValue,
                        };
                    }

                    constructorParameters.Add(member);
                }
            }
        }

        var propertyMaps = new List<MemberMap>();
        foreach (var property in TypeHelper.GetWritableProperties(DestinationType))
        {
            // Already set through the constructor.
            if (constructorParameters.Any(p => string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var member = CreateMemberMap(property.Name, property.PropertyType, Accessors.CreateSetter(property));
            if (member is not null)
            {
                propertyMaps.Add(member);
            }
            else if (!IsIgnored(property.Name))
            {
                unmapped.Add($"property '{property.Name}'");
            }
        }

        return new TypeMap
        {
            SourceType = SourceType,
            DestinationType = DestinationType,
            CustomConstructor = CustomConstructor,
            Factory = factory,
            Constructor = constructor,
            ConstructorParameters = [.. constructorParameters],
            PropertyMaps = [.. propertyMaps],
            BeforeMaps = [.. BeforeMaps],
            AfterMaps = [.. AfterMaps],
            UnmappedMembers = [.. unmapped],
        };
    }

    private bool IsIgnored(string name) => _members.TryGetValue(name, out var member) && member.Ignored;

    /// <summary>
    /// Returns null when the member is ignored or no source could be found for it.
    /// </summary>
    private MemberMap? CreateMemberMap(string name, Type destinationType, Action<object, object?>? setter)
    {
        _members.TryGetValue(name, out var config);
        if (config?.Ignored == true)
        {
            return null;
        }

        Func<object, object?> resolver;
        Type sourceType;

        if (config?.Resolver is not null)
        {
            resolver = config.Resolver;
            sourceType = config.ResolverType!;
        }
        else if (TypeHelper.FindSourcePath(SourceType, name) is { } path)
        {
            resolver = Accessors.CreateGetter(path);
            sourceType = path[^1].PropertyType;
        }
        else
        {
            return null;
        }

        return new MemberMap
        {
            Name = name,
            SourceType = sourceType,
            DestinationType = destinationType,
            Resolver = resolver,
            Setter = setter,
            Condition = config?.Condition,
            HasNullSubstitute = config?.HasNullSubstitute ?? false,
            NullSubstitute = config?.NullSubstitute,
        };
    }
}
