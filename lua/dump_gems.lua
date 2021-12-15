local simtypes = dofile('.\\mods\\resources\\simtype.lua')
local lib = ".\\mods\\dll\\KoreUtils.dll"
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

local tbl = {}
for _,row in ipairs(simtypes) do
    local k,v = row[1], row[2]
    if (not (contains(v,'spear') 
        or contains(v,'invalid')
    )) then
        local simtype = SIMTYPE_ID(v)
        local meta = TYPE.get_component_meta_type(simtype)
        if(meta ~= nil) then
            local item = PLAYER.cheat_add_item(simtype, 1)
            local socketType = get_socket_type(item)
            if (socketType ~= nil) then
                local buffs = TableConcat(ACTOR.get_socket_owner_buffs(item),ACTOR.get_socket_self_buffs(item))
                tbl[#tbl + 1] = {
                    type_id = tonumber(k,16),
                    name = simtype,
                    internal_name = v,
                    buff_id = translate_to_int_array(buffs)[1] or 0,
                    socket_type = socketType
                }
            end
            PLAYER.destroy_item(simtype,1)
        end
    end
end
dump_as_json(tbl,".\\mods\\output\\gem.json")