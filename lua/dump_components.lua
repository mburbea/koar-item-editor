local simtypes = dofile('.\\mods\\resources\\simtype.lua')
local lib = ".\\mods\\dll\\KoreUtils.dll"
local json = dofile('.\\mods\\json.lua')
local fileapi = dofile('.\\mods\\fileapi.lua')
require('crafting_patterns')
local function save_to_file(filename, data)
    fileapi.OpenWrite(filename)
    fileapi.Write(data)
    fileapi.Flush()
    fileapi.Close()
end

local function dump_as_json(tbl,name)
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


local function translate_to_int_array(buffs)
    local output = {}
    for i,buff in ipairs(buffs or {}) do
        local id = tonumber(tostring(buff):sub(5,10),16)
        output[#output+1]=id
    end
    return output
end

local function get_socket_type(item)
    local sockets = {'W','A','U','E'}
    local ix = ACTOR.get_socketable_type(item)
    return sockets[ix+1]
end

local salvage_table = {
    {Name='Longsword', type='component_blade_01',W=true,U=true, Hilt=true,Grip=true,Rivets=true},
    {Name='Hammer', type='component_plumb_01',W=true,U=true,Shaft=true,Grift=true,Bindings=true},
    {Name='Greatsword', type='component_largeblade_01',W=true,U=true, Hilt=true,Grip=true,Bindings=true},
    {Name='Daggers', type='component_smallblade_01',W=true,U=true, Hilt=true,Rivets=true,Bindings=true},
    {Name="Faeblades", type='component_curvedblade_01',W=true,U=true,Fulcrum=true,Rivets=true,Grip=true},
    {Name="Chakrams", type='component_disc_01_fire',U=true,Handle=true,Bindings=true,Grip=true},
    {Name="Longbow",type='component_limb_01',W=true,U=true,Fulcrum=true,Rivets=true,Binding=true},
    {Name="Staff",type='component_rod_01_ice',U=true,Shaft=true,Rivets=true,Bindings=true},
    {Name="Sceptre",type='component_dowel_01_ice',U=true,Handle=true,Bindings=true,Grip=true},
    {Name="Helm", type='component_head_00',A=true,U=true,Rivets=true,Lining=true,Bindings=true},
    {Name="Cuirass",type='component_torso_00',A=true,U=true,E=true,Lining=true,Rivets=true,Strings=true},
    {Name="Gauntlets",type='component_hands_00',A=true,U=true,Grip=true,Lining=true,Rivets=true},
    {Name="Chausses",type='component_legs_00',A=true,U=true,Rivets=true,Strings=true,Trim=true},
    {Name="Greaves",type='component_feet_00',A=true,U=true,Lining=true,Rivets=true,Bindings=true},

    {Name="Cap", type='component_head_06',A=true,U=true,Trim=true,Bindings=true,Grip=true},
    {Name="Armor",type='component_torso_06',A=true,U=true,E=true,Lining=true,Bindings=true,Strings=true},
    {Name="Gloves",type='component_hands_06',A=true,U=true,Grip=true,Strings=true,Bindings=true},
    {Name="Leggings",type='component_legs_06',A=true,U=true,Strings=true,Bindings=true,Grip=true},
    {Name="Boots",type='component_feet_06',A=true,U=true,Trim=true,Rivets=true,Bindings=true},
    {Name="Circlet", type='component_head_11',A=true,U=true,Lining=true,Rivets=true,Bindings=true},
    {Name="Robes",type='component_torso_11',A=true,U=true,E=true,Trim=true,Bindings=true,Strings=true},
    {Name="Handwraps",type='component_hands_11',A=true,U=true,Grip=true,Strings=true,Bindings=true},
    {Name="Shoes",type='component_feet_11',A=true,U=true,Lining=true,Bindings=true,Rivets=true},
    {Name="Shield",type='component_shield_01',U=true,Grip=true,Rivets=true,Trim=true},
    {Name="Buckler",type='component_buckler_01',U=true,Grip=true,Rivets=true,Trim=true},
    {Name="Talisman",type='component_talisman_01_fire',U=true,Grip=true,Bindings=true,Trim=true}
}
local function translate_to_int(ia64)
    return tonumber(tostring(ia64):sub(5,10),16)
end
local defaults = {[534851]=true,[534836]=true,[535029]=true,[534839]=true,[534856]=true,[534858]=true,[534859]=true,[535027]=true,[534855]=true,[534852]=true}
for _,rec in ipairs(salvage_table) do
    rec.simtype = SIMTYPE_ID(rec.type)
    rec.id = translate_to_int(rec.simtype)
end

local function convert_to_array(tbl)
    local res={}
    for k,_ in pairs(tbl) do
        res[#res+1] = k
    end
    return res
end

local function test_salvage(entry)
    if entry.meta_type =="Core" then 
        return
    end
    for _=1,#salvage_table do
        rec=salvage_table[_]
        if(rec[entry.socket_type or entry.category]) then
            local core = PLAYER.cheat_add_item(rec.simtype,1)
            local it = ACTOR.construct_item(core,{PLAYER.cheat_add_item(entry.name,1)}, true)
            local res = ACTOR.salvage_item(it)
            local tbl = {}
            if(res)then
                for i=1,#res do
                    local comp = res[i]
                    local id = translate_to_int(comp)
                    if(id~=rec.id and defaults[id]~=true) then
                        tbl[id]=true
                    end
                    PLAYER.destroy_item(comp,1)
                end
            else
                iprint(entry.name)
            end
            entry[rec.Name]=convert_to_array(tbl)
        end
    end
end

local tbl = {}
for i=#simtypes/2,#simtypes do
    row = simtypes[i]
    local k,v = row[1], row[2]
    if (not (contains(v,'spear') or contains(v,'invalid'))) then
        local simtype = SIMTYPE_ID(v)
        local meta = TYPE.get_component_meta_type(simtype)
        if(meta ~= nil) then
            local item = PLAYER.cheat_add_item(simtype, 1)
            local category = ACTOR.get_component_type(item)
            local socket = get_socket_type(item)
            local buffs = ACTOR.get_all_component_buffs(item)
            local entry = {
                raw_category = category,
                category = crafting_patterns.get_component_type_from_index(category),
                final_product =  tonumber(tostring(ACTOR.get_component_final_product(item)):sub(5,10),16),
                level = ACTOR.get_item_level(item),
                type_id = k,
                name = simtype,
                internal_name = v,
                buffs = translate_to_int_array(buffs),
                socket_type = socket,
                meta_type = meta
            }
            test_salvage(entry)
            tbl[#tbl+1] = entry
            PLAYER.destroy_item(simtype,1)
        end
    end
end

dump_as_json(tbl,".\\mods\\output\\components2.json")
