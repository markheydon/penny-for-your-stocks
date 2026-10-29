# Release runbook

Maintainer guide for tag-based GitHub Releases. Policy: [VERSIONING.md](../VERSIONING.md).

## Steps

1. Ensure `main` is green on CI.
2. Merge release changes.
3. Tag and push: `git tag v0.1.0 && git push origin v0.1.0`
4. Confirm the Release workflow and GitHub Releases page.

NuGet publishing is not configured because this is an application rather than a package.
