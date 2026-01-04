## Repo Structure
Projects:
- `/OperatorPortal` - Back Office panel
- `/CharityPortal` - Front panel for Charities to sign up

All code must follow conventions specific for each project. 

Use domain terminology. Do not invent new terms or use technical jargon when domain terminology exists.

When discussing domain concepts, clarify terminology with the user. Add new terms to `/docs/glossary.md`.


## Testing

100% test coverage is mandatory and enforced.

## Security

- Never commit secrets, API keys, or credentials
- Use environment variables for sensitive configuration
- Do not log sensitive data (passwords, tokens, PII)
- Validate and sanitize all external input

## General Guidelines

- **Fail fast** - If a command fails or something doesn't work, STOP and discuss with the user. Do not improvise or try workarounds. Fix the underlying issue (or update the skill/docs) so it doesn't happen again.
- **Do not modify root configuration files** - If you believe a change is genuinely necessary, provide the suggested changes and ask the user.
- **Do not use `--no-verify`, `--force`, or `--hard` flags.** These are blocked by hooks and will fail. All commits must pass the `verify` gate.