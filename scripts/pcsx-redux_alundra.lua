PCSX = PCSX
bit = bit
ffi = ffi
imgui = imgui

local memory = PCSX.getMemPtr()
local base_address = 0x80127D30
local entity_size = 0x294

-- Offsets dans la structure
local OFFSET_INDEX = 0x00
local OFFSET_INDEX2 = 0x04
local OFFSET_STATUS = 0x10
local OFFSET_TARGET_ANIMATION_ID = 0x88
local OFFSET_TARGET_DIRECTION = 0x8c
local OFFSET_CURRENT_ANIMATION_ID = 0x90
local OFFSET_CURRENT_DIRECTION = 0x94

local OFFSET_Z_FORCE = 0xb8
local OFFSET_X_FORCE = 0xc4
local OFFSET_Y_FORCE = 0xc8
local OFFSET_TARGET_X_FORCE = 0xbc
local OFFSET_TARGET_Y_FORCE = 0xc0
local OFFSET_previousAdjustedXForce	= 0xcc
local OFFSET_previousAdjustedYForce	= 0xd0
local OFFSET_xForceStep	            = 0xd4
local OFFSET_yForceStep	            = 0xd8
local OFFSET_adjustedXForce	        = 0xdc
local OFFSET_adjustedYForce	        = 0xe0
local OFFSET_finalXForce	            = 0xe4
local OFFSET_finalyForce	            = 0xe8
local OFFSET_finalZForce	            = 0xec
local OFFSET_acceleration	        	= 0xf0
local OFFSET_speed	                	= 0xf4


local OFFSET_EVENT_TRIGGER = 0x228
local OFFSET_PROGRAM_INDEXES = 0x4c
local OFFSET_SPRITE_PROGRAM_INDEXES = 0x70

local OFFSET_SPRITE_PROGRAM_INDEXES = 0x70

local function read_s32_le(addr)
    local ptr = ffi.cast("int32_t*", memory + (addr - 0x80000000))
    return ptr[0]
end

local function write_byte(addr, value)
    local ptr = ffi.cast("uint8_t*", memory + (addr - 0x80000000))
    ptr[0] = value
end

-- Variables pour la combobox
local items = {
    {value = 36, name = "36-Herbs"},
    {value = 37, name = "37-Strength Elixyr"},
    {value = 41, name = "41-System error (unused)"},
    {value = 69, name = "69-"},
    {value = 70, name = "70-"},
    {value = 71, name = "71-"},
    {value = 72, name = "72-"},
    {value = 79, name = "79-Gilded Falcon"},
    {value = 80, name = "80-Magic Seed"},
    {value = 81, name = "81-Small Crystal"},
    {value = 82, name = "82-Large Crystal"},
    {value = 83, name = "83-Life Vessel"},
    {value = 84, name = "84-Dew of Life"},
    {value = 85, name = "85-Drop of Life"},
    {value = 86, name = "86-Water of Life"}
}

local selected_item_index = 1
local inventory_address = 0x80028e0c

function DrawImguiFrame()
    if imgui.Begin("Alundra Tools") then
        
        -- Combobox pour sélectionner un élément
        if imgui.BeginCombo("Change random item", items[selected_item_index].name) then
            for i, item in ipairs(items) do
                local is_selected = (selected_item_index == i)
                if imgui.Selectable(item.name, is_selected) then
                    selected_item_index = i
                end
                if is_selected then
                    imgui.SetItemDefaultFocus()
                end
            end
            imgui.EndCombo()
        end
        
        imgui.Spacing()
        
        if imgui.Button("force probablity to 100%") then
            local selected_value = items[selected_item_index].value
            for i = 0, 99 do
                write_byte(inventory_address + i, selected_value)
            end
        end
        
    end
    imgui.End()
end
