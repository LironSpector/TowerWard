# TowerWard (Client) — C# & Unity

**To navigate to the C# code files (and folders):** go to **`Assets/Scripts/`** to view all C# files (gameplay, networking, UI, etc.).

## Overview
This is the client-side for **TowerWard**, a C# + Unity tower‑defense game with **Single Player** and **2‑player Multiplayer** (the server code is in the `TowerWardServer` repository). The client handles gameplay, UI, effects, secure communication with the server and more.

## About the game
**TowerWard** is a **tower defense** game: defend yourself from enemy waves by placing and upgrading towers along the path.  
- **Single Player:** play on a map, deploy and upgrade various types of towers, and fight escalating waves of **enemies**. Win by destroying all waves.  
- **Multiplayer (2 players):** both players connect to the server and the server matches them for a game. Each plays on their **own map**, faces the natural waves, **and can send enemies to the opponent**. Players must manage money/resources and choose between attacking, reinforcing defenses, or using **special abilities**. Periodic **Snapshots** show the opponent’s state to help with decisions and strategy development. The goal is to be the last player standing.  
- **Extra game features (both modes):** special abilities, a **banking system**, and **mystery boxes** that provide additional depth and strategy to the game.  
- **Additional features:** settings management, a sound system and a tutorial for beginners.  


## What the client provides
- **Core gameplay loop:** health and money management, tower placement on the map + upgrades, enemies, and wave progression with increasing difficulty.
- **Multiplayer game flow:** login/registration, matchmaking, match start/finish flow, synchronized events with the server, and opponent interaction (including sending enemies and some special abilities).
- **Secure networking client:** messages are JSON‑based and encrypted via AES, handshake & session management, reconnect/cleanup routines, and message queueing on the main thread.
- **Interaction with the opponent:** periodic **snapshots** and in‑game display of the opponent’s current state for forming a better strategy.
- **Economy systems:** currency earning/spending + validation, plus a **banking** system.
- **Special abilities:** player‑activated effects (e.g., boosts, buffs, money modifiers) with cooldowns, durations, and multiplayer‑specific actions.
- **Status effects & combat depth:** projectile effects (freeze/slow/poison), damage to enemies, target selection and predictive aiming for moving enemies.
- **User interface & UX:** panels for towers, player stats, opponent snapshot, tips, notifications, settings menu, and in‑game prompts.
- **Audio & visuals:** game/audio states for music and effects, and effects + dynamic layout for visual feedback.
- **Scene flow & state management:** main menu/login, waiting/matchmaking, gameplay scene transitions, win/lose handling, and more.
- **Validation & helpers:** various input validations, gameplay validation (e.g., grid and path occupancy checks), placement rules.
- **Extensive feedback:** extensive UI feedback for the different actions and states during the game.

## Main Tech
- **.NET / C#**, **Unity**
- **TCP sockets** with AES/RSA encryption
- **JWT** authentication (access + refresh tokens) - also used for auto-login


## To make it clear
- View the `TowerWardServer` repository to understand more about the project and the server-side functionality.

## Notes
- Designed for learning/demo and easy extension.
