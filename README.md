# ⚡ CS2 ConnectAnnouncer

> **High-Performance Player Connection Announcer with GeoIP Intelligence for SwiftlyS2**

---

### 📋 Overview
**ConnectAnnouncer** is an ultra-lightweight, high-performance, and fully asynchronous connection announcer plugin designed for Counter-Strike 2 dedicated servers running the SwiftlyS2 framework. It detects connecting players, resolves their geographical location (Country and City) in real time, and broadcasts custom colored chat notifications without degrading server tickrate or causing game-thread delays.

---

## 💎 Features at a Glance

* 🚀 **Zero Tick Lag (Fully Asynchronous)** — Network operations run completely in background worker threads (`Task.Run`).
* 🌍 **GeoIP Lookup** — Automatic country and city resolution via `ip-api.com`.
* 🛡️ **Built-in Memory Cache** — Caches up to 2,000 unique IP addresses to eliminate redundant queries and stay within free-tier API rate limits.
* ⚙️ **Hot-Reload Architecture** — Configuration changes take effect immediately on file save without needing a plugin reload (`reloadOnChange: true`).
* 🔒 **Local IP Filter** — Automatic identification and filtering for loopback, local subnets (RFC 1918), and CGNAT ranges.
* 🤖 **Bot Filtering** — Automatically ignores server bots and fake clients (`IsFakeClient`).
* 🔁 **Slot-Recycling Protection** — Verifies player identity before sending messages to prevent misattribution if a player disconnects during the API lookup.

---

## 📥 Installation
