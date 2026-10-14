# Contributing to PureMapper

Thanks for your interest in improving PureMapper! Bug reports, ideas and pull requests are all welcome.

## Reporting bugs and requesting features

- Search the existing issues first to avoid duplicates.
- For bugs, include the PureMapper and .NET versions, a minimal code sample (source type, destination type
  and configuration) and the exception or wrong result you got.
- Security issues must **not** be reported as public issues. See [SECURITY.md](SECURITY.md).

## Development setup

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet build
dotnet test
```

## Pull requests

1. Open an issue first for larger changes, so we can agree on the approach before you invest time.
2. Create a branch from `main`.
3. Add or update tests in `PureMapper.Tests` for every behavior change.
4. Make sure `dotnet build` produces no warnings and `dotnet test` passes. CI treats warnings as errors.
5. Add an entry under `[Unreleased]` in [CHANGELOG.md](CHANGELOG.md).
6. Keep public APIs documented with XML comments.

## Design principles

PureMapper aims to stay **small and predictable**. Before adding a feature, consider:

- Is it needed by many users, or can it be done with `MapFrom`, `ConstructUsing` or `AfterMap`?
- Does it keep the public API small and the behavior easy to explain?
- Does it avoid new package dependencies?

## Code of conduct

This project follows the [Code of Conduct](CODE_OF_CONDUCT.md). By participating you agree to uphold it.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
