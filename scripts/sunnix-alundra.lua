-- prevent warnings for JIT objects
PCSX = PCSX
bit = bit
ffi = ffi
imgui = imgui

local mem = PCSX.getMemPtr()
local registers = PCSX.getRegisters()
local show = true

-- ################################################################
--                         ADRESSES
-- ################################################################
local FRONT_OBJECT              = 0x8009b774
local WALKING_IN_OBJECT         = 0x801ac828
local CAMERA_FOLLOW_X           = 0x801d7b40
local CAMERA_FOLLOW_Y           = 0x801d7b90
local CAMERA_FOLLOW_Z           = 0x801d7b94
local PLAYER_POS_X              = 0x801ac80e
local PLAYER_POS_Y              = 0x801ac812
local PLAYER_POS_Z              = 0x801ac816
local PLAYER_LOOK_DIR           = 0x801ac784
local SE_BGM_MENU_ENABLED       = 0x800aa118
local DEBUG_MODE_FLAG           = 0x8010CDC0 --0x8010CDC0 -- 0x801dd430
local SHOW_DEBUG_TEXT_FLAG      = 0x800DC05B --0x800DC05B -- 0x801ac6cb
local DEBUG_OPTIONS             = 0x800DC05C --0x801ac6cc -- FR: 800DC05C -- 801ac6cc - 800DC05C = D0670
local DEBUG_WARP_MAP            = 0x800dc064 --0x801ac6d4 -- FR: 800dc064
local DEBUG_SLOW_SPEED          = 0x800dc06c --0x801ac6dc -- FR: 800dc06c
local DEBUG_MAP_LIMITS          = 0x800dc070 --0x801ac6e0 -- FR: 800dc070
local CONTROLLER_2_KEY_PRESSED  = 0x801dda62
local BLOCKING_FLAG             = 0x801f0060
local PLAYER_ACTION_FLAG        = 0x801ac780
local GILDER                    = 0x801dd824
local FALCONS                   = 0x801dd82a
local ITEMS_START               = 0x801dd834
local SPU_KEYS                  = 0x801efe68
local SPU_SE_IDs                = 0x801efe80
local SPU_SE_TYPE               = 0x801efee0
local SAVE_FILE_DESCRIPTIONS    = 0x800295b0
local GLOBAL_STORY_FLAG         = 0x801dd334 -- size tested up to 224 (maybe more)
local LOCAL_STORY_FLAG          = 0x801e6378 -- up to 256 bytes
local GAME_PLAY_TIMER           = 0x801d7cb4
local PLAYER_HP                 = 0x801ac70c
local PLAYER_MAX_HP             = 0x801ac710
local RETRIES                   = 0x80010756
local SAVE_ACTION_FLAG          = 0x800c67e8
local GUI_STATE                 = 0x8012e3e0
local SAVE_STYLE                = 0x801f3a0c

local OBJECT_INSTANCES          = 0x801ac6f8 -- Contains up to 64 instances a 660 byte blocks
local ITEM_DROP_TABLE           = 0x80028db0 -- size: 0x800 bytes
local ITEM_PROPERTIES           = 0x800bbe50 -- 5xshort array for every ItemID

-- Datas bin
local DATAS_BIN_HEADER          = 0x801ef440
local MAP_DATA                  = 0x801536c0 -- Header size of 7x4 bytes (28 bytes)

-- ################################################################
--                       Helper Functions
-- ################################################################
local id = 0

local function getPtrOf(address, type)
    address = bit.band(address, 0x1fffff)
    local pointer = mem + address
    return ffi.cast(type, pointer)
end

local function doSliderInt(address, name, min, max, type)
    local pointer = getPtrOf(address, type)
    local value = pointer[0]
    local changed, value = imgui.SliderInt(name, value, min, max, '%d')
    if changed then
        pointer[0] = value
    end
end

local function createSliderIntButtons(address, name, min, max, type)
    local ptr = getPtrOf(address, type)
    local nextValue
    if imgui.Button(string.format("-##%s", name), 22, 22) then
        nextValue = ptr[0] - 1
        if nextValue < min then
            nextValue = min
        end
        ptr[0] = nextValue
    end
    imgui.SameLine()
    imgui.PushItemWidth(355)
    doSliderInt(address, string.format("##%s", name), min, max, type)
    imgui.PushItemWidth(0)
    imgui.SameLine()
    if imgui.Button(string.format("+##%s", name), 22, 22) then
        nextValue = ptr[0] + 1
        if nextValue > max then
            nextValue = max
        end
        ptr[0] = nextValue
    end
    imgui.SameLine()
    imgui.TextUnformatted(name)
end

local function L_doSliderInt(pointer, name, min, max)
    local value = pointer[0]
    local changed, value = imgui.SliderInt(name, value, min, max, '%d')
    if changed then
        pointer[0] = value
    end
end

local function L_createSliderIntButtons(pointer, name, min, max)
    local nextValue
    if imgui.Button(string.format("-##%s", name), 22, 22) then
        nextValue = pointer[0] - 1
        if nextValue < min then
            nextValue = min
        end
        pointer[0] = nextValue
    end
    imgui.SameLine()
    imgui.PushItemWidth(355)
    L_doSliderInt(pointer, string.format("##%s", name), min, max)
    imgui.PushItemWidth(0)
    imgui.SameLine()
    if imgui.Button(string.format("+##%s", name), 22, 22) then
        nextValue = pointer[0] + 1
        if nextValue > max then
            nextValue = max
        end
        pointer[0] = nextValue
    end
    imgui.SameLine()
    imgui.TextUnformatted(name)
end

local function doCheckInt(address, name, type, valOn, valOff)
    local pointer = getPtrOf(address, type)
    local value = pointer[0]
    local on = value == valOn
    local changed, value = imgui.Checkbox(name, on)
    if changed then
        if value == true then
            pointer[0] = valOn
        else
            pointer[0] = valOff
        end
    end
    return on
end

local function doCheckBit(address, name, type, bitPos)
    local bitMask = bit.lshift(1, bitPos)
    local pointer = getPtrOf(address, type)
    local value = bit.band(pointer[0], bitMask) == bitMask
    local changed, value = imgui.Checkbox(name, value)
    if changed then
        if value == true then
            pointer[0] = bit.bor(pointer[0], bitMask)
        else
            pointer[0] = bit.band(pointer[0], bit.bnot(bitMask))
        end
    end
end

local function addItemCheck(pos, name, desc)
    doCheckInt(ITEMS_START + (pos * 4), name, "int32_t*", 1, 0)
    if (desc == nil) == false then
        if imgui.IsItemHovered() then
            imgui.BeginTooltip()
            imgui.TextUnformatted(desc)
            imgui.EndTooltip()
        end
    end
end

local function nextID()
    id = id + 1
    return id
end

local function toBinary(num, bits)
    bits = bits or 8
    local bin = ""
    for i = bits - 1, 0, -1 do
        bin = bin .. (bit.band(bit.rshift(num, i), 1) == 1 and "1" or "0")
    end
    return bin
end

-- ################################################################
--                         View Functions
-- ################################################################

-- ############################ Info ##############################
local selectedWarp = 0

local function infoPlayer()
    if imgui.TreeNode("Player") then
        if imgui.BeginTable("table_player_pos", 3, imgui.constant.TableFlags.Borders) then
            imgui.TableNextRow(imgui.constant.TableFlags.Headers)
            imgui.TableSetColumnIndex(0)
            imgui.TableHeader("X")
            imgui.TableSetColumnIndex(1)
            imgui.TableHeader("Y")
            imgui.TableSetColumnIndex(2)
            imgui.TableHeader("Z")

            imgui.TableNextRow()
            imgui.TableSetColumnIndex(0)
            imgui.TextUnformatted(getPtrOf(PLAYER_POS_X, "int16_t*")[0])
            imgui.TableSetColumnIndex(1)
            imgui.TextUnformatted(getPtrOf(PLAYER_POS_Y, "int16_t*")[0])
            imgui.TableSetColumnIndex(2)
            imgui.TextUnformatted(getPtrOf(PLAYER_POS_Z, "int16_t*")[0])

            imgui.TableNextRow()
            imgui.TableSetColumnIndex(0)
            imgui.TextUnformatted(math.floor(getPtrOf(PLAYER_POS_X, "int16_t*")[0] / 24))
            imgui.TableSetColumnIndex(1)
            imgui.TextUnformatted(math.floor(getPtrOf(PLAYER_POS_Y, "int16_t*")[0] / 16))
            imgui.TableSetColumnIndex(2)
            imgui.TextUnformatted(math.floor(getPtrOf(PLAYER_POS_Z, "int16_t*")[0] / 16))
            imgui.EndTable()
        end
        imgui.BeginGroup()
        imgui.TextUnformatted("Direction:")
        imgui.SameLine()
        local dir = bit.band(3, bit.rshift(getPtrOf(PLAYER_LOOK_DIR, "uint8_t*")[0], 3))
        if dir == 0 then
            imgui.TextUnformatted("South")
        elseif dir == 1 then
            imgui.TextUnformatted("West")
        elseif dir == 2 then
            imgui.TextUnformatted("North")
        else
            imgui.TextUnformatted("East")
        end
        imgui.PushItemWidth(150)
        doSliderInt(PLAYER_MAX_HP, "Player Max HP", 0, 50, "uint32_t*")
        doSliderInt(PLAYER_HP, "Player HP", 0, getPtrOf(PLAYER_MAX_HP, "uint32_t*")[0], "uint32_t*")
        
        imgui.PushItemWidth(0)
        imgui.EndGroup()
        imgui.SameLine()
        imgui.BeginGroup()
        if imgui.Button("Up", 158, 30) then
            local ptr = getPtrOf(PLAYER_POS_Y, "int16_t*")
            ptr[0] = ptr[0] - 16
        end
        if imgui.Button("Left", 75, 30) then
            local ptr = getPtrOf(PLAYER_POS_X, "int16_t*")
            ptr[0] = ptr[0] - 24
        end
        imgui.SameLine()
        if imgui.Button("Right", 75, 30) then
            local ptr = getPtrOf(PLAYER_POS_X, "int16_t*")
            ptr[0] = ptr[0] + 24
        end
        if imgui.Button("Down", 158, 30) then
            local ptr = getPtrOf(PLAYER_POS_Y, "int16_t*")
            ptr[0] = ptr[0] + 16
        end
        imgui.EndGroup()
        imgui.TreePop()
    end
end

