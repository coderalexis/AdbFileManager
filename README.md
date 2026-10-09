# AdbFileManager — Community Fork

This repository is a community fork of the original [AdbFileManager](https://github.com/T0biasCZe/AdbFileManager) project created by [T0biasCZe](https://github.com/T0biasCZe).

Many thanks to the original author for designing and publishing the C# Windows Forms application that made this fork possible. This edition keeps the original ADB file-manager foundation and builds on it with transfer reliability, a persistent queue, physical-device validation, performance measurements, and further usability improvements.

The code is organized into **Core**, **Infrastructure** and **WinForms** projects under `src/`. See [ARCHITECTURE.md](ARCHITECTURE.md) for responsibilities, dependencies and the testing workflow.

AdbFileManager is an alternative to MTP that uses the ADB protocol to copy files between Windows and Android. The original project reported approximately 41.6 MiB/s (332 Mib/s) over USB 2.0, compared with MTP at around 10 Mb/s.
![image](https://github.com/user-attachments/assets/c2571d20-c27c-4aa7-b450-5809223589ef)

## Measured performance on a physical device

This fork measured **37.63 MiB/s** from internal phone storage with a higher-speed USB cable. The same file reached **2.25 MiB/s** from a microSD card, identifying the removable card as the limiting component. A real 151-file, 5.076 GiB camera folder completed at **2.41 MiB/s**.

See [`BENCHMARKS.md`](BENCHMARKS.md) for the test conditions and full statistics.

**To use this app, you must have enabled USB Debugging in android developer settings.**        
After enabling it, open the source and destination folders, select files and choose **Download to PC** or **Send to Android**.

Note2: Its made in C# and Windows Forms. To put it on Linux/Mac I would completely have to rewrite it, so no Linux/Mac version

## Start the application

This fork's portable package targets **Windows 10/11 x64** and includes the .NET runtime.

1. Download `AdbFileManager-win-x64` from the artifacts of a successful [GitHub Actions build](https://github.com/coderalexis/AdbFileManager/actions).
2. Extract the artifact, then extract `AdbFileManager-win-x64.zip` into a folder.
3. Double-click `Iniciar.cmd` or `AdbFileManager.exe`. Keep the bundled ADB, DLLs and icon folders alongside the executable.

If you have the source repository, double-click **`Iniciar.cmd` in the repository root**. It compiles Release x64 and starts the app using `.tools/dotnet` when available, otherwise the installed .NET SDK. Building from source requires the **.NET 8 SDK**; see [DEVELOPMENT.md](DEVELOPMENT.md).

Ordinary builds without a bundled runtime require **.NET 8 Desktop Runtime x64** for x64 builds, or **x86** for the original Any CPU build. Use the matching architecture from the [official .NET download page](https://dotnet.microsoft.com/download/dotnet/8.0).

The portable package is a [self-contained .NET deployment](https://learn.microsoft.com/dotnet/core/deploying/), so it does not require a separate runtime installation.

# basic controls:    
 * To enter a folder:
   * double click the folder with a left mouse button
   * or press Enter on the keyboard
   * to open a typed Android path, press Enter in the path field
 * to go up a directory:
   * press backspace on the keyboard
   * or double click the header of the file list   
 ![This is the header](https://github.com/user-attachments/assets/ed947c01-e4a0-4374-8359-16f603f63601)
 * to copy files:
   * select the file(s) in you want to copy on the android side or the windows size, and then click the arrow button with the wanted direction



# Video tutorial:
[![Tutorial](https://i3.ytimg.com/vi/3_mgQWvYvE4/hqdefault.jpg)](https://youtu.be/3_mgQWvYvE4)

## File listings and connection status

Android navigation runs asynchronously. A new request cancels the previous one. Empty folders, disconnected or unauthorized devices, and listing errors have separate messages. The Android table is read only.

If Android returns an unsupported listing format, enable compatibility mode. Its file-type checks use actual metadata rather than guessing from filename extensions.

## Browsing and transfer workspace

- **Android** and **This PC** have separate headers, paths and selection summaries. The toolbar shows the device and its connection state.
- **Download to PC →** and **← Send to Android** describe the copy direction. The selection preview and button tooltips show the destination; copying is enabled only for a valid selection and destination.
- Android has clickable path segments, an **Up** button and shortcuts to **DCIM**, **Downloads** and the first readable SD volume under `/storage/????-????`. If no card is accessible, the app explains how to check again.
- The Android filter matches names immediately, without another ADB listing. Refresh retains surviving selections only within the same folder and device; filtered-out files cannot be copied accidentally.
- Transfers appear in a collapsible panel in the main window, with progress, elapsed time, pause, cancel and retry controls. **View details** opens the full queue, statistics and error details.
- Connection and listing problems offer **Check again** and, when available, **View details**. Clearing completed or cancelled entries removes history, without deleting files.
- The workspace uses consistent buttons, English/Spanish text, accessible control names and monitor DPI scaling. `F5` refreshes Android, `Ctrl+F` focuses its filter, `Ctrl+L` focuses the active pane's path and `Alt+Up` opens its parent. `Esc` clears the focused filter.

The old button-style and optional Android Back-button settings are retained in saved configuration for compatibility; the new workspace uses one consistent button design and always provides **Up**.

## Transfer statistics

The transfer queue shows elapsed time while copying and bytes transferred plus average MiB/s after a successful transfer when ADB reports its summary. Average speed uses the ADB process duration. Overall elapsed time also includes preparation, conflict decisions and retries. Missing statistics are shown as `—`; live speed and ETA are not estimated from unreliable percentages.

Statistics are persisted with the queue and included in its JSON export.

# Settings:  
Use **Save and close**, or close the Settings window, to persist changes. Appearance changes apply after restarting the app. Settings are written atomically with a backup; damaged XML is preserved and recovered from a backup or defaults when possible.

***Keep file modified date***     
This keeps the file modification date of the file thats being copied, instead of changing it to the time when it was copied    
***Preview files on double click***  
When double clicking media file (video or photo), it will temporarily copy it to the PC and show it. When the program is closed, it will automatically delete the temporary file from PC.      
***Compatibility fix***        
This fixes files being shown on very old Android versions, which dont support the ls -l flag     


# FAQ:    
**Q**: I open the program but it instantly closes    
***A***: Make sure you are opening AdbFileManager.exe, and not adb.exe      
**Q**: Files are not showing
***A***: If you have phone with older android, enable compatibility mode

# Windows 7 compatibility
* The program works on Windows 7, but you need to install .NET 8 using VxKEX NEXT kernel extension.
* However, dark mode doesnt work, and it breaks the embeded Windows Explorer. Do not use!
