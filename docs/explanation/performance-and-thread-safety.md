# Performance and thread safety

This page explains where PureMapper spends its time, and why you can share one mapper between threads.

## Pay once at startup

Most of the expensive work happens when the `MapperConfiguration` is created: reflection over the types, matching the members, and compiling delegates for each property. Compiling expressions is much slower than calling the resulting delegates, so this cost only pays off if you do it once.

That is the reason for the most important rule: **create the configuration once and reuse it.** A configuration created per request would repeat all of this each time. A `Mapper`, on the other hand, holds nothing but a reference to the configuration, so `CreateMapper` is cheap.

## What runs on each call

| Step | How it runs |
|---|---|
| Reading a property | Compiled delegate |
| Setting a property on a class | Compiled delegate |
| Setting a property on a struct | Reflection (`PropertyInfo.SetValue`), because a compiled setter would change an unboxed copy |
| Creating with a parameterless constructor, or a struct | Compiled delegate |
| Creating with a parameterized constructor | Reflection (`ConstructorInfo.Invoke`) |
| `MapFrom`, `Condition`, `ConstructUsing` and hooks | Direct delegate call, with a cast of the arguments |
| Finding the map | Lookup in a concurrent dictionary, for the object and each member value |
| Creating a collection | Fills a `List<T>` through its non-generic interface, then copies it for anything but a list |

So mapping is not reflection-free, and a few things have a higher cost than the others:

- **Boxing.** Delegates work with `object`, so each value type member is boxed on the way through.
- **Records and classes with parameterized constructors** go through `ConstructorInfo.Invoke`, and each argument passes through an array.
- **Arrays and other non-list collections** are built twice.
- **Flattened paths** call one getter per step.

The mapper is designed to be simple rather than as fast as generated or handwritten code. For most uses, such as mapping DTOs around database or network calls, the difference will not matter. For a hot path with many objects, measure it with your own types, and write that one mapping by hand if needed: nothing requires you to use the mapper everywhere.

## Why sharing is safe

`MapperConfiguration` and `Mapper` can be used from any number of threads at the same time:

- The plans are built once in the constructor and never change afterwards. There is no method to add a map later.
- `Mapper` has no state of its own. Everything specific to a `Map` call lives on the stack.
- The one thing written after startup is the cache of map lookups. It is a `ConcurrentDictionary`, and two threads computing the same entry get the same answer.

This is why `AddPureMapper` can register both as singletons, and why injecting `IMapper` into a service of any lifetime is fine.

## What sharing does not cover

The guarantee covers PureMapper's own state, not your code or your objects:

- **Callbacks run on the calling thread, possibly on several at once.** A `MapFrom` or `AfterMap` that captures a shared variable has to be safe for this.
- **The objects being mapped are not protected.** If another thread changes the source during a `Map` call, you may see a mix of old and new values.
- **Calling `CreateMap` on a profile after the configuration is built has no effect.** The configuration only reads its profiles once.

## Related

- [How mapping works](how-mapping-works.md)