local function infoWarpPosView()
    if(imgui.TreeNode("Warp")) then

        local portalAddr = getPtrOf(0x801f0070, "uint32_t*")[0] + 1068

        imgui.BeginGroup()
        imgui.TextUnformatted("Warps:")
        if (imgui.BeginListBox("##List", 140, imgui.GetTextLineHeightWithSpacing() * 20, true)) then
            for i = 0, 62 do
                local dest = getPtrOf(portalAddr + 4 + i * 12, "uint16_t*")[0]
                if imgui.Selectable(string.format("Warp %d (%d)", i + 1, dest), selectedWarp == i) then
                    selectedWarp = i
                end
            end
            imgui.EndListBox()
        end
        imgui.EndGroup()
        imgui.SameLine()
        imgui.BeginGroup()

        imgui.TextUnformatted(string.format("Warp Info:"))

        imgui.TextUnformatted("Trigger Zone")
        imgui.Button("##Rect", 80, 80)
        local x, y = imgui.GetCursorPos()
        local tZone = getPtrOf(portalAddr + selectedWarp * 12, "uint8_t*")
        -- Left
        imgui.SetCursorPos(x + 5, y - 50)
        imgui.TextUnformatted(tZone[0])
        -- Top
        imgui.SetCursorPos(x + 30, y - 80)
        imgui.TextUnformatted(tZone[1])
        -- Right
        imgui.SetCursorPos(x + 60, y - 50)
        imgui.TextUnformatted(tZone[2])
        -- Bottom
        imgui.SetCursorPos(x + 30, y - 20)
        imgui.TextUnformatted(tZone[3])
        imgui.SetCursorPos(x, y)

        -- Move player to warp
        if imgui.Button("Move here") then
            x = (tZone[0] + tZone[2]) * 24 / 2.0 + 12
            y = (tZone[1] + tZone[3]) * 16 / 2.0 + 8
            getPtrOf(0x801ac80e, "int16_t*")[0] = x
            getPtrOf(0x801ac812, "int16_t*")[0] = y
        end

        imgui.TextUnformatted(string.format("Map: %d", getPtrOf(portalAddr + 4 + selectedWarp * 12, "uint16_t*")[0]))
        local pos = getPtrOf(portalAddr + 6 + selectedWarp * 12, "uint8_t*")
        imgui.TextUnformatted(string.format("X: %d", pos[0]))
        imgui.SameLine()
        imgui.TextUnformatted(string.format("Y: %d", pos[1]))
        imgui.SameLine()
        imgui.TextUnformatted(string.format("Z: %d", pos[2]))

        imgui.TextUnformatted("")
        local trans = pos[5]
        imgui.TextUnformatted("Transition:")
        local type = bit.rshift(trans, 4)
        if type >= 0 or type < 12 then
            if type == 0 then
                type = "Fade"
            elseif type == 1 then
                type = "Rapid"
            elseif type == 2 then
                type = "White"
            elseif type == 3 then
                type = "Scroll (unused / no function)"
            elseif type == 4 then
                type = "Dream"
            elseif type == 5 then
                type = "Warp"
            elseif type == 6 then
                type = "Gate"
            elseif type == 7 then
                type = "None"
            elseif type == 8 then
                type = "Dead"
            elseif type == 9 then
                type = "Ending (ends the game)"
            elseif type == 10 then
                type = "Restart (warp to ship)"
            elseif type == 11 then
                type = "Title (go to title screen)"
            end
            type = string.format("Type: %s", type)
        else
            type = string.format("Type: %d", type)
        end
        imgui.TextUnformatted(type)
        imgui.TextUnformatted(string.format("Audio: %d", bit.band(trans, 0xF)))
        local dir = pos[6]
        imgui.TextUnformatted("Direction:")
        imgui.TextUnformatted(string.format("Source: %d", bit.rshift(dir, 4)))
        local dir = bit.band(dir, 0x3)
        if dir == 0 then
            dir = "South"
        elseif dir == 1 then
            dir = "West"
        elseif dir == 2 then
            dir = "North"
        else
            dir = "East"
        end
        imgui.TextUnformatted(string.format("Destinaton: %s", dir))

        imgui.EndGroup()

        imgui.TreePop()
    end
end

local function infoPSU()
    if imgui.TreeNode("PSU") then
        if imgui.BeginTable("table_sound_channels", 4, imgui.constant.TableFlags.Borders) then
            imgui.TableSetupColumn("CH")
            imgui.TableSetupColumn("Active")
            imgui.TableSetupColumn("SE ID")
            imgui.TableSetupColumn("???")

            local keys = getPtrOf(SPU_KEYS, "int8_t*")
            local seIDs = getPtrOf(SPU_SE_IDs, "int32_t*")
            local seType = getPtrOf(SPU_SE_TYPE, "int32_t*")

            imgui.TableNextRow(imgui.constant.TableFlags.Headers)
            for i = 0, 3 do
                imgui.TableSetColumnIndex(i)
                imgui.PushID(i)
                local columnName = imgui.TableGetColumnName(i)
                imgui.TableHeader(columnName)
                imgui.PopID()
            end

            for i = 0, 20 do -- 21 channel
                imgui.TableNextRow()
                imgui.TableSetColumnIndex(0)
                imgui.TextUnformatted(i)
                imgui.TableSetColumnIndex(1)
                local keyActive
                if keys[i] == 2 then
                    keyActive = "X"
                else
                    keyActive = ""
                end
                imgui.TextUnformatted(keyActive)
                imgui.TableSetColumnIndex(2)
                imgui.TextUnformatted(seIDs[i])
                imgui.TableSetColumnIndex(3)
                imgui.TextUnformatted(bit.tohex(bit.bswap(seType[i])))
            end
            imgui.EndTable()
        end
        imgui.TreePop()
    end
end

----------------------------------
-- Datas.bin Data reading start --
----------------------------------
local dbHeader = getPtrOf(DATAS_BIN_HEADER, "uint32_t*")
local mapDataHeader = getPtrOf(MAP_DATA, "uint32_t*")

local function mapMemoryButton(name, offset)
    if imgui.Button(string.format("Go to memory##mapMem%d", offset)) then
        local size = 1
        if offset < 6 then
            size = mapDataHeader[offset + 1] - mapDataHeader[offset]
        end
        PCSX.GUI.jumpToMemory(MAP_DATA + mapDataHeader[offset], size)
    end
    imgui.SameLine()
    imgui.TextUnformatted(string.format("%s: %X", name, mapDataHeader[offset]))
end

local function infoDatasBin()
    if imgui.TreeNode("Datas.bin") then

        imgui.TextUnformatted(string.format("GlobalObjGroup: %X", dbHeader[0]))
        imgui.TextUnformatted(string.format("GlobalSprites: %X", dbHeader[1]))
        imgui.TextUnformatted(string.format("GlobalSpritesCopy: %X", dbHeader[2]))
        imgui.TextUnformatted(string.format("GlobalStrings: %X", dbHeader[3]))
        imgui.TextUnformatted(string.format("GlobalStringsCopy: %X", dbHeader[4]))
        imgui.TextUnformatted(string.format("Unknown: %X", dbHeader[5]))
        imgui.TextUnformatted(string.format("LoadingScreen0: %X", dbHeader[6]))
        imgui.TextUnformatted(string.format("LoadingScreen1: %X", dbHeader[7]))
        imgui.TextUnformatted(string.format("LoadingScreen2: %X", dbHeader[8]))
        imgui.TextUnformatted(string.format("LoadingScreen3: %X", dbHeader[9]))
        if imgui.TreeNode("Levels") then
            for i = 0, 483 do
                imgui.TextUnformatted(string.format("Level %d: %X", i, dbHeader[10 + i]))
            end
            imgui.TextUnformatted(string.format("Level end: %X", dbHeader[10 + 484]))
            imgui.TreePop()
        end

        imgui.TreePop()
    end
    if imgui.TreeNode("Current Level Data") then
        mapMemoryButton("Properties", 0)
        mapMemoryButton("Map", 1)
        mapMemoryButton("Tileset", 2)
        mapMemoryButton("ObjGroup", 3)
        mapMemoryButton("Spritesheet", 4)
        mapMemoryButton("Layout", 5)
        mapMemoryButton("Strings", 6)

        imgui.TreePop()
    end
end
--------------------------------
-- Datas.bin Data reading end --
--------------------------------
--------------------------------
--      Object instances      --
--------------------------------
local watchingObject = 1
local showUnknown = false
local copyInstance = {}

local function createWatchObjectRow(id, address, size, description, toolTip)

    if not showUnknown and description == "???" then
        return
    end
    local valuePtr
    if size == 2 then
        valuePtr = getPtrOf(address, "uint16_t*")
    elseif size == 4 then
        valuePtr = getPtrOf(address, "uint32_t*")
    else
        valuePtr = getPtrOf(address, "uint8_t*")
    end
    imgui.TableNextRow()
    imgui.TableSetColumnIndex(0)
    imgui.PushItemWidth(65)
    imgui.extra.InputText(string.format("##watchObjectHex%d", id), string.format("%08X", valuePtr[0]), imgui.constant.InputTextFlags.CharsHexadecimal or imgui.constant.InputTextFlags.CharsUppercase)
    imgui.TableSetColumnIndex(1)
    imgui.PushItemWidth(80)
    local changed, strValue = imgui.extra.InputText(string.format("##watchObjectDecimal%d", id), string.format("%d", valuePtr[0]), imgui.constant.InputTextFlags.CharsDecimal)
    if changed then
        local value = tonumber(strValue)
        if value then
            valuePtr[0] = value
        end
    end
    imgui.PushItemWidth(0)
    imgui.TableSetColumnIndex(2)
    imgui.TextUnformatted(description)
    if toolTip and imgui.IsItemHovered() then
        imgui.BeginTooltip()
        imgui.TextUnformatted(toolTip)
        imgui.EndTooltip()
    end
    imgui.TableSetColumnIndex(3)
    if imgui.Button(string.format("Jump##WatchObjectRow%d", id)) then
        PCSX.GUI.jumpToMemory(address, size)
    end
end

local function getChestInfo(chestAddr)
    if not getPtrOf(chestAddr + 96, "uint32_t*")[0] == 0 or not getPtrOf(chestAddr + 132, "uint32_t*")[0] == 255 or getPtrOf(chestAddr + 64, "uint32_t*")[0] == 0 then
        return ""
    end

    local chestIDVal = getPtrOf(chestAddr + 64, "uint32_t*")[0]
    local offset = bit.rshift(chestIDVal, 5)
    local byteBit = bit.band(chestIDVal, 0x1F)

    return string.format("\nThis Chest is located at GlobalFlag uint %d at the bit %d", offset, byteBit)
end

