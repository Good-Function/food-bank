Check `@docs/architecture/overview.md` to find out project architectire.

## Commands

### Build & Test

Always use dotnet commands for build, test.
``` bash
dotnet build
dotnet test
```

## Testing

Follow `docs/conventions/testing.md`.

100% test coverage is mandatory and enforced.

## Code Conventions

When writing, editing, refactoring, or reviewing code:

- always follow `docs/conventions/software-design.md`

The automatic code review agent enforces these conventions (see `./claude/automatic-code-review/rules.md`)

Code quality is of highest importance. Rushing or taking shortcuts is never acceptable.
