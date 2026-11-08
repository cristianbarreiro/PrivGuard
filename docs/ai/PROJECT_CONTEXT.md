---
schema: privgvard.project-context.v1
kind: canonical-open-knowledge
product_name: PrivGvard
repository_name: PrivGuard
current_code_identity: PrivLock
status_date: 2026-11-08
source_of_truth: repository-code-and-tests
primary_supported_platform: Windows
target_platforms: [Windows, Linux, macOS]
---

# PrivGvard — Canonical Project Context (OKF)

## 1. Purpose and product identity

PrivGvard is a transparent, reversible privacy utility designed to provide users with explicit control over their camera and microphone without modifying hardware firmware, device registers, kernel binaries, security subsystems, or driver signature enforcement.

### Public Product Identity vs. Internal Historical Identifiers
- **Public Product Identity**: **PrivGvard** (Version 2.0.0).
  - Executable: `PrivGvard.exe` on Windows; `PrivGvard` on Linux and macOS.
  - AssemblyName, Product, and Title: `PrivGvard` (configured in [PrivLock.Desktop.csproj](../../src/PrivLock.Desktop/PrivLock.Desktop.csproj)).
  - Storage directory: `%LOCALAPPDATA%\PrivGvard` (Windows), `~/.local/share/PrivGvard` (Linux), `~/Library/Application Support/PrivGvard` (macOS), with automated migration from legacy `PrivLock` directories via `StorageMigrationHelper`.
  - Logging directory: `%LOCALAPPDATA%\PrivGvard\Logs\`.
  - Crash reports: `%LOCALAPPDATA%\PrivGvard\CrashReports\`.
  - Windows installer: `PrivGvard-Setup-2.0.0.exe` (built via [build-installer.ps1](../../build-installer.ps1) with Inno Setup 6).
  - Portable distribution: `PrivGvard-Portable-2.0.0.zip`.
  - CI build artifacts: `PrivGvard-win-x64`, `PrivGvard-win-arm64`, `PrivGvard-linux-x64`, `PrivGvard-osx-arm64`.
- **Internal Historical Code Identifiers**:
  - Repository name: `PrivGuard` (historically `Cam&MicroBlocker`, GitHub: `cristianbarreiro/PrivGuard`).
  - Solution file: `PrivGvard.sln` (formerly `CamMicBlocker.sln`).
  - Namespaces and projects: `PrivLock.*` (`PrivLock.Domain`, `PrivLock.Platform.Abstractions`, `PrivLock.Infrastructure.Common`, `PrivLock.Application`, `PrivLock.UI`, `PrivLock.Platform.Windows`, `PrivLock.Platform.Linux`, `PrivLock.Platform.MacOS`, `PrivLock.Desktop`).
  - Registry keys: `SOFTWARE\PrivLock\PrivilegedOwnership\v2`.
  - IPC/Mutex primitives: `Global\PrivLock_SingleInstance`, `Global\PrivLock_UninstallGate`.
  - *Note*: Internal identifiers remain intentionally stable for code continuity, backward compatibility, and atomic migration safety.

The requested product name is **PrivGvard**. The repository is named `PrivGuard` (historically `Cam&MicroBlocker`, GitHub: `cristianbarreiro/PrivGuard`), the solution is `PrivGvard.sln` (historically `CamMicBlocker.sln`), namespaces and assemblies use `PrivLock`, and the Windows desktop host produces `PrivGvard.exe` / `PrivGvard`. This naming convention reflects code continuity and branding alignment, not an instruction to rename code automatically.

## 2. Current verified state

| Area | Verified truth | Confidence |
|---|---|---|
| Runtime | .NET 10, Avalonia 11.2.3, single desktop host at `src/PrivLock.Desktop` | High |
| Architecture | Domain, abstractions, common infrastructure, application, platform providers, UI and desktop composition root | High |
| Windows | Durable privacy session, JSON journal, ownership attestation, authenticated on-demand privileged worker, PnP/registry/audio orchestration and recovery tests exist | High for unit/integration simulation; hardware/UAC behavior still needs isolated VM acceptance |
| Linux | Detector/controller scaffolding exists, but capability provider reports `None`; persistent recovery adapter is unsupported | High |
| macOS | Detector/controller scaffolding exists, but capability provider reports `None`; persistent recovery adapter is unsupported; secure provider methods are currently no-op success paths | High |
| Tests | All active test suites pass across Domain, Infrastructure, Application, and Windows Platform | High |
| CI | GitHub Actions defines Windows/Ubuntu/macOS build-test matrix, selected publish targets, and Windows Release-Store MSIX packaging gate (verified on pull_request runs #86/#87 for PR #11 [head_branch: feature/sprint-1-store-profile-guards-d01-d02], push runs #89–#103 on main [run #99 on 4d674ebd933f70edd2a1be789df2e3414e1b59f0, run #100 on 86d6adcef9234b579e9313215673cf540e9fa52b, run #101 on 85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a, run #102 on e6a57ae58d7d0cd30265834fc51e2bab32928732, run #103 on f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5], and PR #12 branch runs #104 [commit 7d76698d81603d36c921f77105148f1443e09ddd], #105 [commit b606a4ad28cabf3bf1dc5104ff3382c6ebdd1626], #106 [commit 12ee0407f3b80f5de1303141553529b0e305b2cc], and #107 [commit 1cd0783ffb9ef6b15af9e64cdc3259b60678341a]) | High; post-PR #11 commits have run on push to main or branch runs on PR #12, native privacy/VM dynamics require isolated VM |
| Legacy code | Archived at `legacy/CamMicBlocker` and `legacy/tests/CamMicBlocker.Tests`; build-guarded, not part of active solution | High |

### Active Host vs Legacy Quarantine
- **ACTIVE HOST**: `src/PrivLock.Desktop` (produces `PrivGvard.exe`).
- **LEGACY**: `legacy/CamMicBlocker` (archived PrivLock 1.x implementation with build guard).
- **NORMAL BUILDS MUST NEVER BUILD LEGACY CODE.**

The current build succeeds with `dotnet build PrivGvard.sln --no-restore`. A concurrent restore attempt can fail because of a NuGet scratch-lock collision; that is an environment/parallelism issue and must not be reported as a product code failure without reproducing a serialized restore.

## 3. Architecture map

```text
src/
├── PrivLock.Domain/                  # Pure models, capabilities, results and session state
├── PrivLock.Platform.Abstractions/   # Device, protection, elevation, state and system contracts
├── PrivLock.Infrastructure.Common/   # JSON state/session storage, logging, crash reporting, localization
├── PrivLock.Application/             # Serialized use cases, privacy journal orchestration and recovery
├── PrivLock.Platform.Windows/        # Windows registry, PnP, Core Audio and authenticated worker
├── PrivLock.Platform.Linux/          # Linux discovery and experimental controller scaffolding
├── PrivLock.Platform.MacOS/          # macOS discovery and experimental controller scaffolding
├── PrivLock.UI/                      # Avalonia views, view models, localization, custom chrome
└── PrivLock.Desktop/                 # Single executable host (PrivGvard.exe), DI composition root, worker entry point