local function infoObjects()
    if imgui.TreeNode("Objects") then
        imgui.PushItemWidth(65)
        imgui.extra.InputText("Object in Front", bit.tohex(getPtrOf(FRONT_OBJECT, "uint32_t*")[0]))
        imgui.SameLine()
    
        if imgui.Button("Watch object") then
            local objInst = math.floor((getPtrOf(FRONT_OBJECT, "uint32_t*")[0] - OBJECT_INSTANCES) / 660) + 1
            if objInst > 0 and objInst < 65 then
                watchingObject = objInst
            end
        end
    
        imgui.extra.InputText("Object walkin in", bit.tohex(getPtrOf(WALKING_IN_OBJECT, "uint32_t*")[0]))
        imgui.SameLine()
    
        if imgui.Button("Watch object##2") then
            local objInst = math.floor((getPtrOf(WALKING_IN_OBJECT, "uint32_t*")[0] - OBJECT_INSTANCES) / 660) + 1
            if objInst > 0 and objInst < 65 then
                watchingObject = objInst
            end
        end
    
        imgui.PushItemWidth(0)

        imgui.SeparatorText("Watching object")
        local changed, value = imgui.SliderInt("Instance", watchingObject, 1, 64, '%d')
        if changed then
            watchingObject = value
        end
        local watchingObjAddr = OBJECT_INSTANCES + ((watchingObject - 1) * 660)
        imgui.TextUnformatted(string.format("%08X: Object Address: ", watchingObjAddr))
        imgui.SameLine()
        if imgui.Button("Jump to Memory##watching object") then
            PCSX.GUI.jumpToMemory(watchingObjAddr, 0x294)
        end
        imgui.SameLine()
        if imgui.Button("Teleport to Player") then
            getPtrOf(watchingObjAddr + 278, "uint16_t*")[0] = getPtrOf(OBJECT_INSTANCES + 278, "uint16_t*")[0]
            getPtrOf(watchingObjAddr + 282, "uint16_t*")[0] = getPtrOf(OBJECT_INSTANCES + 282, "uint16_t*")[0]
            getPtrOf(watchingObjAddr + 286, "uint16_t*")[0] = getPtrOf(OBJECT_INSTANCES + 286, "uint16_t*")[0]
        end
        imgui.SameLine()
        if imgui.Button("Teleport on top of Player") then
            getPtrOf(watchingObjAddr + 278, "uint16_t*")[0] = getPtrOf(OBJECT_INSTANCES + 278, "uint16_t*")[0]
            getPtrOf(watchingObjAddr + 282, "uint16_t*")[0] = getPtrOf(OBJECT_INSTANCES + 282, "uint16_t*")[0]
            getPtrOf(watchingObjAddr + 286, "uint16_t*")[0] = getPtrOf(OBJECT_INSTANCES + 286, "uint16_t*")[0] + 200
        end

        if imgui.Button("Copy instance") then
            copyInstance = {}
            local copyPtr = getPtrOf(watchingObjAddr, "uint64_t*")
            for i = 0, 81 do -- 660 / 8 (-1 cause of index 0)
                if not (i == 34 or i == 35) then -- ignore positioning
                    copyInstance[i + 1] = copyPtr[i]
                end
            end
        end
        imgui.SameLine()
        if imgui.Button("Paste instance") then
            local pastePtr = getPtrOf(watchingObjAddr, "uint64_t*")
            if #copyInstance > 0 then
                for i = 0, 81 do -- 660 / 8 (-1 cause of index 0)
                    if not (i == 34 or i == 35) then -- ignore positioning
                        pastePtr[i] = copyInstance[i + 1]
                    end
                end
            end
        end

        local changed, value = imgui.Checkbox("Show unknown", showUnknown)
        if changed then
            showUnknown = value
        end

        imgui.BeginChild("Instance Variables", 0, 350)
        if imgui.BeginTable("table_watch_obj", 4, imgui.constant.TableFlags.Borders or imgui.constant.TableFlags.SizingFixedFit) then

            imgui.TableSetupColumn("Value (Hex)", imgui.constant.TableColumnFlags.WidthFixed)
            imgui.TableSetupColumn("Value", imgui.constant.TableColumnFlags.WidthFixed)
            imgui.TableSetupColumn("Description", imgui.constant.TableColumnFlags.WidthStretch)
            imgui.TableSetupColumn("Jump", imgui.constant.TableColumnFlags.WidthFixed)

            imgui.TableNextRow(imgui.constant.TableFlags.Headers)
            for i = 0, 3 do
                imgui.TableSetColumnIndex(i)
                imgui.PushID(i)
                local columnName = imgui.TableGetColumnName(i)
                imgui.TableHeader(columnName)
                imgui.PopID()
            end

            id = -1
            createWatchObjectRow(nextID(), watchingObjAddr, 4, "Instance ID")
            createWatchObjectRow(nextID(), watchingObjAddr + 4, 4, "???", "Some other ID like value")
            createWatchObjectRow(nextID(), watchingObjAddr + 8, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 12, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 16, 4, "Status?",
            "0 = Not loaded (no rendering or updates)\n" ..
            "1 = Loading? (automatically set to 2, when forced to 1 no collision)\n" ..
            "2 = Normal State (visible & collision)\n" ..
            "3 = Prepare for deletion (Object will be deleted in function and status set to 4)\n" ..
            "4 = Deleted")
            createWatchObjectRow(nextID(), watchingObjAddr + 20, 4, "HP")
            createWatchObjectRow(nextID(), watchingObjAddr + 24, 4, "Max HP")
            createWatchObjectRow(nextID(), watchingObjAddr + 28, 4, "Timer")
            createWatchObjectRow(nextID(), watchingObjAddr + 32, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 36, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 40, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 44, 4, "Carry Object", "Other object this object is holding")
            createWatchObjectRow(nextID(), watchingObjAddr + 48, 4, "Carry Object X")
            createWatchObjectRow(nextID(), watchingObjAddr + 52, 4, "Carry Object Y")
            createWatchObjectRow(nextID(), watchingObjAddr + 56, 4, "Carry Object Z")
            createWatchObjectRow(nextID(), watchingObjAddr + 60, 4, "Chest Item / Item Drop", "Matching Item ID + 1\n0 = Empty Chest / Vase / Box / Enemy")
            createWatchObjectRow(nextID(), watchingObjAddr + 64, 4, "Chest ID", "This ID will be written when the Chest has been opened.\nOn loading the Map this ID is checked and the Chest state is set to empty if already used."..getChestInfo(watchingObjAddr))
            createWatchObjectRow(nextID(), watchingObjAddr + 68, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 72, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 76, 4, "Stand on Event? (??? Load Event???)")
            createWatchObjectRow(nextID(), watchingObjAddr + 80, 4, "??? Map Event ???")
            createWatchObjectRow(nextID(), watchingObjAddr + 84, 4, "Update / Explosion / Damage Event? (??? Tick Event ???)")
            createWatchObjectRow(nextID(), watchingObjAddr + 88, 4, "Explosion / Damage Event? (??? Touch Event ???)")
            createWatchObjectRow(nextID(), watchingObjAddr + 92, 4, "??? Unload Event ???")
            createWatchObjectRow(nextID(), watchingObjAddr + 96, 4, "Interact Event (ID of event?)", "Will be executed on player talk.\nONLY if Interaction Flag 0x8000 is set!")
            createWatchObjectRow(nextID(), watchingObjAddr + 100, 4, "Texture / Object?")
            createWatchObjectRow(nextID(), watchingObjAddr + 104, 4, "Item ID (For item drop)", "Starts at Item-ID's 31 which equals 0 (Dagger) works up to 914 and play's random SE's")
            createWatchObjectRow(nextID(), watchingObjAddr + 108, 4, "Interaction Flag", "bit 0x8000 is the Flag for all interactable Objects\nInteractable is not Pickupable object!\n"..
            [[
Addresses to Objects:
201100  = Player
2294656 = Shop Item Pickup
3351524 = Box / Vase
8626560 = Talk to person (Jess)
237952  = Talk to person (Yuri, Naomi)
41088   = Talk to person (Naomi desk)
196608  = Crow
205064  = Chicken
106496  = Bird
106496  = Some Inoa-Event Object
229640  = Dog
2228480 = Item Pickup
237696  = Sign
33152   = Chest
24704   = Spring Bean Plate
259     = Fish, Slime, 
65539   = Bee
196867  = Turtle]])
            createWatchObjectRow(nextID(), watchingObjAddr + 112, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 116, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 120, 4, "Object behavior?", "Object behavior like the enemy movement and action selection:\n"..
            "  0 = Any other like NPC's\n"..
            "  1 = Slime\n"..
            "  3 = Turtle\n"..
            "  4 = Item (Pickupable)\n"..
            " 18 = Bee\n"..
            " 19 = Fish\n"..
            " 23 = Vase / Box\n"..
            " 74 = Spring Bean Plate\n"..
            " 91 = Chicken\n"..
            " 92 = Crow\n"..
            " 93 = Bird\n"..
            "101 = Dog\n"..
            "255 = Collectable (only works for Itemdrops?)")
            createWatchObjectRow(nextID(), watchingObjAddr + 124, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 128, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 132, 4, "Chest Flag (255=Active)", "Only works when the Interaction event is 0)")
            createWatchObjectRow(nextID(), watchingObjAddr + 136, 4, "Action")
            createWatchObjectRow(nextID(), watchingObjAddr + 140, 4, "Face Direction")
            createWatchObjectRow(nextID(), watchingObjAddr + 144, 4, "Action")
            createWatchObjectRow(nextID(), watchingObjAddr + 148, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 152, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 156, 4, "Texture Address 1?")
            createWatchObjectRow(nextID(), watchingObjAddr + 160, 4, "Texture Address 2?")
            createWatchObjectRow(nextID(), watchingObjAddr + 164, 4, "Texture Address 3?")
            createWatchObjectRow(nextID(), watchingObjAddr + 168, 4, "Animation Delay Timer")
            createWatchObjectRow(nextID(), watchingObjAddr + 172, 4, "Bound to var on bottom?")
            createWatchObjectRow(nextID(), watchingObjAddr + 176, 4, "Another Timer (animation specific)")
            createWatchObjectRow(nextID(), watchingObjAddr + 180, 4, "Bound to var on top?")
            createWatchObjectRow(nextID(), watchingObjAddr + 184, 4, "Velocity Z")
            createWatchObjectRow(nextID(), watchingObjAddr + 188, 4, "Velocity X")
            createWatchObjectRow(nextID(), watchingObjAddr + 192, 4, "Velocity Y")
            createWatchObjectRow(nextID(), watchingObjAddr + 196, 4, "Velocity X 2?")
            createWatchObjectRow(nextID(), watchingObjAddr + 200, 4, "Velocity Y 2?")
            createWatchObjectRow(nextID(), watchingObjAddr + 204, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 208, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 212, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 216, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 220, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 224, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 228, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 232, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 236, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 240, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 244, 4, "Something with Movement reset")
            createWatchObjectRow(nextID(), watchingObjAddr + 248, 4, "-- Player Movement Gravity?")
            createWatchObjectRow(nextID(), watchingObjAddr + 252, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 256, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 260, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 264, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 268, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 272, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 276, 2, "Pos X???")
            createWatchObjectRow(nextID(), watchingObjAddr + 278, 2, "Pos X")
            createWatchObjectRow(nextID(), watchingObjAddr + 280, 2, "Pos Y???")
            createWatchObjectRow(nextID(), watchingObjAddr + 282, 2, "Pos Y")
            createWatchObjectRow(nextID(), watchingObjAddr + 284, 2, "Pos Z???")
            createWatchObjectRow(nextID(), watchingObjAddr + 286, 2, "Pos Z")
            createWatchObjectRow(nextID(), watchingObjAddr + 288, 4, "Pos X (in Tiles)")
            createWatchObjectRow(nextID(), watchingObjAddr + 292, 4, "Pos Y (in Tiles)")
            createWatchObjectRow(nextID(), watchingObjAddr + 296, 4, "Pos Z (in Tiles)")
            createWatchObjectRow(nextID(), watchingObjAddr + 300, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 304, 4, "Object run into")
            createWatchObjectRow(nextID(), watchingObjAddr + 308, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 312, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 316, 4, "Run against something flag")
            createWatchObjectRow(nextID(), watchingObjAddr + 320, 4, "On ground Flag2?", "Same as OnGroundFlag but has no effect?")
            createWatchObjectRow(nextID(), watchingObjAddr + 324, 4, "On ground Flag")
            createWatchObjectRow(nextID(), watchingObjAddr + 328, 4, "Matching Tile 1")
            createWatchObjectRow(nextID(), watchingObjAddr + 332, 4, "Matching Tile 2")
            createWatchObjectRow(nextID(), watchingObjAddr + 336, 4, "Matching Tile 3")
            createWatchObjectRow(nextID(), watchingObjAddr + 340, 4, "Matching Tile 4")
            createWatchObjectRow(nextID(), watchingObjAddr + 344, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 348, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 352, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 356, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 360, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 364, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 368, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 372, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 376, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 380, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 384, 2, "Ground Type")
            createWatchObjectRow(nextID(), watchingObjAddr + 386, 2, "Ground Type2")
            createWatchObjectRow(nextID(), watchingObjAddr + 388, 2, "_Ground Type", "same as Ground Type")
            createWatchObjectRow(nextID(), watchingObjAddr + 390, 2, "_Ground Type2", "same as Ground Type2")
            createWatchObjectRow(nextID(), watchingObjAddr + 392, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 396, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 400, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 404, 4, "Something with Sprite changing?")
            createWatchObjectRow(nextID(), watchingObjAddr + 408, 4, "Sprite Pos X")
            createWatchObjectRow(nextID(), watchingObjAddr + 412, 4, "Sprite Pos Y")
            createWatchObjectRow(nextID(), watchingObjAddr + 416, 4, "Sprite Pos Z")
            createWatchObjectRow(nextID(), watchingObjAddr + 420, 4, "Z-Buffer")
            createWatchObjectRow(nextID(), watchingObjAddr + 424, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 428, 4, "Number of Sprites to draw")
            createWatchObjectRow(nextID(), watchingObjAddr + 432, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 436, 4, "Pallet")
            createWatchObjectRow(nextID(), watchingObjAddr + 440, 4, "Shadow Object")
            createWatchObjectRow(nextID(), watchingObjAddr + 444, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 448, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 452, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 456, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 460, 4, "Invulnerability timer")
            createWatchObjectRow(nextID(), watchingObjAddr + 464, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 468, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 472, 4, "Center Pos X (Fixed Point 16)")
            createWatchObjectRow(nextID(), watchingObjAddr + 476, 4, "Center Pos Y (Fixed Point 16)")
            createWatchObjectRow(nextID(), watchingObjAddr + 480, 4, "Center Pos Z (Fixed Point 16)")
            createWatchObjectRow(nextID(), watchingObjAddr + 484, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 488, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 492, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 496, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 500, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 504, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 508, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 512, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 516, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 520, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 524, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 528, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 532, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 536, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 540, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 544, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 548, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 552, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 556, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 560, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 564, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 568, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 572, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 576, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 580, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 584, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 588, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 592, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 596, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 600, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 604, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 608, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 612, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 616, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 620, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 624, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 628, 4, "Hide message on pickup (Item drop)", "0 Show Message\n1 Hide Message\n2 Can't pickup")
            createWatchObjectRow(nextID(), watchingObjAddr + 632, 4, "Despawn timer (Item drop)")
            createWatchObjectRow(nextID(), watchingObjAddr + 636, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 640, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 644, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 648, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 652, 4, "???")
            createWatchObjectRow(nextID(), watchingObjAddr + 656, 4, "???")

            imgui.EndTable()
        end
        imgui.EndChild()
        if imgui.TreeNode("Interaction Flags:") then
            imgui.BeginChild("iaf")
            for i = 0, 3 do
                for j = 0, 7 do
                    doCheckBit(watchingObjAddr + 108, tostring(1 + i * 8 + j), "uint32_t*", i * 8 + j)
                    if j < 7 then
                        imgui.SameLine()
                    end
                end
            end
            imgui.EndChild()
            imgui.TreePop()
        end

        imgui.SeparatorText("Object Class")
        imgui.BeginChild("Object Class", 0, 250)
        local watchingObjClass = getPtrOf(watchingObjAddr + 100, "uint32_t*")[0]
        if imgui.BeginTable("table_watch_obj", 4, imgui.constant.TableFlags.Borders or imgui.constant.TableFlags.SizingFixedFit) then

            imgui.TableSetupColumn("Value (Hex)", imgui.constant.TableColumnFlags.WidthFixed)
            imgui.TableSetupColumn("Value", imgui.constant.TableColumnFlags.WidthFixed)
            imgui.TableSetupColumn("Description", imgui.constant.TableColumnFlags.WidthStretch)
            imgui.TableSetupColumn("Jump", imgui.constant.TableColumnFlags.WidthFixed)

            imgui.TableNextRow(imgui.constant.TableFlags.Headers)
            for i = 0, 3 do
                imgui.TableSetColumnIndex(i)
                imgui.PushID(i)
                local columnName = imgui.TableGetColumnName(i)
                imgui.TableHeader(columnName)
                imgui.PopID()
            end

            id = -1
            createWatchObjectRow(nextID(), watchingObjClass, 4, "Object Logic")
            if false then
            createWatchObjectRow(nextID(), watchingObjClass + 4, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 8, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 12, 4, "Some texture thingy")
            createWatchObjectRow(nextID(), watchingObjClass + 16, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 20, 4, "Object behavior")
            createWatchObjectRow(nextID(), watchingObjClass + 24, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 28, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 32, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 36, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 40, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 44, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 48, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 52, 4, "Some Animation thing?")
            createWatchObjectRow(nextID(), watchingObjClass + 56, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 60, 4, "Something with animation delay timer")
            createWatchObjectRow(nextID(), watchingObjClass + 64, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 68, 4, "Item drop white flash thing")
            createWatchObjectRow(nextID(), watchingObjClass + 72, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 76, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 80, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 84, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 88, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 92, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 96, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 100, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 104, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 108, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 112, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 116, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 120, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 124, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 128, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 132, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 136, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 140, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 144, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 148, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 152, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 156, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 160, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 164, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 168, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 172, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 176, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 180, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 184, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 188, 4, "-???")
            createWatchObjectRow(nextID(), watchingObjClass + 192, 4, "-???")
            end
            local objLogicAddr = getPtrOf(watchingObjClass, "uint32_t*")[0]
            if objLogicAddr > 0 then
                createWatchObjectRow(nextID(), objLogicAddr + 0x00, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x04, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x08, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x0c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x10, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x14, 2, "-??? (2b)")
                createWatchObjectRow(nextID(), objLogicAddr + 0x16, 2, "Movement Speed", "Sets the movement speed")
                createWatchObjectRow(nextID(), objLogicAddr + 0x18, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x1c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x20, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x24, 2, "-??? (2b)")
                createWatchObjectRow(nextID(), objLogicAddr + 0x26, 2, "Jump Power")
                createWatchObjectRow(nextID(), objLogicAddr + 0x28, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x2c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x30, 2, "-??? (2b)")
                createWatchObjectRow(nextID(), objLogicAddr + 0x32, 2, "Sprint Speed")
                createWatchObjectRow(nextID(), objLogicAddr + 0x34, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x38, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x3c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x40, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x44, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x48, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x4c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x50, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x54, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x58, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x5c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x60, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x64, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x68, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x6c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x70, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x74, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x78, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x7c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x80, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x84, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x88, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x8c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x90, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x94, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x98, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0x9c, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xa0, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xa4, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xa8, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xac, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xb0, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xb4, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xb8, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xbc, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xc0, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xc4, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xc8, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xcc, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xd0, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xd4, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xd8, 4, "-???")
                createWatchObjectRow(nextID(), objLogicAddr + 0xdc, 4, "-???")
            end

            imgui.EndTable()
        end
        imgui.EndChild()

        imgui.SeparatorText("")
        imgui.TreePop()
    end
