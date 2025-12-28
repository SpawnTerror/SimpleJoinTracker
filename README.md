# SimpleJoinTracker for CS2 🚀

![Version](https://img.shields.io/badge/Version-1.0.0-blue?style=for-the-badge)
![Game](https://img.shields.io/badge/Game-CS2-orange?style=for-the-badge)
![Framework](https://img.shields.io/badge/Framework-CounterStrikeSharp-green?style=for-the-badge)
![Language](https://img.shields.io/badge/Language-C%23-purple?style=for-the-badge)

A lightweight, high-performance connection tracker and ranking system for **Counter-Strike 2**.
Give your loyal players the recognition they deserve with custom rank titles that automatically upgrade as they play!

---

## ✨ Features

* **📈 Connection Tracking:** Counts every time a player joins the server.
* **🏆 Custom Ranking System:** Assigns funny/cool titles (e.g., "Spacebar Warrior", "Hyperion God") based on joins.
* **🌈 Rainbow Ranks:** Support for multi-color text for top-tier players (e.g., "Autostrafer").
* **🎨 Full Color Support:** Use `{lime}`, `{red}`, `{gold}`, `{purple}` and more in chat.
* **💾 SQLite Database:** Stores data locally in `player_data.db` (No MySQL setup required!).
* **⚙️ 100% Configurable:** Edit all ranks, colors, and messages via simple JSON files.

---

## 📸 Preview

![204C93~1](https://github.com/user-attachments/assets/989c4d19-3c7e-4345-a209-ea28fb8ee0e4)

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
```

---

### 2. `ranks.json`
You can create unlimited ranks! The plugin automatically sorts them.

MinCount: How many connections needed.

Title: The rank name.

Color: The color of the rank name.

IsRainbow: Set to true for the rainbow text effect (sorta, it is simply every letter has different colour)

```JSON
[
  {
    "MinCount": 0,
    "Title": "KZ Nub",
    "Color": "{grey}",
    "IsRainbow": false
  },
  {
    "MinCount": 1000,
    "Title": "Strafe God",
    "Color": "{red}",
    "IsRainbow": false
  },
  {
    "MinCount": 3000,
    "Title": "Autostrafer",
    "Color": "{white}",
    "IsRainbow": true
  }
]
```

## 🎨 Supported Colors
You can use these tags in your config files:

{default}, {white}, {darkred}, {green}, {lime}, {red}, {blue}, {gold}, {yellow}, {silver}, {grey}, {purple}, {olive}, {magenta}, {lightred}, {lightblue}.

---

## 🤝 Credits
Author: SpawnTerror

Framework: CounterStrikeSharp

License: MIT
