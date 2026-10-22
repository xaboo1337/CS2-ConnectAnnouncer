<div align="center">

# 🌐 ConnectAnnouncer (CS2)

**A high-performance, asynchronous connection announcer plugin for Counter-Strike 2 powered by the SwiftlyS2 framework, featuring GeoIP detection, zero game-thread lag, and customizable colored chat broadcasts.**

[![SwiftlyS2](https://img.shields.io/badge/SwiftlyS2-Compatible-blue.svg)](https://swiftlys2.net)
[![GitHub release](https://img.shields.io/github/v/release/xaboo1337/CS2-ConnectAnnouncer?color=brightgreen)](https://github.com/xaboo1337/CS2-ConnectAnnouncer/releases)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Linux%20%7C%20Windows-lightgrey.svg)]()

</div>

---

## 🚀 Key Features

* **Instant & Reliable Detection**: Accurately triggers on client connection (`OnClientPutInServer`) while filtering out bot accounts automatically.
* **Smart GeoIP Integration**: Fetches real-world location (Country & City) via `ip-api.com`.
* **Zero Server Tick-Drop (Fully Async)**: Network requests are completely offloaded to background tasks with thread-safe `Core.Scheduler.NextTick` UI rendering.
* **In-Memory GeoCache**: Caches up to 2,000 unique IP addresses to avoid duplicate API requests and protect free-tier rate limits.
* **Auto-Configuration (`.jsonc`)**: Automatically sets up configuration with structured comment blocks and live-reload support (`reloadOnChange`).
* **LAN / Localhost Filtering**: Identifies loopback, private subnets (RFC 1918), and CGNAT ranges, displaying custom local text instead of network errors.
* **Slot-Recycling Protection**: Safely validates player instances to prevent misattributed notifications if a player quickly reconnects.

---

## 📑 Documentation

| Section | Description |
| :--- | :--- |
| **[Installation](#-installation)** | Step-by-step setup guide for your CS2 dedicated server |
| **[Configuration](#%EF%B8%8F-configuration)** | Explanation of settings and formatting placeholders |
| **[Color Tags](#-chat-color-tags)** | Available color formatting codes for in-game chat |
| **[Compiling Source](#%EF%B8%8F-building-from-source)** | Instructions to build the `.dll` locally |

---

## 📦 Installation

1. Make sure your server has **[SwiftlyS2](https://swiftlys2.net/)** installed and up-to-date.
2. Go to the **[Releases](../../releases)** tab and download `ConnectAnnouncer.zip` (or `ConnectAnnouncer.dll`).
3. Extract or place `ConnectAnnouncer.dll` into your server directory:
   ```text
   csgo/addons/swiftlys2/plugins/ConnectAnnouncer/ConnectAnnouncer.dll