end

--------------------------------
--    Object instances end    --
--------------------------------

local function GenInfoCategory()
    if imgui.CollapsingHeader("Info") then

        local time = getPtrOf(GAME_PLAY_TIMER, "uint32_t*")[0]
        local hours = (time / 216000) % 100
        local minutes = time / 3600 % 60
        local seconds = time / 60 % 60
        imgui.TextUnformatted(string.format("Game time: %d:%02d:%02d", hours, minutes, seconds))
        local retries = getPtrOf(RETRIES, "uint8_t*")[0]
        local legendSwordReady = ""
        if retries > 19 then
            legendSwordReady = "(Legendary Sword ready to pick up)"
        end
        imgui.TextUnformatted(string.format("Retries: %d %s", retries, legendSwordReady))
        infoObjects()
        infoPlayer()
        if imgui.TreeNode("Positions") then
            if imgui.TreeNode("Camera") then
                imgui.SeparatorText("Follow")
                imgui.TextUnformatted(string.format("X: %d", getPtrOf(CAMERA_FOLLOW_X, "int32_t*")[0]))
                imgui.TextUnformatted(string.format("Y: %d", getPtrOf(CAMERA_FOLLOW_Y, "int32_t*")[0]))
                imgui.TextUnformatted(string.format("Z: %d", getPtrOf(CAMERA_FOLLOW_Z, "int32_t*")[0]))
                imgui.TreePop()
            end
            infoWarpPosView()
            imgui.TreePop()
        end
        infoPSU()
        if imgui.TreeNode("Save File Descriptions") then
            imgui.TextUnformatted("Global Flags:")
            local windowVisibleX2 = imgui.GetCursorScreenPos() + imgui.GetContentRegionAvail()
            for i = 0, 56 do
                for j = 0, 3 do
                    local data = getPtrOf(GLOBAL_STORY_FLAG, "uint8_t*")[i * 4 + j]
                    imgui.TextUnformatted(string.format("%02X", data))
                    imgui.SameLine()
                    if imgui.IsItemHovered() then
                        imgui.BeginTooltip()
                        imgui.TextUnformatted(string.format("uInt: %d\nByte: %d\n%s", i, i * 4 + j, toBinary(data, 8)))
                        imgui.EndTooltip()
                    end
                end
                imgui.TextUnformatted(" ")
                local latestTextX2 = imgui.GetItemRectMax()
                local nextTextX2 = latestTextX2 + 80
                if i < 56 and nextTextX2 < windowVisibleX2 then
                    imgui.SameLine()
                end
            end
            imgui.TextUnformatted("Local Flags:")
            windowVisibleX2 = imgui.GetCursorScreenPos() + imgui.GetContentRegionAvail()
            for i = 0, 255 do
                imgui.TextUnformatted(string.format("%02X", getPtrOf(LOCAL_STORY_FLAG, "uint8_t*")[i]))
                local latestTextX2 = imgui.GetItemRectMax()
                local nextTextX2 = latestTextX2 + 20
                if i < 255 and nextTextX2 < windowVisibleX2 then
                    imgui.SameLine()
                end
            end

            imgui.TextUnformatted("")
            imgui.TextUnformatted("Flag is a bit check with the first 4 bytes of Global Flags")
            local tableFlags = imgui.constant.TableFlags.Borders or imgui.constant.TableFlags.SizingFixedFit
            if imgui.BeginTable("save_file_descriptions", 3, tableFlags) then
                imgui.TableSetupColumn("Nr", imgui.constant.TableColumnFlags.WidthFixed)
                imgui.TableSetupColumn("Flag", imgui.constant.TableColumnFlags.WidthFixed)
                imgui.TableSetupColumn("Description", imgui.constant.TableColumnFlags.WidthStretch)

                local flag = getPtrOf(SAVE_FILE_DESCRIPTIONS, "int16_t*")
                local description = getPtrOf(SAVE_FILE_DESCRIPTIONS + 2, "uint8_t*")

                local offset = 34

                imgui.TableNextRow(imgui.constant.TableFlags.Headers)
                for i = 0, 2 do
                    imgui.TableSetColumnIndex(i)
                    imgui.PushID(i)
                    local columnName = imgui.TableGetColumnName(i)
                    imgui.TableHeader(columnName)
                    imgui.PopID()
                end

                for i = 0, 41 do -- 42 texts
                    imgui.TableNextRow()
                    imgui.TableSetColumnIndex(0)
                    imgui.TextUnformatted(i + 1)
                    imgui.TableSetColumnIndex(1)
                    imgui.TextUnformatted(string.format("0x%08X", flag[i * offset]))
                    imgui.TableSetColumnIndex(2)
                    imgui.TextUnformatted(ffi.string(description + i * offset, 32))
                end

                imgui.EndTable()
            end
            imgui.TreePop()
        end
        infoDatasBin()
    end
