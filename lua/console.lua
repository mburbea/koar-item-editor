--COMMANDS:
--[[
*******************************************************************************
additem(name, quantity)
*******************************************************************************
additem takes simtype internal name and an optional quantity.
    examples:
--add kellerac's sword
additem('sword2h_unique12f')
    --add 10 prismere lockpicks.
additem('lockmelter',10)

********************************************************************************
spawn(name)
********************************************************************************
spawn takes a simtype and spawns it in front of you.
examples:
--spawns a clown car
spawn('obj_clowncar')
--spawns alyn shir
spawn('NPC_MQ_DOK_AlynShir')

*******************************************************************************
iprint(message)
*******************************************************************************
iprint uses the games notification system to print message and localize data.
Takes either a string or an ia64.
-- print Hello world
iprint('Hello world!')
-- print the localization of the suffix Of The Warlord.
iprint(BUFF_ID('Suffix_Char_Crit_Chance_Plus_Damage_03'))

*******************************************************************************
cheatmenu()
*******************************************************************************
launches the games built in debug cheat menu.

*******************************************************************************
messagebox(str, title)
*******************************************************************************
Pops up a messagebox. Can optionally take a title. Takes either a string or an
ia64.
messagebox('advanced','hello world!')
*******************************************************************************
lua()
*******************************************************************************
Brings up a prompt and allows the user to enter lua commands to execute.
*******************************************************************************
run(path)
*******************************************************************************
Runs an external file located at path.
run('.\\mods\\some_other.lua')
]]--
run('.\\mods\\add_all_items.lua')
--run('.\\mods\\dump_gems.lua')
--run('.\\mods\\dump_components.lua')
--lua()

