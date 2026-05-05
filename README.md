<div align="center">
  <img width="128" height="128" alt="ManagedBass logo" src="https://github.com/user-attachments/assets/834c571c-6fba-4d6c-a203-9774a456424e" />

  # ManagedBass

  **Free, open-source, cross-platform .NET wrapper for the [Un4seen BASS audio library](https://www.un4seen.com) and its add-ons.**

  [![NuGet version](https://img.shields.io/nuget/v/ManagedBass.svg?label=NuGet&logo=nuget)](https://www.nuget.org/packages/ManagedBass/)
  [![NuGet downloads](https://img.shields.io/nuget/dt/ManagedBass.svg?label=Downloads&logo=nuget)](https://www.nuget.org/packages/ManagedBass/)
  [![Build](https://github.com/winnerspiros/ManagedBass/actions/workflows/build-test.yml/badge.svg)](https://github.com/winnerspiros/ManagedBass/actions/workflows/build-test.yml)
  [![License](https://img.shields.io/github/license/winnerspiros/ManagedBass)](LICENSE.md)
</div>

---

## About BASS

> BASS is an audio library for use in software on several platforms. Its purpose is to provide developers with powerful and efficient sample, stream (MP3, MP2, MP1, OGG, WAV, AIFF, custom generated, and more via OS codecs and add-ons), MOD music (XM, IT, S3M, MOD, MTM, UMX), MO3 music (MP3/OGG compressed MODs), and recording functions. All in a compact DLL that won't bloat your distribution.
>
> — [Un4seen](https://www.un4seen.com)

> [!IMPORTANT]
> **BASS and its add-ons must be downloaded separately from [un4seen.com](https://www.un4seen.com).** This project is a .NET wrapper — it does not include the native libraries.

ManagedBass targets **Any CPU**, but the native BASS libraries (`.dll` / `.so` / `.dylib` / `.a`) come in separate x86, x64, and ARM builds. Download the architecture(s) you need from Un4Seen's website and place them in your build output directory.

- 📖 [Un4Seen BASS documentation](https://www.un4seen.com/doc/) — full reference for the underlying C API (ManagedBass provides a near 1-to-1 mapping)
- 💡 [Sample repositories](https://github.com/ManagedBass) — code examples

---

## Getting Started

### 1. Install the NuGet package

```shell
dotnet add package ManagedBass
```

Or using the Package Manager Console:

```powershell
Install-Package ManagedBass
```

### 2. Download BASS native libraries

Go to <https://www.un4seen.com>, download the BASS zip for your target platform(s), and copy the native library into your project's build output directory.

### 3. Initialize and play

```csharp
Bass.Init();
int stream = Bass.CreateStream("audio.mp3");
Bass.ChannelPlay(stream);
```

---

## Supported Targets

| Target | Notes |
|--------|-------|
| .NET Standard 2.0 | Xamarin, Mono, and other legacy hosts |
| .NET 10.0+ | All modern .NET hosts (desktop, MAUI, etc.) |

### Platforms

| Platform | Supported |
|----------|-----------|
| Windows | ✅ |
| Linux | ✅ |
| macOS | ✅ |
| Android | ✅ |
| iOS | ✅ (dynamic linking, iOS 8+) |

> [!NOTE]
> iOS 8+ supports dynamic linking — you no longer need a dedicated statically-linked build of ManagedBass for iOS.

---

## Add-Ons

Add-ons extend BASS with more audio format support and capabilities. Each add-on is shipped as its own NuGet package (e.g. `ManagedBass.Midi`, `ManagedBass.Fx`).

Add-ons load automatically when you call any method on their class. If you use an add-on's features indirectly through BASS, load it explicitly first — the lightest way is to read its `Version` property.

### Plugin add-ons

A *plugin* hooks into the standard BASS stream/sample creation pipeline to add support for more audio formats (FLAC, OPUS, APE, etc.). Plugins are loaded with `Bass.PluginLoad` and unloaded with `Bass.PluginFree`.

### Platform availability

| Add-On | Plugin | Windows | Linux | macOS | Android | iOS |
|--------|:------:|:-------:|:-----:|:-----:|:-------:|:---:|
| BassAac | ✅ | ✅ | ✅ | | ✅ | |
| BassAc3 | ✅ | ✅ | ✅ | ✅ | | |
| BassAdx | ✅ | ✅ | | | | |
| BassAix | ✅ | ✅ | | | | |
| BassAlac | ✅ | ✅ | ✅ | | ✅ | |
| BassApe | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassAsio | | ✅ | | | | |
| BassCd | ✅ | ✅ | ✅ | | | |
| BassDsd | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassDShow | | ✅ | | | | |
| BassEnc | | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassEnc_Ogg | | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassEnc_Opus | | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassFlac | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassFx | | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassHls | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassMidi | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassMix | | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassMpc | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassOfr | ✅ | ✅ | | | | |
| BassOpus | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassSfx | | ✅ | | | | |
| BassSpx | ✅ | ✅ | ✅ | ✅ | | |
| BassTags | | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassTta | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassVst | | ✅ | | | | |
| BassWA | | ✅ | | | | |
| BassWaDsp | | ✅ | | | | |
| BassWasapi | | ✅ | | | | |
| BassWinamp | | ✅ | | | | |
| BassWma | ✅ | ✅ | | | | |
| BassWv | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| BassZXTune | ✅ | ✅ | ✅ | ✅ | ✅ | |

---

## Contributing

Contributions are very welcome! Please open an issue or pull request. Be patient — this project is maintained in spare time.

ManagedBass is maintained by [@winnerspiros](https://github.com/winnerspiros). It was previously developed by [@olitee](https://github.com/olitee) and [@DustinBond](https://github.com/DustinBond), and originally created by [@MathewSachin](https://github.com/MathewSachin).

---

## Changelog

See the [releases page](https://github.com/winnerspiros/ManagedBass/releases).

