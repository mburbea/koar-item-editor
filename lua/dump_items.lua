--local simtypes = dofile('.\\mods\\resources\\simtype.lua')
local parent = dofile('.\\mods\\resources\\parent.lua')
local json = dofile('.\\mods\\json.lua')
local fileapi = dofile('.\\mods\\fileapi.lua')

function save_to_file(filename, data)
    fileapi.OpenWrite(filename)
    fileapi.Write(data)
    fileapi.Flush()
    fileapi.Close()
end

function dump_as_json(tbl,name)
    local callback = function(text, canceled)
        save_to_file(name,json.encode(tbl))
    end
    name_win.launch(SL(357894144),
        PLAYER.get_equipped_object_from_equip_type_and_slot('Ring', 0),
        true,
        255,
        callback,
        false) 
end

local function contains(word, toFind)
    return string.find(string.lower(word), toFind) and true or false
end

function TableConcat(t1,t2)
    if(t1 == nil) then return t2 end
    if(t2 == nil) then return t1 end
    for i=1,#t2 do
        t1[#t1+1] = t2[i]
    end
    return t1
end

local function starts_with(str, start)
    return string.lower(str:sub(1, #start)) == string.lower(start)
end

local function ends_with(str, ending)
    return string.lower(str:sub(-#ending)) == string.lower(ending)
 end

function save_to_file(filename, data)
    FileOpenWrite(filename)
    FileWrite(data)
    FileFlush()
    FileClose()
end


function get_hex_of_raw_id(v)
    return ("%06X"):format(TELEMETRY.get_database_id_specified_object_was_created_from(v))
end

function determine_category(actor)
    local e = ACTOR.get_equip_type(actor)
    if(e == "Weapon") then
        local cats = {}
        cats['Dagger']="Daggers"
        cats['Sword'] = "Longsword"
        cats['Two-Handed Sword'] = 'Greatsword'
        cats['Staff'] = 'Staff'
        cats['Wand'] = 'Sceptre'
        cats['Mirror Blades']= 'Faeblades'
        cats['Greathammer'] = 'Hammer'
        cats['Bow']='Longbow'
        cats['Chakram']='Chakrams'
        cats['Fists']='Fists'
        return cats[inventory_win.get_weapon_type(actor)]
    elseif(e == 'Robe') then
        return "Robes"
    end
    return e
end

function set_all(set, val, ...)
    local rest = {...}
    for _, l in ipairs(rest) do set[l] = val end
    return set
end

function determine_armor_type(first_buff)
    local tbl = {}
    set_all(tbl,'Finesse', 532186, 532187, 532188, 532189, 532190, 537333)
    set_all(tbl,'Might', 1908352, 1908353, 1908354, 1908355, 1908356, 537334)
    set_all(tbl,'Sorcery',385267, 387029, 387030, 387031, 387032, 743057)
    return tbl[first_buff] or 'None'
end

function get_buffs(actor,prefix,suffix)
    local selfs = {}
    local normal = {}
    local first_normal = nil
    for _,buff in ipairs(ACTOR.get_owner_buffs_from_object(actor) or {}) do
        local id = tonumber(tostring(buff):sub(5,10),16)
        if(id == prefix) then
            prefix = nil
        elseif(id == suffix)then
            suffix = nil        
        else
            first_normal = first_normal or id
            normal[#normal + 1] = id
        end
    end
    for i,buff in ipairs(ACTOR.get_self_buffs_from_object(actor) or {}) do
        local id = tonumber(tostring(buff):sub(5,10),16)
        if(id == prefix) then
            prefix = nil
        elseif(id == suffix)then
            suffix = nil        
        else
            selfs[#selfs + 1] = id
        end
    end
    return selfs, normal, first_normal
end

function get_socket_info(actor)
    local sockets = {[0]="W",[1]="A",[2]="U",[3]="E"}
    local retVal = ''
    local len = buff_utils.get_total_sockets_on_item(actor)
    for i= 1,len,1 do
        local res = buff_utils.get_socket_type_at_index(actor, i)
        retVal= retVal .. sockets[res]
    end
    return retVal
end

function dump_as_json(tbl,name)
    local callback = function(text, canceled)
        save_to_file(name,json.encode(tbl))
    end
    name_win.launch(SL(357894144),
        PLAYER.get_equipped_object_from_equip_type_and_slot('Head', 0),
        true,
        255,
        callback,
        false) 
end

function determine_elem_by_name(name)
    local lowered = name:lower()
    if(string.find(lowered, 'primal') ~= nil) then
        return 'Primal'
    elseif(string.find(lowered, 'fire') ~= nil) then
        return 'Fire'
    elseif(string.find(lowered, 'ice') ~= nil) then
        return 'Ice'
    elseif(string.find(lowered, 'lightning')~= nil) then
        return 'Lightning'
    else return nil
    end
end
function determine_element(actor, internal_name, parent_name)
    local types = {}
    types['Scourge']='Primal'
    local elem = determine_elem_by_name(internal_name or '') or determine_elem_by_name(parent_name or '')
    if(elem ~= nil) then
        return elem
    end
    if(ACTOR.get_equip_type(actor) == 'Weapon') then
        local tbl = character_data.get_item_dot_damage_by_type(actor)
        if(tbl and tbl[1] and tbl[1].dmg_type) then
            return types[tbl[1].dmg_type] or tbl[1].dmg_type
        end                
    end
    return 'None'
end

local rarity_text = {"Common","Infrequent","Rare","Unique","Set"}
local tbl = {}
local magic = {}
for _,item in ipairs(PLAYER.get_pocket_contents("Default")) do
    local simtype = ACTOR.get_actor_sim_type(item)
    local typeId = tonumber(tostring(simtype):sub(5,10),16)
    local entry = parent[typeId]
    local category = determine_category(item)
    if(category =='Magic' or category=='Fists') then
        magic[#magic+1] = typeId
    elseif(entry ~= nil) then
        local selfs,normal,first_normal = get_buffs(item, entry.prefix, entry.suffix)
        tbl[#tbl + 1] = {
                category = category,
                type_id = typeId,
                level = ACTOR.get_item_level(item),
                name = item,
                original_name = entry.old_name,
                internal_name = entry.internal_name,
                max_durability = ACTOR.get_max_durability(item),
                rarity = rarity_text[buff_utils.get_item_rarity(item)],
                socket_types = get_socket_info(item),
                element = determine_element(item, entry.internal_name, entry.parent_name),
                armor_type = determine_armor_type(first_normal),
                prefix = entry.prefix,
                suffix = entry.suffix,
                item_buffs = selfs,
                player_buffs = normal,
                is_merchant = entry.is_merchant,
                affixable_name = entry.affixable_name,
                has_variants = entry.has_variants
            }
    end
end
save_to_file(".\\mods\\output\\magic.json",json.encode(magic))
--GAME.evaluate_affix_string_against_simtype(buff, simtype)
dump_as_json(tbl,".\\mods\\output\\definitions.json")