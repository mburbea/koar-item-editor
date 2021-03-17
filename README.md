## Kingdoms of Amalur: Reckoning (and Re-Reckoning) Save Editor
This is heavily updated editor, rewritten in WPF and modern C# 8 paradigms. A product of Covid-19 quarantine and my frustrations with this game. 

### Versioning Strategy
* Version 3.x will be for the Remaster.
* Version 2.x will be for the Original. 
  - The current final 2.x release is the following: https://github.com/mburbea/koar-item-editor/releases/tag/v2.1.189
* The auto-updater will only look for updates of the same major version. (e.g. 2.x will find new 2.x releases, and 3.x will find new 3.x releases).
* There is no plan for removing support for the original in 3.x branches, but I'm no longer testing my changes against the original.  
  - You **must** use a 3.x release for the remaster; 3.x releases **might** work for the original. 
* Data definitions are based off the **original** game - not the remaster. This means that descriptions and localizations may be wrong. Item definitions can also be incorrectly defined. One such example is the longbow The Hunter which now applies both its buffs as Item buffs and not Player buffs. 

### Currently Not Supported / Issues
* Modifying the auto-save, tutorial save, quick save or the end of game save are NOT supported (and may never be). These 4 saves are slightly different formats and if any of these files are corrupted the game can get stuck in an infinite loop. Even deleting the files will still cause an infinite loop.

### Features
* Now with support for the Nintendo Switch console release.
* The inventory view displays and allows modifying items currently in your inventory. It is broken up into equipment categories, and allows user sorting by different columns.
  - Level, which is a hidden stat that controls level and stat requirements for an item and the damage amount for weapons
  - Gems socketed in the item. (Immutable)
  - The name
  - The current durability
  - The max durability
  - View current item prefix/suffix. (Immutable)
  - Flags for state: (can be applied to all items) Stolen, Stashable, and Sellability.
  - Adjusting an items player and item buff list. Player buffs apply their benefits to the player, Item buffs apply their benefit strictly to the item. (Note: some buffs can only be applied as item buffs), the item buff list is filtered to those that are marked as "On Object" buffs. There is a copy button, so you can copy buff ids into your clipboard. Item buffs will restrict adding or removing buffs if there is unsupported "instance id" in the list.
  - The change definition button allows you to change an item of a category into another of the same category. (e.g. you can turn your Rusty iron Longsword into Flameblade). You can also double click in the empty space on a row to bring up this dialog.
* Modify the size of your inventory. 
* Quest item management
  - This window will list all the quest items in your inventory and let you mark them as sellable. Note that selling currently active quest items can cause quests to break.
* The stash view
  - The stash view becomes available for save files that have discovered the stash, and are not in the same room as the stash.
  - The stash lists all known properties of stashed items, similar to the inventory view, but does not support modifying the item.
  - Equipment items can be removed and added directly to the stash.
  - The stash size CANNOT be modified, however you can add items past the in game limits. (155 in original, 300 in remaster). 

#### HISTORY

Fork of a really obscure Chinese item editor developed by RenYue for the game [Kingdoms of Amalur: Reckoning on PC](https://store.steampowered.com/agecheck/app/102500/) and subsequently translated by [Raziel23x](https://github.com/raziel23x) to English. Appeared on the [CheatHappens forum](https://www.cheathappens.com/show_board2.asp?headID=111841&titleID=17461).

#### Thanks	
- My brother for his massive assistance in this project.

#### CREDITS AND LICENSE

Credits maintained below but I cannot find a license so taking over this for non-commercial purposes and adding the permissive MIT license.
- Copyright © 2021 Michael Burbea & Amir Burbea (This version)
- Copyright © 2019 Jerome Montino (Modified variant)
- Copyright © 2013 Raziel23x (English version)
- Copyright © 2012 RenYue (Chinese version)
