<div align="center">

# ⚡ ConnectAnnouncer

**High-Performance CS2 Player Connection Announcer with GeoIP for SwiftlyS2**

[![SwiftlyS2](https://img.shields.io/badge/SwiftlyS2-Compatible-blue.svg)](https://swiftlys2.net)
[![Release](https://img.shields.io/github/v/release/xaboo1337/CS2-ConnectAnnouncer?color=green)](https://github.com/xaboo1337/CS2-ConnectAnnouncer/releases)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)]()

</div>

---

### 📋 Overview
A lightweight and fully asynchronous connection announcer for Counter-Strike 2. It announces connecting players in chat with their location (Country, City) without causing game-thread delays or tickrate drops.
<img width="1024" height="191" alt="image" src="https://github.com/user-attachments/assets/cd68161e-195c-4765-a048-673d9510aa73" />


---

### 🚀 Features
* **Zero Tick Lag** — 100% asynchronous background HTTP calls (`Task.Run`).
* **GeoIP Intelligence** — Resolves country and city via `ip-api.com`.
* **In-Memory Cache** — Caches up to 2,000 IPs to prevent duplicate requests.
* **Auto Configuration** — Generates `config.jsonc` with live reload (`reloadOnChange`).
* **Private IP Safe** — Automatically detects local, LAN, and Loopback IP addresses.
* **Bot Filtering** — Automatically ignores fake clients and server bots.

---

### 📥 Installation
1. Ensure your server runs **[SwiftlyS2](https://swiftlys2.net/)**.
2. Download `ConnectAnnouncer.dll` from the **[Releases](../../releases)** tab.
3. Place `ConnectAnnouncer.dll` in:
   ```text
   csgo/addons/swiftlys2/plugins/ConnectAnnouncer/ConnectAnnouncer.dll
