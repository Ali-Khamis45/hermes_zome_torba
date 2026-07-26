# Contributing to Hermes Zone Torba

Thank you for investing time in HZT. This document is the practical companion to
[docs/14-branching-strategy.md](docs/14-branching-strategy.md) and [docs/15-coding-standards.md](docs/15-coding-standards.md) —
read those for the full rationale; this is the checklist you'll actually use day to day.

## Before you start

1. Search open issues and discussions before filing a new one — duplicates slow everyone down.
2. For anything larger than a small fix (new module, schema change, public API change), open an issue or
   discussion first and get a maintainer nod before writing code. We'd rather redirect early than review a
   1,500-line PR that takes the wrong architectural approach.
3. Set up your environment per the [Quick Start](README.md#quick-start-development) in the README.

## Branching model

- `main` — protected, always releasable, tagged for releases. No direct commits.
- `develop` — integration branch. All feature work merges here first via PR.
- `feature/<area>-<short-description>` — your working branch, branched from `develop`.
- `fix/<area>-<short-description>` — bug fixes.
- `release/<version>` — cut from `develop` when preparing a release; only fixes land here.
- `hotfix/<version>-<short-description>` — cut from `main` for emergency production fixes, merged back to
  both `main` and `develop`.

Full details, protection rules, and diagrams: [docs/14-branching-strategy.md](docs/14-branching-strategy.md).

## Commit messages

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <short summary>

[optional body]

[optional footer(s)]
```

Types: `feat`, `fix`, `docs`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`.
Scopes match top-level modules: `backend`, `frontend`, `desktop`, `core`, `agents`, `plugins`, `sdk`, `docs`,
`infra`. Example: `feat(backend): add InstallHermesCommand handler with rollback on failed download`.

Breaking changes must include a `BREAKING CHANGE:` footer — this drives semantic-release version bumps.

## Pull requests

1. Rebase onto the latest `develop` before opening the PR.
2. Fill out the [PR template](.github/PULL_REQUEST_TEMPLATE.md) completely — link the issue, describe the
   change, list test evidence.
3. Keep PRs scoped to one logical change. If your diff touches backend, frontend, and desktop simultaneously
   for unrelated reasons, split it.
4. All CI checks (build, lint, unit tests, architecture tests) must pass before requesting review.
5. At least one maintainer approval is required; PRs touching `security/`, `SecurityLayer`, or auth code
   require a security-focused reviewer (see [CODEOWNERS](.github/CODEOWNERS)).
6. Squash-merge is the default merge strategy into `develop` to keep history readable.

## Code review guidelines

**As an author:**
- Keep diffs reviewable — under ~400 lines where possible.
- Explain *why*, not just *what*, in the PR description.
- Respond to every comment, even if just "done" or "disagree because X".

**As a reviewer:**
- Review for correctness, security, and architectural fit first; nitpicks last.
- Block on: violated layer dependencies (see [docs/15-coding-standards.md](docs/15-coding-standards.md#dependency-rules)),
  missing tests for new behavior, unhandled error paths, secrets or credentials in code.
- Don't block on style the linter should catch — file a follow-up issue instead.

## Testing expectations

Every PR that adds or changes behavior needs corresponding tests. See
[docs/16-testing.md](docs/16-testing.md) for what tier (unit / integration / architecture / E2E) applies to
your change. CI will run:

```bash
# Backend
dotnet test backend/HermesZoneTorba.slnx

# Frontend
npm --prefix frontend run lint && npm --prefix frontend run test

# Desktop
npm --prefix desktop run tauri build -- --debug
```

## Plugin contributions

Plugins live under `plugins/` and follow the manifest schema in [docs/10-plugin-system.md](docs/10-plugin-system.md).
First-party plugins go through the same review bar as core code, including a permissions audit.

## Code of conduct

Be direct, be kind, assume good faith. Harassment, discrimination, or bad-faith arguing get you removed from
the project. Report concerns to the maintainers listed in [CODEOWNERS](.github/CODEOWNERS).

## Release process

See [docs/18-roadmap.md](docs/18-roadmap.md) and [docs/17-deployment.md](docs/17-deployment.md#release-strategy)
for how `release/*` branches, semantic versioning, and changelog generation work.
