# Architecture

The application is organized into three production projects. Dependencies are checked by `ArchitectureTests`.

```mermaid
flowchart LR
    UI[WinForms: views, themes, resources] --> Core[Core: models, presenters, rules, contracts]
    UI --> Infrastructure[Infrastructure: ADB, XML, JSON]
    Infrastructure --> Core
```

## Core

`src/AdbFileManager.Core` targets .NET 8 without Windows Forms or shell dependencies.

- `Browsing`: immutable file/device metadata, the selected device session, the browser/view contracts and `BrowserPresenter`. The presenter owns readiness, path validation, request cancellation and rejection of stale results.

`BrowserFileFilter`, `BrowserSelection` and `AndroidBreadcrumb` keep filtering, selection restoration and path-segment rules independent of controls. SD discovery belongs to the browser boundary: Infrastructure probes storage on the captured device and the presenter rejects stale discovery results.
- `Transfers`: the queue and state transitions, conflict decisions, progress/statistics models, backend/store contracts and the batch factory. Transient failures are represented by `TransferException`, independent of the ADB implementation.
- `Settings`: the settings model, validation, persistence contract and `SettingsService`.

The presenter reports typed statuses. It does not translate messages, draw controls or show dialogs. Its view notifications retain the caller's synchronization context; WinForms calls it from the UI thread.

## Infrastructure

`src/AdbFileManager.Infrastructure` references Core only.

- `Adb`: the injected `IAdbClient`, per-instance process runner, concrete Android browser, parsers, transfer backend and media-preview temporary storage.
- `Persistence`: XML settings with atomic replacement/recovery, and JSON queue storage.

ADB output interpretation and protocol-specific failure classification stay here. Settings XML names are explicitly mapped with `XmlElement` so property renames do not discard existing values. Retired, inactive options are ignored when reading older XML.

## WinForms

`src/AdbFileManager.WinForms` owns the application entry point, resource files, controls and views. `Program.cs` is the composition root: it constructs shared services, injects them into the main window, and disposes icons/preview storage when the application ends.

`MainForm.cs` contains construction and window lifetime. Focused partial files contain layout, local shell navigation, Android view rendering, UI event handlers and transfer-window coordination. The browser's decisions are in `BrowserPresenter`, not in these partials. Selected rows contain the original immutable `AndroidFile` metadata, so sorting and rendering cannot change the copy source.

The settings, APK, wireless and unlock dialogs receive their dependencies through constructors. No dialog consults a global main-window instance or a global selected phone. `AppTheme` owns theme application; `IconProvider` owns and disposes cached assets, using absolute paths. `LocalizationText` is a stateless resource helper.

View namespaces and resource logical names intentionally remain compatible with Windows Forms resource lookup. Moving a view or renaming a control requires updating its Designer/resource keys together. Core and Infrastructure use feature namespaces.

## Tests and development

- Unit/integration checks reference the production Core and Infrastructure projects. No production `.cs` files are copied into the test compilation.
- `InternalsVisibleTo` exposes implementation helpers only to their test assemblies.
- `scripts/ui-smoke.ps1` runs STA window checks using injected fake services and writes screenshots to `artifacts/ui-checks`. It covers resources, translations, read-only file rows, loading, cancellation, disconnects and dialogs without changing user settings or requiring a phone.
- Workspace checks also cover filter/refresh selection, discovered SD paths, copy eligibility, reconnection actions, retry/collapse of the integrated transfer panel and usable pane bounds at layout scales of 125%, 150% and 200%. These layout checks complement real-window visual inspection; they do not simulate a physical monitor DPI change.
- `scripts/device-smoke.ps1` runs the opt-in physical device suite.
- `.editorconfig` defines whitespace and naming-independent formatting. Use `dotnet format whitespace AdbFileManager.sln --verify-no-changes --no-restore` to check formatting.

Add behavior to the relevant service or presenter first, test it without controls, then connect the UI. Add interfaces at boundaries that need substitution; use ordinary methods for pure calculations and formatting.