end

-- ########################## General #############################

local function genWarpTypeCombo()
    local ptr = getPtrOf(0x801ac6d8, "uint8_t*")
    local arr = {
        "0-Fade",
        "1-Rapid",
        "2-White",
        "3-Scroll (unused)",
        "4-Dream",
        "5-Warp",
        "6-Gate",
        "7-None",
        "8-Dead",
        "9-Ending (Ends the game)",
        "10-Restart (Go to Map 389 on the Ship)",
        "11-Title (Go to title screen)"
    }
    local itemSelected = ptr[0]
    local flags = 0
    local selectedStr = arr[itemSelected + 1]
    if selectedStr == nil then
        selectedStr = "null"
    end
    if imgui.BeginCombo("##Player Action", selectedStr, flags) then

        for i = 1, #arr do
            local nr = i - 1
            if(imgui.Selectable(arr[i], false)) then
                ptr[0] = nr
            end
        end

        imgui.EndCombo()
    end
end

local function GenGeneralCategory()
    if imgui.CollapsingHeader("General") then
        doCheckInt(SE_BGM_MENU_ENABLED, "Allow SE/BGM menu", "uint32_t*", 0, 1)
        doCheckInt(DEBUG_MODE_FLAG, "Debug mode", "uint32_t*", 0x80000000, 0)
        if imgui.IsItemHovered() then
            imgui.BeginTooltip()
            imgui.TextUnformatted("Enables the debug menu and it's functions")
            imgui.EndTooltip()
        end
        if imgui.TreeNode("Debug") then
            doCheckBit(SHOW_DEBUG_TEXT_FLAG, "Show Debug Text", "uint8_t*", 7)
            doCheckBit(DEBUG_OPTIONS, "1: Lines", "int16_t*", 0)
            doCheckBit(DEBUG_OPTIONS, "2: Prims", "int16_t*", 1)
            doCheckBit(DEBUG_OPTIONS, "3: Warp Info", "int16_t*", 2)
            doCheckBit(DEBUG_OPTIONS, "4: Forth Warp", "int16_t*", 3)
            doCheckBit(DEBUG_OPTIONS, "5: Logic", "int16_t*", 4)
            doCheckBit(DEBUG_OPTIONS, "6: Init Error", "int16_t*", 5)
            doCheckBit(DEBUG_OPTIONS, "7: BG", "int16_t*", 6)
            doCheckBit(DEBUG_OPTIONS, "8: Cell No (WIP)", "int16_t*", 7)
            doCheckBit(DEBUG_OPTIONS, "9: Defence Hitbox", "int16_t*", 8)
            doCheckBit(DEBUG_OPTIONS, "10: Attack Hitbox", "int16_t*", 9)
            doCheckBit(DEBUG_OPTIONS, "11: PC Balance", "int16_t*", 10)
            doCheckBit(DEBUG_OPTIONS, "12: Damage", "int16_t*", 11)
            --doCheckBit(mem, DEBUG_OPTIONS, "13: ", "int16_t*", 12)
            --doCheckBit(mem, DEBUG_OPTIONS, "14: ", "int16_t*", 13)
            --doCheckBit(mem, DEBUG_OPTIONS, "15: ", "int16_t*", 14)
            --doCheckBit(mem, DEBUG_OPTIONS, "16: ", "int16_t*", 15)

            createSliderIntButtons(DEBUG_WARP_MAP, "Warp Map", 0, 482, "uint16_t*")
            genWarpTypeCombo()
            imgui.SameLine()
            if imgui.Button("Warp") then
                local showDebug = getPtrOf(SHOW_DEBUG_TEXT_FLAG, "uint8_t*")
                showDebug[0] = bit.bor(showDebug[0], 0x80)
                local debugOptions = getPtrOf(DEBUG_OPTIONS, "uint8_t*")
                debugOptions[0] = bit.bor(debugOptions[0], 0x08)
                local pad2Pressed = getPtrOf(CONTROLLER_2_KEY_PRESSED, "uint16_t*")
                pad2Pressed[0] = bit.bor(pad2Pressed[0], 0x0800) -- Controller 2 Start Button
            end
            doSliderInt(DEBUG_MAP_LIMITS, "Map Limits", 0, 60, "int8_t*")
            doSliderInt(DEBUG_SLOW_SPEED, "Slow Speed (WIP)", 2, 99, "int8_t*")
            imgui.TreePop()
        end
        local saveMode
        local saveModeNum = getPtrOf(SAVE_STYLE, "uint32_t*")[0]
        if saveModeNum == 1 then
            saveMode = "Normal"
        elseif saveModeNum == 2 then
            saveMode = "Without var set"
        elseif saveModeNum == 3 then
            saveMode = "Test save mode"
        else
            saveMode = "Deactivated"
        end
        doSliderInt(SAVE_STYLE, string.format("Saving mode (%s)", saveMode), 0, 3, "uint32_t*")
        if imgui.Button("Open save dialog") then
            local ptr = getPtrOf(SAVE_ACTION_FLAG, "uint16_t*")
            if ptr[0] == 0 then
                ptr[0] = 10000
            end
        end
        doCheckInt(GUI_STATE, "Show GUI", "uint32_t*", 1, 0)
    end
end

-- ######################## Event System ##########################
local keepUnblocked = false

