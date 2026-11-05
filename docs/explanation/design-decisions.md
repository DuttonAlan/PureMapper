# Design decisions

PureMapper aims to stay small and predictable. A new feature has to be needed by many users, keep the behavior easy to explain, and avoid new package dependencies. If something can be done with `MapFrom`, `ConstructUsing` or `AfterMap`, that is usually preferred over a new option. This page goes through what that means in practice, and what you give up in return.

## Conventions, with an explicit list of pairs

Copying properties with the same name is what most mapping code does, so that part needs no configuration. But you still have to name each pair of types with `CreateMap`, and PureMapper never creates a map on its own.

An automatic map would save a line, but then anything could be mapped to anything, and a mistake would show up as a half-empty object at runtime. With an explicit list, validation knows what to check, and a missing map is an error naming the line to add. It also limits what a mapper can do with your types to what you stated.

The downside of conventions remains: renaming a property silently changes a mapping. That is the reason for the next decision.

## Validation is a separate step

Creating a configuration only fails for problems that make a map impossible to build, such as a destination that cannot be created. An unmapped member does not make it fail, so you need to call `AssertConfigurationIsValid` yourself.

Keeping these apart lets you choose where the check runs, in a [unit test or at startup](../how-to/validate-your-configuration.md), and lets you define maps that are deliberately partial, for example while migrating code. The price is that a project which never calls it gets no protection. The check is one line, so all the documentation recommends adding it.

Validation looks at types only, so it cannot promise that mapping will succeed: whether `"abc"` converts to an `int` depends on the value. It answers the narrower question of whether you accounted for every member. It also errs on the side of letting things through when it cannot know, such as a `MapFrom` returning `object`.

## Destination members drive mapping

Validation asks whether each *destination* member has a source, not whether each source member is used. A DTO is usually a smaller view of a larger object, so unused source properties are normal. A destination property without a value is much more likely to be a mistake.

## Plain delegates instead of expressions

`MapFrom` takes a `Func<TSource, TMember>`, not an expression tree. A lambda is simply called, so it can contain any code, and what you see is what runs. Debugging and stack traces work as usual.

The main thing this gives up is `IQueryable` projection, which has to translate lambdas into SQL and so has to be able to read them. It would also need its own version of every rule to behave the same way in a database, and that is a large feature for a small library. A `Select` in your query does the same job and shows you exactly what is loaded. Null safety inside `MapFrom` is also your own job, since nothing can rewrite the lambda.

## No tracking of visited objects

Handling circular references would mean remembering every object during a mapping, and paying for that on every call, including the large majority that have no cycles. It would also raise questions that have no single right answer, such as whether a shared object should become one instance or two. So PureMapper leaves that out, and you [break the cycle in your map](../how-to/handle-circular-references.md). DTOs rarely need cycles anyway.

The cost is severe when it does happen: a stack overflow ends the process, and validation does not warn you.

## ReverseMap does not invert anything

`ReverseMap` creates an independent map by convention. It does not try to work out the reverse of your `MapFrom`, because in general a function cannot be inverted: there is no way to get `FirstName` and `LastName` back from `s.FirstName + " " + s.LastName`.

Flattening could in principle be reversed by name, but it would have to invent the nested objects and decide what happens to the rest of their properties. Requiring you to [write that out](../how-to/map-in-both-directions.md#rebuild-flattened-properties) keeps the rule simple: what a reverse map does is what you can read in the reverse map.

## AddPureMapper fails on a second call

A second call could add to the first, replace it, or be ignored. With the usual `TryAdd` pattern it would be ignored, so maps would silently go missing. The configuration is immutable and already built by then, so PureMapper [throws instead](../how-to/register-with-dependency-injection.md#call-it-once) and asks you to pass everything in one call.

For the same reason of finding errors early, `AddPureMapper` builds the configuration immediately instead of on the first request.

## Reflection and compiled expressions

Finding properties by name at runtime needs reflection, and compiled expressions make it fast. In exchange, PureMapper does not work with Native AOT or trimming. A source generator would avoid that, and would find some errors at compile time, but it would be a much larger and different project.

## Related

- [How mapping works](how-mapping-works.md)
- [Requirements and limitations](../reference/limitations.md)
- [Contributing: design principles](../../CONTRIBUTING.md#design-principles)
