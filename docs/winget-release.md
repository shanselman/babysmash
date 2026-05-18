# Winget release checklist

This repo publishes a signed Windows installer on every `v*` tag. Winget
publishing adds a generated manifest submission PR to `microsoft/winget-pkgs`.

## Package identity

- PackageIdentifier: `Hanselman.BabySmash`
- PackageName: `BabySmash!`
- Moniker: `babysmash`
- Publisher: `Scott Hanselman`
- Installer asset: `BabySmash-Setup.exe`

There is an older community package named `SillyKoalaHugs.BabySmash`; this
workflow intentionally uses the Hanselman publisher namespace to match
`Hanselman.WingetTUI`.

## Required secret

The automated workflow uses a repository secret named:

```text
WINGET_CREATE_GITHUB_TOKEN
```

Use a token that is accepted by the `microsoft/winget-pkgs` organization policy.
Classic PATs with a lifetime longer than 90 days are rejected by Microsoft Open
Source policy.

## Release asset requirements

The Build BabySmash workflow must upload this Windows release asset:

```text
BabySmash-Setup.exe
```

The Winget Submit workflow downloads that installer from the release, calculates
its SHA256, generates a manifest under:

```text
manifests\h\Hanselman\BabySmash\<version>
```

and submits it with `wingetcreate`.

## Normal release flow

1. Cut and publish the GitHub release as usual by pushing a `v*` tag.
2. Confirm the release has the signed `BabySmash-Setup.exe` asset.
3. Run the **Winget Submit** workflow with the release tag, for example:

```powershell
gh workflow run winget-submit.yml --repo shanselman/babysmash --ref main -f tag=v4.1.0
```

4. Watch the workflow:

```powershell
gh run list --repo shanselman/babysmash --workflow "Winget Submit" --limit 5
```

5. Track the resulting `microsoft/winget-pkgs` PR until it is merged.

## Manual fallback

If `wingetcreate submit` is blocked by token policy, generate the manifest from
the same release asset URL and open a PR from a fork of `microsoft/winget-pkgs`.
Validate before opening the PR:

```powershell
winget validate --manifest <manifest-folder>
```

Use manifest schema `1.10.0` for now because it validates cleanly on current
GitHub-hosted Windows runners and matches the `winget-tui` workflow pattern.
