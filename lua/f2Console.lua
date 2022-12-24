function run(path)
    local func, error_str = loadfile(path)
        if func then
            local ran, errorMsg = pcall(func)
            if not ran then
                messagebox(errorMsg,'runtime error')
            end
        else
            if error_str then
                messagebox(error_str,'compile error')
            end
        end
end

cheats.on_enter = function()
	run('.\\mods\\console.lua')
end

function additem(name, qty)
    local simtype = SIMTYPE_ID(name)
    if(simtype~=nil) then
        PLAYER.cheat_add_item(simtype, qty or 1)
    end
end

function iprint(str)
    interfaceLibrary.ftp_notify(str)
end

function spawn(name)
    local simtype = SIMTYPE_ID(name)
    if(simtype~=nil) then
        local player = get_player()
        local pos = ACTOR.get_point_near_object(player, ACTOR.get_angle(player), 500)
        PROTO.create_actor(simtype, pos[1], pos[2], pos[3], player)
    end
end

function messagebox(str, title)
    message_box_win.open_message_box(str,title or 'messagebox')
end

function cheatmenu()
	WINDOW.show_window(cheats.m_window, false)
	SOUND.play_ui(Constants.general_audio.menu_enter)
    INPUT.push_mode(0x1ee5000001b0HL, true)
    pause_game()
	cheats.update_visibility()
end

function lua()
    local function action_eater(event, action)
    end
    local callback = function(text, canceled)
        local str = WINDOW.get_editbox_text(name_win.m_editbox)
        unpause_game()
        UIcontrollerCommon.close_common_control(action_eater)
        local func, error_str = loadstring(str)
        if func then
            local ran, errorMsg = pcall(func)
            if not ran then
                messagebox(errorMsg,'runtime error')
            end
        else
            if error_str then
                messagebox(error_str,'compile error')
            end
        end
    end

    UIcontrollerCommon.open_common_control(action_eater, true)
    pause_game()
    name_win.launch('Enter code:',
        PLAYER.get_equipped_object_from_equip_type_and_slot('Head', 0),
        true,
        255,
        callback,
        false) 
end