local function getPlayerActionArray()
    local arr = {
        "00 - No Action",
        "01 - Moving",
        "02 - Start Jump while moving",
        "03 - Sprint",
        "04 - Sprint dash",
        "05 - Pickup Object",
        "06 - Start Jump with Object while moving",
        "07 - Moveing with Object",
        "08 - Flail hit (Iron)",
        "09 - Throw Object",
        "0A - Throw Object while Jumping",
        "0B - ",
        "0C - Hold Object",
        "0D - Sleeping",
        "0E - Climbing",
        "0F - Swimming (slow)",
        "10 - Attack Sword (Dagger, Legend Sword)",
        "11 - Attack Wand (Ice + Charged, Fire + Charged)",
        "12 - Attack Flail (Iron)",
        "13 - Attack Bow (Hunter, Willow + Charged)",
        "14 - Jump Attack Sword (Dagger)",
        "15 - Jump Attack Wand (Ice + Fire)",
        "16 - Jump Attack Flail (Iron)",
        "17 - Jump Attack Bow (Hunter, Willow)",
        "18 - Charge Attack Sword (Sword)",
        "19 - Charge Attack Flail (Steel)",
        "1A - In Minecart",
        "1B - In Minecart moving",
        "1C - Damage Knockback",
        "1D - Swimming (still)",
        "1E - Minecart stopping",
        "1F - ",
        "20 - Enter Sand",
        "21 - Exit Sand",
        "22 - In Sand",
        "23 - In Sand moving",
        "24 - In Sand dash",
        "25 - (???) Flail hit wall (steel)",
        "26 - ",
        "27 - (???) Flail dash hit wall (steel)",
        "28 - Swimming (dash)",
        "29 - Prepare Sprint",
        "2A - Stop Sprint",
        "2B - Start Jump",
        "2C - Jump + Moving",
        "2D - Jump",
        "2E - Start Jump with Object",
        "2F - Jump + Moving with Object",
        "30 - Jump with Object",
        "31 - Damage Taken",
        "32 - Start Spellcast",
        "33 - Loop Spellcast",
        "34 - End Spellcast",
        "35 - Climb (still)",
        "36 - Loading Map?",
        "37 - Minecart moving fast",
        "38 - (???) Throw Object",
        "39 - ",
        "3A - Damage Taken (swimming)",
        "3B - Damage Knockback (swimming)",
        "3C - ",
        "3D - (???) Throw Object while jumping",
        "3E - Sprint against wall",
        "3F - Attack Sword (Sword)",
        "40 - Attack Flail (Steel)",
        "41 - Jump Attack Sword (Sword)",
        "42 - Jump Attack Flail (Steel)",
        "43 - (???) Charged Attack Flail (Iron)",
        "44 - Attack Sword (Fiend Blade)",
        "45 - (???) Attack Flail (Iron)",
        "46 - Jump Attack Sword (Fiend Blade)",
        "47 - (???) Jump Attack Flail (Iron)",
        "48 - (???) Charged Attack Flail short (Iron)",
        "49 - Attack Sword (Holy Sword)",
        "4A - (???) Attack Flail (Iron)",
        "4B - Jump Attack Sword (Holy Sword)",
        "4C - (???) Jump Attack Flail (Iron)",
        "4D - (???) Charged Attack Flail short (Iron)",
        "4E - Dead",
        "4F - ",
        "50 - Start Pray / Mourn",
        "51 - Pray / Mourn",
        "52 - Stop Pray / Mourn",
        "53 - Waking up Laying",
        "54 - Awake Laying",
        "55 - Shacke head Laying",
        "56 - ",
        "57 - Monster Alundra O.O",
        "58 - Get thrown away",
        "59 - End Earthquake",
        "5A - Start Earthquake",
        "5B - Start Victory Pose",
        "5C - Victory Pose",
        "5D - Victory Pose shine",
        -- From here messed up things crashing the game
        "5E - ",
        "5F - ",
        "60 - ",
        "61 - ",
        "62 - ",
        "63 - ",
        "64 - ",
        "65 - ",
        "66 - ",
        "67 - ",
        "68 - ",
        "69 - ",
        "6A - ",
        "6B - ",
        "6C - ",
        "6D - ",
        "6E - ",
        "6F - ",
        "70 - ",
        "71 - ",
        "72 - ",
        "73 - ",
        "74 - ",
        "75 - ",
        "76 - ",
        "77 - ",
        "78 - ",
        "79 - ",
        "7A - ",
        "7B - ",
        "7C - ",
        "7D - ",
        "7E - ",
        "7F - ",
        "80 - ",
        "81 - ",
        "82 - ",
        "83 - ",
        "84 - ",
        "85 - ",
        "86 - ",
        "87 - ",
        "88 - ",
        "89 - ",
        "8A - ",
        "8B - ",
        "8C - ",
        "8D - ",
        "8E - ",
        "8F - ",
        "90 - ",
        "91 - ",
        "92 - ",
        "93 - ",
        "94 - ",
        "95 - ",
        "96 - ",
        "97 - ",
        "98 - ",
        "99 - ",
        "9A - ",
        "9B - ",
        "9C - ",
        "9D - ",
        "9E - ",
        "9F - ",
        "A0 - ",
        "A1 - ",
        "A2 - ",
        "A3 - ",
        "A4 - ",
        "A5 - ",
        "A6 - ",
        "A7 - ",
        "A8 - ",
        "A9 - ",
        "AA - ",
        "AB - ",
        "AC - ",
        "AD - ",
        "AE - ",
        "AF - ",
        "B0 - ",
        "B1 - ",
        "B2 - ",
        "B3 - ",
        "B4 - ",
        "B5 - ",
        "B6 - ",
        "B7 - ",
        "B8 - ",
        "B9 - ",
        "BA - ",
        "BB - ",
        "BC - ",
        "BD - ",
        "BE - ",
        "BF - ",
        "C0 - ",
        "C1 - ",
        "C2 - ",
        "C3 - ",
        "C4 - ",
        "C5 - ",
        "C6 - ",
        "C7 - ",
        "C8 - ",
        "C9 - ",
        "CA - ",
        "CB - ",
        "CC - ",
        "CD - ",
        "CE - ",
        "CF - ",
        "D0 - ",
        "D1 - ",
        "D2 - ",
        "D3 - ",
        "D4 - ",
        "D5 - ",
        "D6 - ",
        "D7 - ",
        "D8 - ",
        "D9 - ",
        "DA - ",
        "DB - ",
        "DC - ",
        "DD - ",
        "DE - ",
        "DF - ",
        "E0 - ",
        "E1 - ",
        "E2 - ",
        "E3 - ",
        "E4 - ",
        "E5 - ",
        "E6 - ",
        "E7 - ",
        "E8 - ",
        "E9 - ",
        "EA - ",
        "EB - ",
        "EC - ",
        "ED - ",
        "EE - ",
        "EF - ",
        "F0 - ",
        "F1 - ",
        "F2 - ",
        "F3 - ",
        "F4 - ",
        "F5 - ",
        "F6 - ",
        "F7 - ",
        "F8 - ",
        "F9 - ",
        "FA - ",
        "FB - ",
        "FC - ",
        "FD - ",
        "FE - ",
        "FF - ",
    }
    return arr
end

local function GenEventSystemCategory()
    if imgui.CollapsingHeader("Event system") then
        if imgui.TreeNode("Blocking") then
            if imgui.Button("Unblock") then
                getPtrOf(BLOCKING_FLAG, "int32_t*")[0] = 0
            end

            local changed, value = imgui.Checkbox("Keep unblocked", keepUnblocked)
            keepUnblocked = value
            if value then
                getPtrOf(BLOCKING_FLAG, "int32_t*")[0] = 0
            end
            
            imgui.TextUnformatted("Blocking Flag:")
            doCheckBit(BLOCKING_FLAG, "1: Inventory", "int32_t*", 0)
            doCheckBit(BLOCKING_FLAG, "2: same as 1", "int32_t*", 1)
            doCheckBit(BLOCKING_FLAG, "3: Inventory and Movement", "int32_t*", 2)
            doCheckBit(BLOCKING_FLAG, "4: Inventory + Movement + Enemy + Animation", "int32_t*", 3)
            doCheckBit(BLOCKING_FLAG, "5: same as 3 (update of KI?)", "int32_t*", 4)
            doCheckBit(BLOCKING_FLAG, "6: Inventory + Movement + Enemy", "int32_t*", 5)
            doCheckBit(BLOCKING_FLAG, "7: same as 4", "int32_t*", 6)
            doCheckBit(BLOCKING_FLAG, "8: same as 3", "int32_t*", 7)

            imgui.TreePop()
        end
        if imgui.TreeNode("Player Action") then
            local ptr = getPtrOf(PLAYER_ACTION_FLAG, "uint32_t*")
            local arr = getPlayerActionArray()
            local itemSelected = ptr[0]
            local flags = 0
            if imgui.BeginCombo("##Player Action", arr[itemSelected + 1], flags) then

                for i = 1, #arr do
                    local nr = i - 1
                    if(imgui.Selectable(arr[i], false)) then
                        ptr[0] = nr
                    end
                end

                imgui.EndCombo()
            end

            imgui.TreePop()
        end
    end
end

-- ########################## Inventory ###########################
local showAllItems = false
local itemPropIndex = ffi.new("uint8_t[1]", 0);

local function createItemPropField(index, name, desc)
    local itemProperties = getPtrOf(ITEM_PROPERTIES, "uint16_t*")
    imgui.PushItemWidth(45)
    local changed, value = imgui.extra.InputText(string.format("%s##ItemProperties_%d", name, index), string.format("%d", (itemProperties + 5 * itemPropIndex[0] + index)[0]), imgui.constant.InputTextFlags.CharsDecimal)
    if changed and (value == "") == false then
        (itemProperties + 5 * itemPropIndex[0] + index)[0] = tonumber(value)
    end
    if (desc == nil) == false and imgui.IsItemHovered() then
        imgui.BeginTooltip()
        imgui.TextUnformatted(desc)
        imgui.EndTooltip()
    end
    imgui.PushItemWidth(0)
end

