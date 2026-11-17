# CtrlIt Work Timer

**A tiny always-on-top timer that shows how long you've been working, beeps every 15 minutes, and can reset itself whenever you step away from your PC.**

CtrlIt Work Timer is a small window that sits on top of everything else. Start it when you sit down, or let it start itself when you log in, and glance at it to see how long you've been at it.

## Features

- **Start, Stop and Reset** buttons and a large HH:MM:SS display.
- **Always on top**, so it stays visible over your other windows. You can turn this off in the right-click menu.
- **Adjustable transparency**, from 20% to 100%, with the slider on the window.
- **A beep every 15 minutes** of counted time (at 15, 30, 45 minutes and so on).
- **No title bar.** Drag the window from anywhere on its background, close it with the × in the corner, or right-click it and choose Exit.
- **Auto mode** (in the right-click menu):
  - resets the timer when you lock the PC, log off, put it to sleep or hibernate it, or shut it down
  - starts counting when you log in, unlock, or wake the PC
  - launches CtrlIt Work Timer when you log in to Windows
- **Remembers** its position, transparency and settings.
- **One .exe, no installer.** Settings are kept in `%APPDATA%\CtrlItWorkTimer\settings.ini`.

## Download

Get `CtrlItWorkTimer.exe` from the [latest release](../../releases/latest), put it in a permanent folder and run it. Right-click the timer for **Stay on top**, **Auto** and **About**.

Windows may show a SmartScreen warning because the program isn't signed. Click **More info**, then **Run anyway**.

Requires Windows 10 or 11 with .NET Framework 4.x, which is built in.

## Privacy

CtrlIt Work Timer makes no network connections and collects nothing. It only listens for Windows' own lock, unlock, sleep and wake notifications so that Auto mode can reset and restart the timer.

## Building from source

No Visual Studio needed. The C# compiler that ships with Windows is enough:

```
build.bat
```

This compiles `CtrlItWorkTimer.cs` into `CtrlItWorkTimer.exe` and starts it. Compiler output is saved in `build.log`. `build.bat ci` only compiles, which is what the release workflow uses.

## Releases

Publishing a release on GitHub (for example with tag `v1.0.0`) runs the workflow in `.github/workflows/release.yml`, which builds `CtrlItWorkTimer.exe` on a clean Windows machine and attaches it to that release.

## Support the project

CtrlIt Work Timer is free and open source. If it's useful to you, you can support its development via [Ko-fi](https://ko-fi.com/ctrlittools) or the **Sponsor** button at the top of this page.

## Licence

[MIT](LICENSE)
