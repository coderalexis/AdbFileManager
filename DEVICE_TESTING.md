# Physical Android device validation

The automated unit and fake-process tests do not prove that a real Android device accepts every shell and transfer operation. This smoke test exercises the production `AdbClient`, `AdbTransferBackend`, and `TransferQueue` against one authorized device.

## Safety and scope

The test creates a uniquely named directory under `/sdcard/Download/AdbFileManager-smoke-*`. It only reads, replaces, and deletes data created inside that directory. A `finally` block removes the remote and local scratch directories even when an assertion fails.

The device serial is masked in the evidence output. Do not capture notifications or other personal phone content when taking a screenshot.

## Run

1. Connect a test phone and enable USB debugging.
2. Accept the debugging authorization prompt on the phone.
3. Confirm that `AdbFileManager/adb.exe devices -l` shows the device with state `device`.
4. From the repository root run:

```powershell
.\scripts\device-smoke.ps1
```

With multiple devices, select one explicitly:

```powershell
.\scripts\device-smoke.ps1 -Serial <adb-serial>
```

The detailed transcript is saved under the ignored `artifacts/device-smoke/` directory.

## Assertions

The test fails unless all of these checks pass:

1. ADB reports the selected device as `device`.
2. A 1 MiB file with a Unicode filename is pushed and its device-side SHA-256 matches the local hash.
3. Pulling the file back produces the same SHA-256.
4. A conflict without Replace preserves the original remote file.
5. Explicit Replace changes the remote file to the replacement hash.
6. The queue's Keep both policy creates a second remote file and preserves its contents.
7. A nested directory completes a push/pull round trip with its file contents intact.
8. The isolated remote scratch directory is removed.

For review evidence, attach a screenshot showing `RESULT: ALL PHYSICAL DEVICE CHECKS PASSED` and include the transcript. State the phone model, Android version, connection type, ADB version, commit hash, and command used.

The first recorded physical run is documented in `tests/DEVICE_SMOKE_RESULT_2026-09-21.md`.
