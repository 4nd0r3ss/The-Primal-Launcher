Welcome to The Primal Launcher official page
============================================

The Primal Launcher (TPL) is a launcher for long-abandoned PC videogame FFXIV version 1.23b. I aims to transform the game into a single player experience for those who want to experience this game's story and lore. It is *NOT* compatible with later versions of FFXIV, and it will never be.

**The Primal Launcher is NOT a private server!** (in a way) 
You will not be able to play with other people as TPL does not accept external connections.

This project is focused on easy of use so no databases, web servers, compilation or configurations are required to run TPL. All you need is the game installed in your computer, TPL will take care of everything for you.

TPL was built to be an open source, single executable application with no installation. Just download the binary and run the app. 
It is highly recommended to download TPL only from its official channels.

### How to get the game
It is fairly easy to find copies of this game for a few bucks on sites like ebay, so make sure to buy yours (it looks nice on the shelf!). Any other way of getting a copy of the game is **NOT** recommended, for obvious reasons.

### How to use TPL
First of all, install the game (duh). **It must be installed in a non-system folder, otherwise TPL won't be able to upgrade the game's version**. *This is by design.*

After game installation you will have version 1.0, so we need to upgrade it to version 1.23b. Run TPL, it will detect the game installation and guide you through the update process. There are three steps:
- Download update files (~5GB)
- Update game to 1.23b
- Patch binaries

The process is very simple, all you have to do is to click one button to start each step. The UI is self-explanatory.

### TPL options
I'm in the process of implementing different options to enhance/tweak gameplay.

### TPL generated Files
When first executed, TPL will create a directory 'The Primal Launcher User Files' inside your 'Documents' folder. 
There you will find your characters file, an app config file and the update files.

### Update files
All update files are automatically downloaded by TPL. The files are downloaded from the 3rd party website [http://ffxivpatches.s3.amazonaws.com/]

## What works

Update/login
- Game update files download;
- Game update to v1.23b and binaries patch;
- Dummy login web page

Lobby
- Character create/delete
- Rename character
- Game start

Openings
- All 3 openings fully implemented.
- Battle tutorials implemented for Disciples of War

In-game general
- Change class/job
- Currency add/remove (custom command)
- Items add/remove (custom command)
- Mounts - both chocobo and gobbue (available from start for now)
- NPCs in most zones with default talk dialog.
- Add/remove exp (custom command)
- Level up/down (custom command)
- Dynamic NPC spawn/despawn (load/unload only actors around player at a certain distance).
- All map transitions done.
- Some shops (buy only).
- Quest mechanics implemented (accept, progress, complete. Rewards partially implemented) 

In-game main menu
- Attributes - Character initial attributes, currency, key items.
- Gear - Equip/unequip items (items attr not applied to character stat yet).
- Inventory - Items for display only for now.
- Teleport - Teleport from main menu implemented (total anima for display only, no favored destinations yet).
- Return - implemented.
- Exit game - implemented.

Battle
- Basic auto-attack partially implemented (still buggy)

Weaponskills, abilities, traits, etc.
- Not implemented.

Travel
- PL Custom teleport command (can visit most maps in the game, including jail and cottage).
- Company warp (aethernet) fully functional in all 3 cities.
- Teleport from Aetherytes to Gates and Gates to Aetherytes.

Dungeons
- It's possible to find some dungeons by walking around the map.

Raids/Trials
- Not implemented.