tests/
├── PrivLock.Domain.Tests/            # Cross-platform domain logic tests
├── PrivLock.Infrastructure.Tests/    # Cross-platform storage, crash reporting & localization tests
├── PrivLock.Application.Tests/       # Cross-platform application orchestration & recovery tests
└── PrivLock.Platform.Windows.Tests/  # Windows-specific PnP, registry, audio, worker & IPC tests

legacy/                               # Quarantined archived legacy code (build-guarded)
├── CamMicBlocker/
└── tests/CamMicBlocker.Tests/
```

### Active Host vs. Legacy Quarantine
- **ACTIVE HOST**: `src/PrivLock.Desktop` (compiles as `PrivGvard.exe` / `PrivGvard`).
- **LEGACY**: `legacy/CamMicBlocker` (archived PrivLock 1.x WPF code with compile-time build guard).
- **RULE**: Active builds and CI must never reference or build legacy files.

---

## 4. Window and System Tray Lifecycle

PrivGvard features an integrated desktop and notification-area lifecycle:

1. **Title-Bar Close Button (`X`) & `Alt+F4`**:
   - Intercepted by `MainWindow.OnClosing` (`e.Cancel = true`).
   - Hides `MainWindow` (`Hide()`) and removes the application from the Windows taskbar.
   - The process remains running in the background with its System Tray icon active.
2. **System Tray Icon**:
   - Left-click or double-click: calls `ShowMainWindow()` (`Show()`, `WindowState = Normal`, `Activate()`, `BringIntoView()`).
   - A normal Windows launch while an instance is already running forwards an activation request so the existing window is shown instead of silently exiting.
   - Right-click context menu: localized options dynamically bound via `LocalizationCatalog`:
     - **Abrir PrivGvard** / **Open PrivGvard**: restores the main window.
     - **Salir** / **Exit**: initiates clean application shutdown.
3. **Application Shutdown (`Salir` / `Exit` / OS Shutdown)**:
   - Initiates centralized teardown through `ShutdownCoordinator.RestoreAsync()`.
   - Reverses only confirmed PrivGvard-owned changes according to the persistent WAL journal.
   - Preserves external conflicts.
   - Closes open child windows, disposes the tray icon, and terminates the process cleanly.

---

## 5. Safety invariants

These invariants are mandatory for every implementation and AI-assisted change:

1. **Start Unelevated (`asInvoker`)**: The application process runs as a standard user process by default.
2. **On-Demand Elevation**: Elevation is requested transiently and strictly for the exact privileged action authorized by the user.
3. **Single Application Binary**: No permanent background service and no second elevated executable (`PrivLock.Elevated.exe` is strictly prohibited).
4. **Authenticated Worker**: Privileged operations execute via self-invocation (`PrivGvard.exe --privileged-worker <pipe> <pid> <nonce>`) using Windows UAC `runas`. IPC uses random named pipes, strict parent PID validation, 256-bit cryptographically random nonces, and a closed command whitelist (`apply-policy`, `restore-policy`, `apply-device`, `restore-device`, `verify-policy-ownership`, `verify-device-ownership`, `ping`). The legacy public `--privileged-exec` dispatcher is permanently removed.
5. **No Low-Level Hardware Tampering**: Never touch firmware, EEPROM, device registers, kernel driver binaries, SIP, UAC bypasses, Defender, or driver signature enforcement.
6. **Verified Effective State**: Never report a device or resource as `Blocked` or `Protected` without a fresh native post-mutation observation (`EffectiveStatus`). Desired state is never evidence of effective protection.
7. **Durable Journal & Reversibility**: Capture exact original state (existence, type, value, mute/volume, problem state) before mutation. Journal intent in Write-Ahead Logging (`privacy-session-v1.json`). Restore only confirmed session-owned resources.
8. **Preserve External Conflicts**: If a native resource was modified externally outside the session, preserve the external state as a conflict rather than overwriting it blindly.
9. **Fail Securely**: Treat timeouts, command failures, missing tools, permission denials, and verification failures as explicit operation errors. Never swallow exceptions or convert errors into `OperationResult.Ok()`.
10. **Sanitized Diagnostics**: Do not log raw user paths, credentials, or sensitive device details in rolling logs or crash dumps. Use operation IDs, anonymized metadata, and hashed identifiers.

---

## 6. Capability truth by platform

| Platform | Camera Protection | Microphone Protection | Persistent Recovery | Status / Posture |
|---|---|---|---|---|
| **Windows** | Dual-layer: AppPrivacy Group Policy + verified PnP device node disable (`CfgMgr32.dll`) | Dual-layer: AppPrivacy Group Policy + verified PnP node disable + Core Audio endpoint mute lock | Fully implemented via `WindowsPrivacySessionPlatformAdapter` and WAL journal | **Supported** (Primary production candidate) |
| **Linux** | Unsupported (`CapabilityLevel.None`) | Unsupported (`CapabilityLevel.None`) | `UnsupportedPrivacySessionPlatformAdapter` | **Discovery & UI Only** (Mutation experimental/unsupported) |
| **macOS** | Unsupported (`CapabilityLevel.None`; TCC cannot silently revoke permissions) | Unsupported (`CapabilityLevel.None`) | `UnsupportedPrivacySessionPlatformAdapter` | **Discovery & UI Only** (Mutation experimental/unsupported) |

---

## 7. Known audit status & resolved findings

- **Cross-Platform CI Topology**: Resolved. CI split separates shared tests (Domain, Application, Infrastructure) from Windows-only platform tests.
- **System Tray Localization**: Resolved. Dynamic localized headers for tray items ("Abrir PrivGvard" / "Open PrivGvard", "Salir" / "Exit") update on language change.
- **Title-Bar Close Button Styling**: Resolved. Fluent Dark hover/pressed states with distinct red background and white glyph.
- **Executable & Metadata Branding**: Resolved. Active desktop binary compiles as `PrivGvard.exe` / `PrivGvard` with official metadata.
- **Storage Migration**: Resolved. `StorageMigrationHelper` safely migrates `%LOCALAPPDATA%\PrivLock` data to `%LOCALAPPDATA%\PrivGvard`.
- **Legacy Quarantine**: Resolved. Archived WPF code quarantined under `legacy/CamMicBlocker` with compile-time build guard.
- **Linux & macOS Mutation Guarding**: Maintained. Capability providers explicitly declare `CapabilityLevel.None` and register `UnsupportedPrivacySessionPlatformAdapter`.
- **Volatile Test Counts**: Removed from canonical documentation in favor of dynamic test suite validation.
- **Release-Store MSIX CI Gate (Sprint 1.2)**: Implemented in `.github/workflows/ci.yml`. Builds `Release-Store`, generates x64 MSIX via `build-msix.ps1`, and validates internal `AppxManifest.xml` (`cdevstudios.PrivGuard`, `runFullTrust` sole restricted capability, no elevation) via `StoreMsixManifestVerifier` and `verify-store-msix.ps1`. Validated in CI pull_request runs #86 and #87 (PR #11, `head_branch`: `feature/sprint-1-store-profile-guards-d01-d02`, base: `main`) and push runs #89 (ID: `42379421580`), #90 (ID: `42384884759`), #91 (ID: `42390732091`), #92 (ID: `42396473407`), #93 (ID: `42410227273`), #94 (ID: `42417187681`), #95 (ID: `42420318570`), #96 (ID: `42426468347`), #97 (ID: `42428357398`), #98 (ID: `42429999278`), #99 (ID: `42432585253`), #100 (ID: `42435675882`), #101 (ID: `42438637184`), #102 (ID: `42444149346`), and #103 (ID: `42448487384`). Comprobación técnica de empaquetado; no sustituye validación en VM ni autoriza publicación (Microsoft Store permanece en `CONDITIONAL HOLD`).
- **VM Lab Readiness, Reconciliación Post-Push R7, Reparación de Integridad de Evidencia, Restauración UTF-8, Endurecimiento L0 y Pre-Merge Gate (Fase 8B.1A.7.2 / R8, R8.1, R8.2, R8.3 y R8.3.1)**: Completed post-push reconciliation of commit `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5` on canonical remote HEAD `origin/main` via CI Run #103 (ID: `42448487384`, event `push`, 7/7 jobs green, 4 single-file artifacts published, no MSIX or build provenance artifact). Repaired report source integrity in R7 by replacing alien code snippets (`ValueStopwatch`) with real production code from `WindowsUninstallRequestListener.cs`, correcting public (5-parameter) and internal signatures, rectifying Run #102 job IDs and .NET 10 SDK/runtime versions, and restoring the real test inventory (16 methods in `WindowsUninstallRequestListenerTests.cs` and 4 in `WindowsSingleInstanceStartupTests.cs`). Audited the timing boundary of `TryRequestRunningInstanceAndAcquire` with formal verdict `SOFT DEADLINE ACCEPTED — SEMANTICS DOCUMENTED` (monotonic polling-bounded timeout without sub-millisecond hard guarantees; zero production code churn). Hardened L0 physical host preflight tooling (`scripts/lab/Get-L0HostPreflight.ps1`) by eliminating premature readiness labels, adding explicit `-VmStoragePath` target volume verification, separating `UNKNOWN` from `FALSE`. In R8.1, opened PR #12 and remediated code reviews by extracting classification logic to `scripts/lab/L0HostPreflight.Logic.psm1`, fixing newline escape expansion, eliminating absolute file paths, expanding unit tests to 28 deterministic cases (Run #105, ID: `42488518496`, 7/7 jobs green, 28/28 tests passing). In R8.2, restored UTF-8 byte integrity across reports from clean parent blobs (`7d76698d81603d36c921f77105148f1443e09ddd`), eradicated mojibake, normalized links to relative paths, established strict `-VmStoragePath` local drive validation, conservative multi-socket aggregation, diagnostic `VMMonitorModeExtensions` handling for Hyper-V physical hosts, and expanded unit tests to 44/44 PASS (Run #106, ID: `42499040076`, 7/7 jobs green). In R8.3, implemented comprehensive fail-closed boundary hardening: strict multi-socket CPU aggregation (partial/null/negative cores fail-closed by returning null total cores/processors and incomplete inventory), strict boolean allowlist (`ConvertTo-L0NormalizedBoolean`), strict numeric double/int validation (`Test-L0ValidDouble`, `Test-L0ValidInt`) rejecting NaN, Infinity, and negative values, and strict target directory resolution (`Test-L0StorageTargetResolution`) requiring directory existence and local fixed disk (`DriveType = 3`). Expanded hermetic unit tests to 87 deterministic cases (87/87 PASS). In R8.3.1, reconciled remote evidence with published commit `1cd0783ffb9ef6b15af9e64cdc3259b60678341a` and verified GitHub Actions CI Run #107 (ID: `42540424299`, 7/7 jobs green, Windows job `128913895043` reporting 87/87 PASS). Documented that while all review issues are 100% remediated in code, the three GitHub review threads remain `outdated=true` and `is_resolved=false` awaiting formal resolution by the operator or integration with write permissions. Noted that branch protection returned 403 on API query. PR #12 remains open and mergeable, with formal status `TECHNICALLY GREEN — MERGE POLICY NOT VERIFIED` (0 production code edits in `src/`, 0 edits in `tests/`, zero merge executed). Harmonized Topology B runbook (`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`) with official Microsoft Windows 11 VM requirements (>= 64 GiB storage) and hypervisor taxonomy (Type 1 native vs Type 2 hosted). Host preflight remains `GUEST DETECTED — L0 NOT VERIFIED`; dynamic VM testing (TEST-01 to TEST-11, including TEST-09.7) held in `LAB HOLD / NOT RUN`. Microsoft Store in `CONDITIONAL HOLD`; Phase 8B.1C in `BLOCKED / NOT AUTHORIZED`. Documented in `docs/reports/INFORME-FASE-8B1A7_2-R8_3-FAIL-CLOSED-PR12-FINAL-GATE.md`, `docs/reports/INFORME-FASE-8B1A7_2-R8-R7-EVIDENCE-INTEGRITY-L0-PREFLIGHT.md`, `docs/reports/INFORME-FASE-8B1A7_2-R7-POST-R6-RECONCILIATION-L0-READINESS.md`, and `docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md`.

---

## 8. Documentation precedence

When documentation and code conflict, follow this strict hierarchy:

1. **Current source code and passing tests** (CODE WINS; TESTS WIN).
2. **This file ([PROJECT_CONTEXT.md](PROJECT_CONTEXT.md))** and root safety rules ([AGENTS.md](../../AGENTS.md)).
3. **[AUDIT-ROADMAP.md](AUDIT-ROADMAP.md)** and **[recovery-validation.md](../recovery-validation.md)**.
4. **Public README** ([README.md](../../README.md)).

AI agents must identify and flag contradictions explicitly rather than assuming outdated statements are correct.
