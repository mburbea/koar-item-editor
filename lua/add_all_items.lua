local simtypes = dofile('.\\mods\\resources\\simtype.lua')

function contains(word, toFind)
    return string.find(string.lower(word), toFind) and true or false
end

local function starts_with(str, start)
    return string.lower(str:sub(1, #start)) == string.lower(start)
 end
 
function add_all_items(vals)
    local allowed = {Weapon=true,Torso=true,Hands=true,Legs=true,Hat=true,Feet=true,Shield=true,Ring=true,Necklace=true,Robe=true,Cap=true}
    for i,row in ipairs(vals) do
        local k,v = row[1],row[2]
        if (not (contains(v,'spear') or contains(v,'invalid'))) then
            local simt = SIMTYPE_ID(v)
            local r = TYPE.get_equip_type(simt,1)
            if(not PLAYER.has_item(simt) and r~=nil and allowed[r]) then
                local it = PLAYER.cheat_add_item(simt, 1)
                r = ACTOR.get_equip_type(it)
                if(allowed[r] == nil) then
                    PLAYER.destroy_item(simt,1)
                end
            end
        end
    end
end

add_all_items(simtypes)