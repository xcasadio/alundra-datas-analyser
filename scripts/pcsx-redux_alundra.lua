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

-- Fonction utilitaire pour lire un int32 depuis la RAM PSX
local function read_s32_le(addr)
    local ptr = ffi.cast("int32_t*", memory + (addr - 0x80000000))
    return ptr[0]
end


function DrawImguiFrame()
    imgui.Begin("Alundra Tools", true)
	
	--for i = 0, 9 do
	--	local entity_addr = base_address + i * entity_size
	--	local index = read_s32_le(entity_addr + OFFSET_INDEX)
	--	local status = read_s32_le(entity_addr + OFFSET_STATUS)
	--	local eventTrigger = read_s32_le(entity_addr + OFFSET_EVENT_TRIGGER)
	--
	--	local programIndexes = {}
	--	local spriteProgramIndexes = {}
	--
	--	for j = 0, 5 do
	--		programIndexes[j+1] = read_s32_le(entity_addr + OFFSET_PROGRAM_INDEXES + j * 4)
	--		spriteProgramIndexes[j+1] = read_s32_le(entity_addr + OFFSET_SPRITE_PROGRAM_INDEXES + j * 4)
	--	end
	--
	--	imgui.TextUnformatted(string.format(
	--		"Entity %02d | Index: %d | Status: %d | EVT: %d",
	--		i, index, status, eventTrigger
	--	))
	--	imgui.TextUnformatted("  Prog: [" .. table.concat(programIndexes, ", ") .. "]")
	--	imgui.TextUnformatted("  Spr:  [" .. table.concat(spriteProgramIndexes, ", ") .. "]")
	--	imgui.Separator()
	--end
	
	--for i = 0, 9 do
	local i = 6
		local entity_addr = base_address + i * entity_size
		local index2 = read_s32_le(entity_addr + OFFSET_INDEX2)
		local targetAnimID = read_s32_le(entity_addr + OFFSET_TARGET_ANIMATION_ID)
		local tagertDir = read_s32_le(entity_addr + OFFSET_TARGET_DIRECTION)
		local currentAnimID = read_s32_le(entity_addr + OFFSET_CURRENT_ANIMATION_ID)
		local currentDir = read_s32_le(entity_addr + OFFSET_CURRENT_DIRECTION)
		
		local Z_FORCE = read_s32_le(entity_addr + OFFSET_Z_FORCE)
		local X_FORCE = read_s32_le(entity_addr + OFFSET_X_FORCE)
		local Y_FORCE = read_s32_le(entity_addr + OFFSET_Y_FORCE)
		local TARGET_X_FORCE = read_s32_le(entity_addr + OFFSET_TARGET_X_FORCE)
		local TARGET_Y_FORCE = read_s32_le(entity_addr + OFFSET_TARGET_Y_FORCE)
		local previousAdjustedXForce = read_s32_le(entity_addr + OFFSET_previousAdjustedXForce)
		local previousAdjustedYForce = read_s32_le(entity_addr + OFFSET_previousAdjustedYForce)
		local xForceStep	           = read_s32_le(entity_addr + OFFSET_xForceStep	          )
		local yForceStep	           = read_s32_le(entity_addr + OFFSET_yForceStep	          )
		local adjustedXForce	       = read_s32_le(entity_addr + OFFSET_adjustedXForce	      )
		local adjustedYForce	       = read_s32_le(entity_addr + OFFSET_adjustedYForce	      )
		local finalXForce	           = read_s32_le(entity_addr + OFFSET_finalXForce	          )
		local finalyForce	           = read_s32_le(entity_addr + OFFSET_finalyForce	          )
		local finalZForce	           = read_s32_le(entity_addr + OFFSET_finalZForce	          )
		
		local acceleration	       = read_s32_le(entity_addr + OFFSET_acceleration	      )
		local speed	               = read_s32_le(entity_addr + OFFSET_speed	              )
	
	
		imgui.TextUnformatted(string.format(
			"Entity %02d | Index2: %d | anim: %d => %d| dir: %d => %d| acc %d| speed %d| forces %d %d %d| target: %d %d| prev: %d %d| step: %d %d",
			i, index2, currentAnimID, targetAnimID, currentDir, tagertDir, acceleration, speed, X_FORCE, Y_FORCE, Z_FORCE, TARGET_X_FORCE, TARGET_Y_FORCE, previousAdjustedXForce, previousAdjustedYForce, xForceStep, yForceStep		
		))
		
		imgui.TextUnformatted(string.format(
			"adj %d %d| final: %d %d %d",
			adjustedXForce, adjustedYForce, finalXForce, finalyForce, finalZForce			
		))
		
		imgui.Separator()
	--end
		
    imgui.End()
end
