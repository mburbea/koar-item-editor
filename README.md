## Kingdoms of Amalur: Reckoning Save Editor
This is heavily updated editor, rewritten in WPF and modern C#8 paradigms. A product of covid-19 quarantine and my frustrations with this game. 

#### New Features
* Reliable updating of inventory size
  - The way the save file lays out the inventory size, is stored in a data structure that's fields location change every save
  - the old editor assumed a fix offset, so it would only work some times.
* Toggle the sellable flag of items
   - Quest items can get stuck in your inventory, this let's you simply turn that flag off for stuck quest items.
   - Can be done on ALL inventory items.
* Better display of items, categorizing them by their equipment type as well as visualizing their core effects.
   - All weapon category are detected and show the appropriate icon.
   - Armor types are currently not detected so it's all bucketed as armor.
   - Kite Shields are easy to detect, but buckler & talisman currently don't always find rhyme or reason. So many talisman's may be categorized as bucklers.
   - There are **many** of Core effects, and only so many you can get from crafting.
   - What's worse is there are half tier effects that when you salvage will go up a tier (e.g. Poison 3.5 will salvage to Master Infected Hilt.) Most are not easy to find.
   Nearly every unique has a different effect code. Possibly more than one.
* Adding and removing of Core Effect #1
   - Core effects are effects added by usually the second component in a crafted items, or being mastercrafted.
   - They confer a visual effect (like the hilt being on fire or the lightning on `Lightning Cage`), as well as a damage effect.
   - Adding a code from a unique weapon usually will turn your weapon Unique. 
   - Inexplicably, the item card display effect can actually be different than the applied effect. So your sword can say it deals frost damage but will be on fire and light enemies aflame. The editor currently couples the two together (like the game does).
   - You can use right click to copy a listed core effect.
* Change the Item's TypeId #6
   - The typeId defines the equipment model it uses and also changes the item's description in game.
   - It also defines stats (I believe) There is a struct that follows after the typeId, but modifying it doesn't seem to do anything.
   - If your holding the item, it probably will not update the base stats of the item.
* Add a quick way to diversify items
   - This operation will change the durability of all unknown equipment.
   - You can use this to find an item in the editor. (e.g. 54/82 might become `Lightning Cage`,53/82 might become `Gentleman's Favor`).

### What's old
* List items.
* Change Item current and max durability.
* Change an item's name (if it has a name).
* View, add and remove an item's support effects. (For crafted gear this would come come from something like rivets).

### Todo:
- [ ] Figure out what's currently equipped.
- [ ] Find a way to determine an armor type.
- [ ] Come up with a listing of TypeIds. Quite a big ordeal as they are more than 500 different items types in the game.
- [ ] Possibly add support for rings & amulets 

#### Thanks
- My brother for his massive assistance in creating the wpf gui. 
- [This steam guide which listed the struct of the support component segment] (https://steamcommunity.com/sharedfiles/filedetails/?id=1072394368)
#### HISTORY

Fork of a really obscure Chinese item editor developed by RenYue for the game [Kingdoms of Amalur: Reckoning on PC](https://store.steampowered.com/agecheck/app/102500/) and subsequently translated by [Raziel23x](https://github.com/raziel23x) to English. Appeared on the [CheatHappens forum](https://www.cheathappens.com/show_board2.asp?headID=111841&titleID=17461).

#### CREDITS AND LICENSE

Credits maintained below but I cannot find a license so taking over this for non-commercial purposes and adding the permissive MIT license.
- Copyright © 2020 Michael Burbea & Amir Burbea (This version)
- Copyright © 2019 Jerome Montino (Modified variant)
- Copyright © 2013 Raziel23x (English version)
- Copyright © 2012 RenYue (Chinese version)
