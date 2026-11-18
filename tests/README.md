# Hopaut tests

| Project | Type | Needs a container runtime |
|---------|------|---------------------------|
| `Hopaut.ArchitectureTests` | Layer-table and DDD rules over every `src` project, as a ratchet (plan Step 6.7) | no |
| `Hopaut.BuildingBlocks.UnitTests` | SharedKernel and BuildingBlocks unit tests | no |
| `Hopaut.IntegrationTests` | Real Host (`WebApplicationFactory<Program>`) against a PostGIS container (Testcontainers) | **yes** |
| `Modules/<X>/Hopaut.Modules.<X>.UnitTests` | Domain + Application tests per module (created by the plan's Phase 8 steps) | no |
| `load/` | Legacy k6 scripts, not part of `dotnet test` (finding F-051) | – |

Stack: xUnit v3 (3.2.x, VSTest mode), AwesomeAssertions, NSubstitute, `FakeTimeProvider`, Testcontainers. Naming: `GivenPrecondition_WhenAction_ThenExpected`, with `// Arrange`, `// Act`, `// Assert`.

`HopautApiFactory` gives the test Host a test-only JWT signing key; `TestJwt.Create(userId, roles, …)` issues matching bearer tokens (and invalid variants: other issuer, audience or key, expired) for endpoint tests. `TestCaller.SendAsync(services, TestCaller.For(userId, roles), command, ct)` sends a command straight to the mediator as that caller (the principal `ICurrentUser` and `IResourceAuthorizer` read); pass `null` to send it like background work, without a caller.

## Running

```powershell
dotnet test Bingo.sln                                   # everything (needs a container runtime)
dotnet test tests/Hopaut.ArchitectureTests              # no container needed
dotnet test tests/Hopaut.BuildingBlocks.UnitTests       # no container needed
dotnet test <project> --no-build --filter "FullyQualifiedName~<Class>.<Method>"
```

## Container runtime for the integration tests

Testcontainers needs a Docker-compatible API. Integration tests fail (they are never skipped) when none is reachable.

- **Docker Desktop:** nothing to configure.
- **Podman (Windows):** `podman machine start` normally prints an API-forwarding pipe; set `DOCKER_HOST` to it and disable the resource reaper:
  ```powershell
  $env:DOCKER_HOST = 'npipe:////./pipe/podman-machine-default'
  $env:TESTCONTAINERS_RYUK_DISABLED = 'true'
  ```
- **Podman when API forwarding fails** (`win-sshproxy.exe failed to start`): serve the API inside the machine on localhost only, then point Testcontainers at it:
  ```powershell
  podman machine ssh "sudo systemd-run --unit=hopaut-podman-tcp podman system service --time=0 tcp://127.0.0.1:2375"
  $env:DOCKER_HOST = 'tcp://127.0.0.1:2375'
  $env:TESTCONTAINERS_RYUK_DISABLED = 'true'
  # stop it afterwards: podman machine ssh "sudo systemctl stop hopaut-podman-tcp"
  ```

With the resource reaper disabled, fixtures dispose their containers; check `podman ps -a` after an interrupted run.
