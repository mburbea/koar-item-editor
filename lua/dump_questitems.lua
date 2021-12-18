local simtypes = dofile('.\\mods\\resources\\simtype.lua')

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

tbl = {}
for _,row in ipairs(simtypes) do
    local k,v = row[1], row[2]
    if (not (contains(v,'spear') 
        or contains(v,'invalid') 
        or starts_with(v,'ao_')
        or contains(v,"recipe_")
        or contains(v,"reagent_")
        or contains(v,"component_")
        or contains(v,"support_")
        or contains(v,"bag_")
        or contains(v,"key")
        or starts_with(v, "parent_")
        or starts_with(v, "dev_")
        or starts_with(v, "shard_")
        or v == "unarmed_0"
        or v == "unarmed_1"
    )) then
        local simtype = SIMTYPE_ID(v)
        if(TYPE.is_quest_item(simtype)) then
            tbl[#tbl + 1] = {
                id =  tonumber(k,16),
                name = simtype,
                internal_name = v,

            }
        end
    end
end
dump_as_json(tbl, ".\\mods\\output\\questItemDefinitions.json")
