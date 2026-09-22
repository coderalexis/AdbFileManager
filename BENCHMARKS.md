# Physical-device transfer benchmarks

These measurements exercise the production `AdbTransferBackend.CopyAsync` path on a Samsung SM-G780G running Android 13 over USB. The bundled ADB version was `37.0.1-15733141`.

## Cable and storage comparison

The same 142,451,734-byte (135.9 MiB) video was pulled once from the phone's microSD card and once from internal storage. This separates the USB/ADB path from removable-storage read performance.

| Source | Copy time | Throughput |
|---|---:|---:|
| microSD (`/storage/<SD-card>/DCIM/camera15`) | 60.50 s | **2.25 MiB/s** |
| Internal storage (`/sdcard/Download`) | 3.61 s | **37.63 MiB/s** |

Internal storage was approximately **16.7× faster**. The faster cable and the existing ADB/backend path sustained at least **37.63 MiB/s**; the removable card limited the camera-folder transfer.

## Real camera-folder pull

| Files | Total bytes | Size | Copy time | Throughput |
|---:|---:|---:|---:|---:|
| 151 | 5,450,776,535 | 5.076 GiB | 2,159.45 s (35:59) | **2.41 MiB/s** |

The detailed folder-test evidence is recorded in [`tests/REAL_FOLDER_RESULT_2026-09-21.md`](tests/REAL_FOLDER_RESULT_2026-09-21.md).

These results show why transfer speed should be displayed in the application and why storage type needs to be considered when diagnosing performance.
