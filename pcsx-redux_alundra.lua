PCSX = PCSX
bit = bit
ffi = ffi
imgui = imgui

local memory = PCSX.getMemPtr()
local base_address = 0x80127D30
local entity_size = 0x294

-- Offsets dans la structure
local OFFSET_INDEX = 0x00
local OFFSET_STATUS = 0x10
local OFFSET_EVENT_TRIGGER = 0x228
local OFFSET_PROGRAM_INDEXES = 0x4c
local OFFSET_SPRITE_PROGRAM_INDEXES = 0x70

-- Fonction utilitaire pour lire un int32 depuis la RAM PSX
local function read_s32_le(addr)
    local ptr = ffi.cast("int32_t*", memory + (addr - 0x80000000))
    return ptr[0]
end


function DrawImguiFrame()
    imgui.Begin("Alundra Tools", true)
	
	for i = 0, 9 do
		local entity_addr = base_address + i * entity_size
		local index = read_s32_le(entity_addr + OFFSET_INDEX)
		local status = read_s32_le(entity_addr + OFFSET_STATUS)
		local eventTrigger = read_s32_le(entity_addr + OFFSET_EVENT_TRIGGER)
	
		local programIndexes = {}
		local spriteProgramIndexes = {}
	
		for j = 0, 5 do
			programIndexes[j+1] = read_s32_le(entity_addr + OFFSET_PROGRAM_INDEXES + j * 4)
			spriteProgramIndexes[j+1] = read_s32_le(entity_addr + OFFSET_SPRITE_PROGRAM_INDEXES + j * 4)
		end
	
		imgui.TextUnformatted(string.format(
			"Entity %02d | Index: %d | Status: %d | EVT: %d",
			i, index, status, eventTrigger
		))
		imgui.TextUnformatted("  Prog: [" .. table.concat(programIndexes, ", ") .. "]")
		imgui.TextUnformatted("  Spr:  [" .. table.concat(spriteProgramIndexes, ", ") .. "]")
		imgui.Separator()
	end
		
    imgui.End()
end