local function GenInventoryCategory()
    if imgui.CollapsingHeader("Inventory") then

        L_createSliderIntButtons(itemPropIndex, "ItemProperties ItemID", 0, 99)
        createItemPropField(0, "Inv Slot", "The inventory slot to be selected when this item is picked")
        imgui.SameLine()
        createItemPropField(1, "Replacable?", "Flag to mark that this Item can be replaced or invert the replacement order?")
        imgui.SameLine()
        createItemPropField(2, "Prio", "The priority of replacement.\nThis value determines which Item will be used when multiple in the same Slot are present.\nFor example Dagger has 0, Sword 1, Fiend Blade 2 etc.")
        imgui.SameLine()
        createItemPropField(3, "Max Count", "Maximum count of Items\nFor example Herbs has 9")
        imgui.SameLine()
        createItemPropField(4, "Icon", "The Icon to be displayed")

        imgui.TextUnformatted("")

        doSliderInt(GILDER, "Gilder", 0, 9999, "int16_t*")
        doSliderInt(FALCONS, "Falcons", 0, 50, "int16_t*")
        doSliderInt(ITEMS_START + 0xF0, "Keys", 0, 99, "int32_t*") -- 60

        local changed, value = imgui.Checkbox("Show all items", showAllItems)
        if changed then
            showAllItems = value
        end

        if imgui.TreeNode("Weapons") then
            addItemCheck(0, "Dagger", "Dagger from the start of the game")
            addItemCheck(1, "Sword")
            addItemCheck(2, "Fiend Blade")
            addItemCheck(3, "Holy Sword")
            addItemCheck(7, "Legend Sword")
            addItemCheck(8, "Iron Flail")
            addItemCheck(9, "Steel Flail")
            if showAllItems then
                addItemCheck(10, "Unobtainable Flail 1 (unused)")
                addItemCheck(11, "Unobtainable Flail 2 (unused)")
            end
            addItemCheck(4, "Hunters Bow")
            addItemCheck(5, "Willow Box")
            if showAllItems then
                addItemCheck(12, "Unobtainable Ice Wand (unused)")
            end
            addItemCheck(13, "Ice Wand")
            if showAllItems then
                addItemCheck(14, "Unobtainable Fire Wand (unused)")
            end
            addItemCheck(15, "Fire Wand")
            addItemCheck(6, "Spirit Wand")
            imgui.TreePop()
        end
        if imgui.TreeNode("Armor") then
            addItemCheck(16, "Cloth Armor")
            addItemCheck(17, "Leather Armor")
            addItemCheck(18, "Ancient Armor")
            addItemCheck(19, "Silver Armor")
            if showAllItems then
                addItemCheck(20, "(unused)##i20") -- Secret Pass variant
                addItemCheck(21, "(unused)##i21") -- Jess' chest as item?
                addItemCheck(22, "(unused)##i22") -- Spring bean variant
                addItemCheck(23, "(unused)##i23") -- Red strenght potion
            end
            addItemCheck(24, "Short Boots")
            addItemCheck(25, "Long Boots")
            addItemCheck(26, "Merman Boots")
            addItemCheck(27, "Charm Boots")
            if showAllItems then
                addItemCheck(28, "(unused)##i28") -- Green magic potion
                addItemCheck(29, "(unused)##i29") -- Blue wonder essence
            end
            imgui.TreePop()
        end
        if imgui.TreeNode("Usable") then
            addItemCheck(30, "Spring Bean")
            addItemCheck(31, "Sand Cape")
            if showAllItems then
                addItemCheck(33, "(unused)##i33") -- Curious Key variant
            end
            addItemCheck(34, "Bomb")
            doSliderInt(0x801dd8c0, "Herbs", 0, 9, "int32_t*") -- 35
            addItemCheck(36, "Strength Elixyr")
            addItemCheck(37, "Magic Elixyr")
            addItemCheck(38, "Wonder Essence")
            addItemCheck(39, "Aqua Cape")
            addItemCheck(40, "Stenght Tonic")
            if showAllItems then
                addItemCheck(41, "(unused)##i41") -- System error
            end
            addItemCheck(42, "Earth Scroll")
            addItemCheck(43, "Earth Book")
            addItemCheck(44, "Water Scroll")
            addItemCheck(45, "Water Book")
            addItemCheck(46, "Fire Scroll")
            addItemCheck(47, "Fire Book")
            addItemCheck(48, "Wind Scroll")
            addItemCheck(49, "Wind Book")
            addItemCheck(50, "Olga's Ring")
            if showAllItems then
                addItemCheck(51, "Oak's Ring (unused)")
            end
            addItemCheck(52, "Silver Armlet")
            addItemCheck(53, "Nava's Charm")
            addItemCheck(54, "Recovery Ring")
            if showAllItems then
                addItemCheck(55, "Refresher (unused)")
                addItemCheck(57, "(unused)##i57") -- Save book (the book used to save the game)
            end
            addItemCheck(58, "Power Glove")
            imgui.TreePop()
        end
        if imgui.TreeNode("Quest Items") then
            addItemCheck(32, "Broken Armor")
            addItemCheck(56, "Secret Pass")
            addItemCheck(59, "Elevator Key")
            addItemCheck(72, "Sluice Key")
            addItemCheck(73, "Bonquet")
            addItemCheck(74, "Small Key")
            addItemCheck(75, "Jess' Letter")
            addItemCheck(76, "Tree Gem")
            addItemCheck(77, "Zolist's Stone")
            if showAllItems then
                addItemCheck(78, "(unused)##i78") -- Gilded Falcon
                addItemCheck(79, "(unused)##i79") -- Magic Seed
                addItemCheck(80, "(unused)##i80") -- Smal Crystal
                addItemCheck(81, "(unused)##i81") -- Large Crystal
                addItemCheck(82, "(unused)##i82") -- Life Vessel
                addItemCheck(83, "(unused)##i83") -- Dew of Life
                addItemCheck(84, "(unused)##i84") -- Drop of Life
                addItemCheck(85, "(unused)##i85") -- Water if Life
            end
            addItemCheck(86, "Book of Runes")
            addItemCheck(87, "Book of Elna")
            addItemCheck(88, "Curious Key") -- Chest pickup softlock
            imgui.TreePop()
        end
        if imgui.TreeNode("Crests") then
            addItemCheck(61, "Ruby Crest")
            addItemCheck(62, "Sapphire Crest")
            addItemCheck(63, "Topaz Crest")
            addItemCheck(64, "Agate Crest")
            addItemCheck(65, "Garnet Crest")
            addItemCheck(66, "Emeral Crest")
            addItemCheck(67, "Diamond Crest")
            if showAllItems then
                addItemCheck(68, "(unused)##i68") -- 1 Gilder
                addItemCheck(69, "(unused)##i69") -- 5 Gilder
                addItemCheck(70, "(unused)##i70") -- 10 Gilder
                addItemCheck(71, "(unused)##i71") -- 30 Gilder
            end
            imgui.TreePop()
        end
    end
end

-- ##################################################################################
--                               Assembly reading
-- ##################################################################################
local registryNames = {
    "r0", "at", "v0", "v1", "a0", "a1", "a2", "a3", 
    "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7", 
    "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7", 
    "t8", "t9", "k0", "k1", "gp", "sp", "fp", "ra"
}

-- I Format
--  Opcode   rs   rt   Immediate
--|   6b   | 5b | 5b |    16b    |

-- J Format
--  Opcode   Pseudo
--|   6b   |   26b  |

-- R Format
--  Opcode   rs   rt   rd   shift   func
--|   6b   | 5b | 5b | 5b |   5b  |  6b  |

local function getRegistryName(code)
    return registryNames[code + 1] or string.format("0x%X", code)
end

local function IRegFormat(instr)
    local rs = bit.band(bit.rshift(instr, 21), 0x1F)  -- Bits 25-21
    local rt = bit.band(bit.rshift(instr, 16), 0x1F)  -- Bits 20-16
    local imm = bit.band(instr, 0xFFFF)  -- Last 16 Bit

    return rs, rt, imm
end

local function RRegFormat(instr)
    local rs = bit.band(bit.rshift(instr, 21), 0x1F)  -- Bits 25-21
    local rt = bit.band(bit.rshift(instr, 16), 0x1F)  -- Bits 20-16
    local rd = bit.band(bit.rshift(instr, 11), 0x1F)  -- Bits 15-11
    local shift = bit.band(bit.rshift(instr, 6), 0x1F)  -- Bits 10-6

    return rs, rt, rd, shift
end

local function decodeBcondZ(instr)
    local rs, rt, imm = IRegFormat(instr)

    if rt == 0x00 then
        return "BLTZ"
    elseif rt == 0x01 then
        return "BGEZ"
    else
        return "Unknown BcondZ instruction"
    end
end

local function decodeBEQ(instr)
    local rs, rt, imm = IRegFormat(instr)

    if rt == 0x00 then
        return "BEQZ"
    end
    return "BEQ"
end

local function decodeADDU(instr)
    local rs, rt, rd, shift = RRegFormat(instr)

    if rt == 0x00 then
        return "MOVE", string.format("%s = %s", getRegistryName(rd), getRegistryName(rs))
    end
    return "ADDU", string.format("%s = %s + %s", getRegistryName(rd), getRegistryName(rt), getRegistryName(rs))
end

local function decodeADDIU(instr)
    local rs, rt, imm = IRegFormat(instr)
    local text

    if(rs == rt) then
        text = string.format("%s += 0x%s", getRegistryName(rs), bit.tohex(imm))
    else
        text = string.format("%s = %s + 0x%s", getRegistryName(rs), getRegistryName(rt), bit.tohex(imm))
    end

    return "ADDIU", text
end

local function decodeSW(instr)
    local rs, rt, imm = IRegFormat(instr)
    local text

    text = string.format("(%s + 0x%s) = %s", getRegistryName(rs), bit.tohex(imm), getRegistryName(rt))

    return "SW", text
end

local function decodeInstruction()
    local pc = registers.pc
    local code = getPtrOf(pc, "uint32_t*")[0] --bit.bswap(getPtrOf(pc, "uint32_t*")[0])

    local instrName = "N/A"
    local text = ""

    local opCode = bit.rshift(code, 26)

    if code == 0 then -- NOP
        instrName = "NOP"
    elseif opCode == 0x01 then -- BcondZ
        instrName = decodeBcondZ(code)
    elseif opCode == 0x02 then -- J
        instrName = "J"
    elseif opCode == 0x03 then -- JAL
        instrName = "JAL"
    elseif opCode == 0x04 then -- BEQ 
        instrName = decodeBEQ(code)
    elseif opCode == 0x05 then -- BNE
        instrName = "BNE"
    elseif opCode == 0x06 then -- BLEZ 
        instrName = "BLEZ"
    elseif opCode == 0x07 then -- BGTZ
        instrName = "BGTZ"
    elseif opCode == 0x08 then -- ADDI
        instrName = "ADDI"
    elseif opCode == 0x09 then -- ADDIU
        instrName, text = decodeADDIU(code)
    elseif opCode == 0x0A then -- SLTI
        instrName = "SLTI"
    elseif opCode == 0x0B then -- SLTIU
        instrName = "SLTIU"
    elseif opCode == 0x0C then -- ANDI
        instrName = "ANDI"
    elseif opCode == 0x0D then -- ORI
        instrName = "ORI"
    elseif opCode == 0x0E then -- XORI
        instrName = "XORI"
    elseif opCode == 0x0F then -- LUI
        instrName = "LUI"
    elseif opCode == 0x10 then -- COP0
        instrName = "COP0"
    elseif opCode == 0x11 then -- COP1
        instrName = "COP1"
    elseif opCode == 0x12 then -- COP2
        instrName = "COP2"
    elseif opCode == 0x13 then -- COP3
        instrName = "COP3"
    elseif opCode == 0x20 then -- LB
        instrName = "LB"
    elseif opCode == 0x21 then -- LH
        instrName = "LH"
    elseif opCode == 0x22 then -- LWL
        instrName = "LWL"
    elseif opCode == 0x23 then -- LW
        instrName = "LW"
    elseif opCode == 0x24 then -- LBU
        instrName = "LBU"
    elseif opCode == 0x25 then -- LHU
        instrName = "LHU"
    elseif opCode == 0x26 then -- LWR
        instrName = "LWR"
    elseif opCode == 0x28 then -- SB
        instrName = "SB"
    elseif opCode == 0x29 then -- SH
        instrName = "SH"
    elseif opCode == 0x2A then -- SWL
        instrName = "SWL"
    elseif opCode == 0x2B then -- SW
        instrName, text = decodeSW(code)
    elseif opCode == 0x2E then -- SWR
        instrName = "SWR"
    elseif opCode == 0x30 then -- LWC0
        instrName = "LWC0"
    elseif opCode == 0x31 then -- LWC1
        instrName = "LWC1"
    elseif opCode == 0x32 then -- LWC2
        instrName = "LWC2"
    elseif opCode == 0x33 then -- LWC3
        instrName = "LWC3"
    elseif opCode == 0x38 then -- SWC0
        instrName = "SWC0"
    elseif opCode == 0x39 then -- SWC1
        instrName = "SWC1"
    elseif opCode == 0x3A then -- SWC2
        instrName = "SWC2"
    elseif opCode == 0x3B then -- SWC3
        instrName = "SWC3"
    elseif opCode == 0x00 then
        local func = bit.band(code, 0x3F)
        if func == 0x00 then -- SLL
            instrName = "SLL"
        elseif func == 0x02 then -- SRL
            instrName = "SRL"
        elseif func == 0x03 then -- SRA
            instrName = "SRA"
        elseif func == 0x04 then -- SLLV
            instrName = "SLLV"
        elseif func == 0x06 then -- SRLV
            instrName = "SRLV"
        elseif func == 0x07 then -- SRAV
            instrName = "SRAV"
        elseif func == 0x08 then -- JR
            instrName = "JR"
        elseif func == 0x09 then -- JALR
            instrName = "JALR"
        elseif func == 0x0C then -- SYSCALL
            instrName = "SYSCALL"
        elseif func == 0x0D then -- BREAK
            instrName = "BREAK"
        elseif func == 0x10 then -- MFHI
            instrName = "MFHI"
        elseif func == 0x11 then -- MTHI
            instrName = "MTHI"
        elseif func == 0x12 then -- MFLO
            instrName = "MFLO"
        elseif func == 0x13 then -- MTLO
            instrName = "MTLO"
        elseif func == 0x18 then -- MULT
            instrName = "MULT"
        elseif func == 0x19 then -- MULTU
            instrName = "MULTU"
        elseif func == 0x1A then -- DIV
            instrName = "DIV"
        elseif func == 0x1B then -- DIVU
            instrName = "DIVU"
        elseif func == 0x20 then -- ADD
            instrName = "ADD"
        elseif func == 0x21 then -- ADDU
            instrName, text = decodeADDU(code)
        elseif func == 0x22 then -- SUB
            instrName = "SUB"
        elseif func == 0x23 then -- SUBU
            instrName = "SUBU"
        elseif func == 0x24 then -- AND
            instrName = "AND"
        elseif func == 0x25 then -- OR
            instrName = "OR"
        elseif func == 0x26 then -- XOR
            instrName = "XOR"
        elseif func == 0x27 then -- NOR
            instrName = "NOR"
        elseif func == 0x2A then -- SLT
            instrName = "SLT"
        elseif func == 0x2B then -- SLTU
            instrName = "SLTU"
        end
    end
    
    return string.format("%0-8X   %-8s: %s", pc, instrName, text)
