# Physical device smoke test result — 2026-09-21

## Environment

- Product commit: `cf38ea0` (`add reliable transfer queue`)
- Device: Samsung SM-G780G (serial masked as `…W0VY`)
- Android: 13, API 33
- Connection: USB
- Bundled ADB: `37.0.1-15733141`
- Test command: `.\scripts\device-smoke.ps1 -Serial <adb-serial>`

## Result

`PhysicalDeviceTests.TransferBackendAndQueueRoundTripOnAndroidDevice` passed in 4 seconds. VSTest reported one selected test, one passed, zero failed.

The complete suite was then executed with the device enabled: **47 passed, 0 failed, 0 skipped** in 4 seconds (46 process/unit tests plus this physical-device test).

The test verified:

1. Push of a 1 MiB file named `payload teléfono.bin`.
2. Device-side and local SHA-256 equality: `777ecc0180a69108d8bcb7ab08e09d1bd1f23079a66ec0db06622c0f9b241c50`.
3. Pull round trip with the same SHA-256.
4. A conflict without Replace preserved the existing remote file.
5. Explicit Replace produced the replacement hash `3c4efed11b6c95cfbaba02457dfd97cf34b221c0a8e8324485341e8761377d19`.
6. Keep both created `payload teléfono (1).bin` through `TransferQueue`.
7. A nested directory completed a push/pull round trip with its contents intact.
8. The remote scratch directory was removed. A separate ADB check returned `CLEANED`.

## Relevant output

```text
ADB: 37.0.1-15733141
Device: …W0VY
Model: SM-G780G
PASS push 1 MiB Unicode filename: 777ecc0180a69108d8bcb7ab08e09d1bd1f23079a66ec0db06622c0f9b241c50
PASS pull round trip and SHA-256 comparison
PASS conflict without Replace preserved existing data
PASS explicit file replacement: 3c4efed11b6c95cfbaba02457dfd97cf34b221c0a8e8324485341e8761377d19
PASS queue Keep both: payload teléfono (1).bin
PASS nested directory push/pull round trip
RESULT: ALL PHYSICAL DEVICE CHECKS PASSED
PASS remote scratch directory removed

Tests total: 1
Passed: 1
Failed: 0
Total time: 4.6942 seconds
```

The full local transcript is stored under the ignored `artifacts/device-smoke/` directory.
