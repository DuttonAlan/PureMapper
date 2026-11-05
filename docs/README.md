# PureMapper documentation

The documentation follows the [Diátaxis](https://diataxis.fr/) framework and has four parts. Each part answers a different kind of question, so pick the one that fits what you need right now.

| | Part | Read it when you want to… |
|---|---|---|
| 🎓 | [Tutorials](#tutorials) | learn PureMapper by doing, step by step, from nothing to a working result |
| 🛠️ | [How-to guides](#how-to-guides) | get a specific job done, e.g. map to a record or update an existing object |
| 📖 | [Reference](#reference) | look up exact facts: API members, mapping rules, exceptions |
| 💡 | [Explanation](#explanation) | understand how PureMapper works and why it is designed that way |

## Tutorials

Start here if you are new to PureMapper.

1. [Getting started](tutorials/getting-started.md): build a console application that maps objects, flattens properties, customizes a member and validates its configuration.
2. [Use PureMapper in a web API](tutorials/use-in-a-web-api.md): register the mapper with dependency injection, organize maps in a profile, and map requests and responses in an ASP.NET Core application.

## How-to guides

Defining maps:

- [Customize a member](how-to/customize-a-member.md)
- [Map to records and immutable types](how-to/map-to-records-and-immutable-types.md)
- [Map collections](how-to/map-collections.md)
- [Convert between types](how-to/convert-between-types.md)
- [Map inherited types](how-to/map-inherited-types.md)
- [Map in both directions](how-to/map-in-both-directions.md)
- [Update an existing object](how-to/update-an-existing-object.md)
- [Handle circular references](how-to/handle-circular-references.md)

Setting up your application:

- [Organize maps with profiles](how-to/organize-maps-with-profiles.md)
- [Register with dependency injection](how-to/register-with-dependency-injection.md)
- [Validate your configuration](how-to/validate-your-configuration.md)
- [Troubleshoot mapping errors](how-to/troubleshoot-mapping-errors.md)

## Reference

- [API](reference/api.md): every public type and member
- [Mapping rules](reference/mapping-rules.md): the exact rules for matching, creating, converting and null handling
- [Exceptions](reference/exceptions.md): what is thrown, when, and with which message
- [Requirements and limitations](reference/limitations.md)

## Explanation

- [How mapping works](explanation/how-mapping-works.md)
- [Design decisions](explanation/design-decisions.md)
- [Performance and thread safety](explanation/performance-and-thread-safety.md)

## Terms

- A **map** tells PureMapper that a source type can be mapped to a destination type. You create one with `CreateMap<TSource, TDestination>()`.
- A **member** is a destination property or constructor parameter that a map fills in.
- A **profile** is a class that groups related maps.
- A **conversion** is what happens to a value when no map applies, e.g. `decimal` to `string`.