end

local function decodeInstruction2()
    local pc = registers.pc
    local code = getPtrOf(pc, "uint32_t*")[0]

    -- Opcode extrahieren
    local op = bit.band(bit.rshift(code, 26), 0x3F)
    local rs = bit.band(bit.rshift(code, 21), 0x1F)
    local rt = bit.band(bit.rshift(code, 16), 0x1F)
    local rd = bit.band(bit.rshift(code, 11), 0x1F)
    local shamt = bit.band(bit.rshift(code, 6), 0x1F)
    local funct = bit.band(code, 0x3F)
    local imm = bit.tobit(bit.band(code, 0xFFFF))  -- Signed Immediate value

    local pcHex = string.format("%s", bit.tohex(pc + 0x80000000))
    local before = string.format("0x%X", registers.GPR.r[rt])  -- pre-instruction value
    local after = nil
    local inner = "unknown"
    local instructionName = "N/A"

    if op == 0x09 then  -- ADDIU (addi without overflow)
        instructionName = "ADDIU"
        after = string.format("0x%X", registers.GPR.r[rs] + imm)
        inner = string.format("%s += 0x%X", getRegistryName(rt), imm)
    elseif op == 0x23 then  -- LW (Load Word)
        instructionName = "LW"
        local addr = registers.GPR.r[rs] + imm
        after = string.format("0x%X", getPtrOf(addr, "uint32_t*")[0])
        inner = string.format("%s = value(0x%X)", getRegistryName(rt), addr)
    elseif op == 0x2B then  -- SW (Store Word)
        instructionName = "SW"
        local addr = registers.GPR.r[rs] + imm
        before = string.format("0x%X", getPtrOf(addr, "uint32_t*")[0])
        after = string.format("0x%X", registers.GPR.r[rt])
        inner = string.format("value(0x%X) = %s", addr, getRegistryName(rt))
    elseif op == 0x08 then  -- ADDI (signed immediate addition)
        instructionName = "ADDI"
        after = string.format("0x%X", registers.GPR.r[rs] + imm)
        inner = string.format("%s += 0x%X", getRegistryName(rt), imm)
    elseif op == 0x04 then  -- BEQ (Branch if equal)
        if registers.GPR.r[rs] == registers.GPR.r[rt] then
            instructionName = "BEQ"
            after = string.format("jump 0x%X", pc + 4 + (imm * 4))
            inner = string.format("if (%s == %s) goto 0x%X", getRegistryName(rs), getRegistryName(rt), pc + 4 + (imm * 4))
        end
    elseif op == 0x02 then  -- JUMP (J-Typ)
        instructionName = "JUMP"
        local target = bit.band(pc, 0xF0000000) + (bit.band(code, 0x3FFFFFF) * 4)
        after = string.format("jump 0x%X", target)
        inner = string.format("goto 0x%X", target)
    elseif op == 0x0D then  -- ORI
        instructionName = "ORI"
        after = string.format("0x%08X", bit.bor(registers.GPR.r[rs], imm))
        inner = string.format("%s = %s | 0x%X", getRegistryName(rt), getRegistryName(rs), imm)   
    elseif op == 0x0C then  -- ANDI
        instructionName = "ANDI"
        after = string.format("0x%08X", bit.band(registers.GPR.r[rs], imm))
        inner = string.format("%s = %s & 0x%X", getRegistryName(rt), getRegistryName(rs), imm)
    elseif op == 0x0A then  -- SLTI
        instructionName = "SLTI"
        after = registers.GPR.r[rs] < imm and "1" or "0"
        inner = string.format("%s = (%s < 0x%X)", getRegistryName(rt), getRegistryName(rs), imm)
    elseif op == 0x0F then  -- LUI
        instructionName = "LUI"
        after = string.format("0x%s", bit.tohex(bit.lshift(bit.band(imm, 0xFFFF), 16)))
        inner = string.format("%s = 0x%X << 16", getRegistryName(rt), imm)
    elseif op == 0x03 then  -- JAL
        instructionName = "JAL"
        local target = bit.band(pc, 0xF0000000) + (bit.band(code, 0x3FFFFFF) * 4)
        after = string.format("jump 0x%X", target)
        inner = string.format("ra = 0x%X; goto 0x%X", pc + 8, target)
    elseif op == 0 then  -- R-Typ Instruktionen (funct using)
        if funct == 0x20 then  -- ADD
            instructionName = "ADD"
            after = string.format("0x%X", registers.GPR.r[rs] + registers.GPR.r[rt])
            inner = string.format("%s = %s + %s", getRegistryName(rd), getRegistryName(rs), getRegistryName(rt))
        elseif funct == 0x22 then  -- SUB
            instructionName = "SUB"
            after = string.format("0x%X", registers.GPR.r[rs] - registers.GPR.r[rt])
            inner = string.format("%s = %s - %s", getRegistryName(rd), getRegistryName(rs), getRegistryName(rt))
        elseif funct == 0x24 then  -- AND
            instructionName = "AND"
            after = string.format("0x%X", bit.band(registers.GPR.r[rs], registers.GPR.r[rt]))
            inner = string.format("%s = %s & %s", getRegistryName(rd), getRegistryName(rs), getRegistryName(rt))
        elseif funct == 0x25 then  -- OR
            instructionName = "OR"
            after = string.format("0x%X", bit.bor(registers.GPR.r[rs], registers.GPR.r[rt]))
            inner = string.format("%s = %s | %s", getRegistryName(rd), getRegistryName(rs), getRegistryName(rt))
        elseif funct == 0x2A then  -- SLT (Set Less Than)
            instructionName = "SLT"
            after = registers.GPR.r[rs] < registers.GPR.r[rt] and "1" or "0"
            inner = string.format("%s = (%s < %s)", getRegistryName(rd), getRegistryName(rs), getRegistryName(rt))
        elseif funct == 0x26 then  -- XOR
            instructionName = "XOR"
            after = string.format("0x%08X", bit.bxor(registers.GPR.r[rs], registers.GPR.r[rt]))
            inner = string.format("%s = %s ^ %s", getRegistryName(rd), getRegistryName(rs), getRegistryName(rt))
        elseif funct == 0x27 then  -- NOR
            instructionName = "NOR"
            after = string.format("0x%08X", bit.bnot(bit.bor(registers.GPR.r[rs], registers.GPR.r[rt])))
            inner = string.format("%s = ~(%s | %s)", getRegistryName(rd), getRegistryName(rs), getRegistryName(rt))
        elseif funct == 0x00 then  -- SLL
            instructionName = "SLL"
            after = string.format("0x%08X", bit.lshift(registers.GPR.r[rt], shamt))
            inner = string.format("%s = %s << %d", getRegistryName(rd), getRegistryName(rt), shamt)
        elseif funct == 0x02 then  -- SRL
            instructionName = "SRL"
            after = string.format("0x%08X", bit.rshift(registers.GPR.r[rt], shamt))
            inner = string.format("%s = %s >> %d", getRegistryName(rd), getRegistryName(rt), shamt)
        elseif funct == 0x03 then  -- SRA
            instructionName = "SRA"
            after = string.format("0x%08X", bit.arshift(registers.GPR.r[rt], shamt))
            inner = string.format("%s = %s >> %d (arith)", getRegistryName(rd), getRegistryName(rt), shamt)
        end
    end

    PCSX.Assembler.Internals.resolveSymbol(pc)

    local text = string.format("%-11s %-8s %-50s | %s -> %s", pcHex, instructionName, inner, before, after or "N/A")
    return text
end

-- ##################################################################################
--                            Assembly reading END
-- ##################################################################################

local assemblyInstructionText = ""
local itemInFrontText = ""
local useItemInFront = false

local everyItemDropIs = 0

function DrawImguiFrame()
    if show then
        imgui.Begin("Alundra Tools", show)
        --if(imgui.Button("Decode current instruction")) then
        --    assemblyInstructionText = decodeInstruction()
        --end
        assemblyInstructionText = decodeInstruction()
        imgui.PushItemWidth(600)
        imgui.extra.InputText("##Out", assemblyInstructionText)
        imgui.PushItemWidth(0)

        if imgui.Button("Copy") then
            io.popen('clip','w'):write(string.format("%s\n", assemblyInstructionText)):close()
        end

        if imgui.Button("Set every item drop to: ") then
            local itemDropTablePtr = getPtrOf(ITEM_DROP_TABLE, "uint8_t*");
            for i = 0, 0x800 - 1 do
                itemDropTablePtr[i] = everyItemDropIs
            end
        end
        imgui.SameLine()
        imgui.PushItemWidth(30)
        local changed, value = imgui.extra.InputText(string.format("##EveryItemDropValue", id), string.format("%02X", everyItemDropIs), imgui.constant.InputTextFlags.CharsHexadecimal)
        if changed and (value == "") == false then
            everyItemDropIs = tonumber(value, 16)
        end
        imgui.PushItemWidth(0)

        GenInfoCategory()
        GenGeneralCategory()
        GenEventSystemCategory()
        GenInventoryCategory()

        imgui.End()
    end
end