# SimpleJoinTracker for CS2 🚀

![Version](https://img.shields.io/badge/Version-1.0.0-blue?style=for-the-badge)
![Game](https://img.shields.io/badge/Game-CS2-orange?style=for-the-badge)
![Framework](https://img.shields.io/badge/Framework-CounterStrikeSharp-green?style=for-the-badge)
![Language](https://img.shields.io/badge/Language-C%23-purple?style=for-the-badge)

A lightweight, high-performance connection tracker and ranking system for **Counter-Strike 2**.
Give your loyal players the recognition they deserve with custom rank titles that automatically upgrade as they play!

---

## ✨ Features

* **📈 Connection Tracking:** Counts every time a player joins the server (or map changes).
* **🏆 Custom Ranking System:** Assigns funny/cool titles (e.g., "Spacebar Warrior", "Hyperion God") based on joins.
* **🌈 Rainbow Ranks:** Support for animated multi-color text for top-tier players (e.g., "Autostrafer").
* **🎨 Full Color Support:** Use `{lime}`, `{red}`, `{gold}`, `{purple}` and more in chat.
* **💾 SQLite Database:** Stores data locally in `player_data.db` (No MySQL setup required!).
* **⚙️ 100% Configurable:** Edit all ranks, colors, and messages via simple JSON files.

---

## 📸 Preview

*(Upload a screenshot of your chat message here to show off the ranks!)*

> **[HYPERION KZ]** {red}Strafe God {red}SpawnTerror {default}connected for the {lime}1002nd {default}time!

---

## 📥 Installation

1.  **Prerequisites:** Ensure you have [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) installed on your server.
2.  **Download:** Go to the [Releases Page](../../releases) and download the latest `.zip` file.
3.  **Install:** Extract the contents into your plugins folder:
    `game/csgo/addons/counterstrikesharp/plugins/SimpleJoinTracker`
4.  **Restart:** Restart your server or type `css_plugins load SimpleJoinTracker` in the console.

---

## ⚙️ Configuration

The plugin creates two configuration files in the plugin folder after the first run.

### 1. `config.json`
Change the global prefix shown before every message.
```json
{
  "ServerNamePrefix": "{lime}HYPERION KZ |"
}
