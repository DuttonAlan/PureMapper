# Handle circular references

PureMapper does not track the objects it has visited. An object graph that refers back to itself is followed until the stack overflows, which ends the process: it cannot be caught with `try`/`catch`, and validation does not detect it. This guide shows how to break the cycle in your map.

Typical examples are parent and child objects that point to each other, such as Entity Framework Core entities with navigation properties in both directions.

## Find out whether you have one

Look for a crash with `Stack overflow.` in the output and a stack trace that repeats `PureMapper.Mapper.MapObject` and `MapCore`. Types that can form a cycle are only a problem if the objects actually do at runtime, so this may appear only with some data.

## Drop the back-reference

The simplest fix, and usually the best one for a DTO, is to leave out the property that points back:

```csharp
public class ParentDto
{
    public string Name { get; set; } = "";
    public List<ChildDto> Children { get; set; } = [];
}

public class ChildDto
{
    public string Name { get; set; } = "";
    public ParentDto? Parent { get; set; }
}

cfg.CreateMap<Parent, ParentDto>();
cfg.CreateMap<Child, ChildDto>()
    .ForMember(d => d.Parent, o => o.Ignore());
```

If nothing uses `ChildDto.Parent`, remove the property from the DTO altogether. Then there is nothing to ignore.

## Restore the back-reference

If the destination needs the reference, ignore it as above and set it after the children are mapped:

```csharp
cfg.CreateMap<Parent, ParentDto>()
    .AfterMap((source, destination) =>
    {
        foreach (var child in destination.Children)
        {
            child.Parent = destination;
        }
    });

cfg.CreateMap<Child, ChildDto>()
    .ForMember(d => d.Parent, o => o.Ignore());
```

## Related

- [Requirements and limitations](../reference/limitations.md)
