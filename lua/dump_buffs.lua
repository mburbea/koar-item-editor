local json = dofile('.\\mods\\json.lua')
local buffs = dofile('.\\mods\\resources\\buff.lua')
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
        PLAYER.get_equipped_object_from_equip_type_and_slot('Head', 0),
        true,
        255,
        callback,
        false) 
end

local tbl = {}
for _,row in ipairs(buffs) do
    local k,v = row[1], row[2]
    local buff = BUFF_ID(v)
    local elem = {}
    local pd = buff_utils.get_param_data(buff)
    local r = buff_utils.get_buffed_parameter_list_for_display(nil, pd)
    tbl[#tbl + 1] = {
        id = tonumber(k,16),
        name = v,
        desc = r,
        buff_type = BUFF.get_buff_type(buff):gsub("%s+",""),
        modifier = buff,
        apply_type = BUFF.get_buff_apply_type(buff):gsub("%s+", ""),
        rarity = BUFF.get_buff_rarity_name(buff),
        flavor = BUFF.get_buff_description(buff)
        --   ['ui_str'] = BUFF.get_ui_string_for_buffvar(buff),
    }
end
dump_as_json(tbl,".\\mods\\output\\buff.json")