# The Primal Launcher

The Primal Launcher (TPL) is a launcher for long-abandoned PC videogame FFXIV version 1.23b. I aims to transform the game into a single player experience for those who want to check this game's story and lore. It is *NOT* compatible with later versions of FFXIV, and will never be.

This project is focused on easy of use, so no databases, web servers, compilation or configurations are required to run TPL. All you need is the game installed in your computer, and TPL will take care of everything for you.

TPL was built to be an open source, single executable application with no installation. Just download the .zip, extract it and run the app.

# Disclaimer

TPL is *NOT* private server software, it's a single-player launcher. You wont be able to play with other players. If you are looking for private server software, check out the highly recommended Meteor Project: http://ffxivclassic.fragmenterworks.com/wiki/index.php/Main_Page

# What to expect 

I've been in and out of this project throughout several years, and I work on it in my spare time. That being said, I have no idea when this project will be completed or if it ever will be. 

So far I did my best to get most of the system stuff working. Still a LONG way to go, but you should find most things at least partially working.

As you may be aware, this game's wonkiness is stuff of legend. Performance is 100% random, sometimes it will run super fast, sometimes slow AF. The problem is NOT your computer.

The game WILL crash. A lot. It may crash/hang/freeze on its own or because something has not been implemented yet. In any case, just close it and open it again, your progress should be saved unless something very weird happened.

*IMPORTANT*
If you decide to progress further in the game, be aware that I CANNOT guarantee your save file will work on later versions of TPL.

# Current gameplay state

As I mentioned above, most of the system stuff works at least partially. 

- Main menu: all sections will open the corresponding widget but it may or may not show data.
- Game opening, battle tutorial and start quests are fully implemented for all 3 cities.
- Inn quests fully implemented for all 3 cities.
- Battle mechanics work (still buggy), but no calculations are implemented yet (damage inflicted and taken are fixed amounts for now). Auto-attack, weaponskills, abilities, spells also working with fixed damage amounts.
- Character: exp gain, level up, hotbar, equipment change, class change/unlock implemented.
- Most shops implemented, you can buy items but not sell them yet.
- Mounts implemented and unlocked from the start for now.
- Accepting and progressing quests inplemented, but only the first couple quests heve been scripted.
- Regional and Local levequests can be accepted/dropped, but not started or progressed.
- Crafting is not implemented, but you can open the craft interface.
- Gathering not implemented.
- You can use the teleport menu to travel to any aetheryte crystal, they are all unlocked from the start.
- You can travel to aetheryte gates from their parent crystals' menu.

# User Instructions

- Install the game. I recommend installing it in a non-system folder, like your 'Documents' folder for example. If you choose to install it on the default location (C:/Program Files (x86)), you will need to give TPL admin permissions so it can update the game to version 1.23b.
- Download the .zip file from this page and extract it to your preferred folder.
- Run TPL. If no game installation is found, it will let you know and close.
- If a game installation is found, TPL will show you the 'Game Update' screen. 
- Follow the on-screen instructions.
- Done!