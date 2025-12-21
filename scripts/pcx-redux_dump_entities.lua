-- Alundra Full Entity dump per frame
-- Works with french version of Alundra

PCSX  = PCSX
ffi   = ffi
imgui = imgui
json  = require("json")

local BASE_ADDR    = 0x80127D30
local ENTITY_SIZE  = 0x294
local ENTITY_COUNT = 64
local OUTPUT_DIR   = "D:/development/repo/Alundra Remake/dump"

local mem = PCSX.getMemPtr()
os.execute('mkdir "' .. OUTPUT_DIR .. '"')

local frame_no = 0
local recording = false

local function s8(addr)  return ffi.cast("int8_t*",   mem + (addr-0x80000000))[0]end
local function u8(addr)  return ffi.cast("uint8_t*",  mem + (addr-0x80000000))[0]end
local function s16(addr) return ffi.cast("int16_t*" , mem + (addr-0x80000000))[0] end
local function u32(addr) return ffi.cast("uint32_t*", mem + (addr-0x80000000))[0] end
local function s32(addr) return ffi.cast("int32_t*" , mem + (addr-0x80000000))[0] end

local function ptr(addr) return u32(addr) end
local function idx_from_ptr(p)
  if p < BASE_ADDR or p >= BASE_ADDR + ENTITY_COUNT * ENTITY_SIZE then return nil end
  local d = p - BASE_ADDR
  if d % ENTITY_SIZE ~= 0 then return nil end
  return d / ENTITY_SIZE
end

local function ptr_as_index(a)
  local p = u32(a)
  local idx = idx_from_ptr(p)
  return idx or string.format("0x%08X", p)
end

local function int_array(a,n)  local t={} for i=0,n-1 do t[i+1]=s32(a+i*4) end return t end
local function uint_array(a,n) local t={} for i=0,n-1 do t[i+1]=u32(a+i*4) end return t end
local function short_array(a,n)local t={} for i=0,n-1 do t[i+1]=s16(a+i*2) end return t end

local function sprite_ref(addr)
  return {
    images       = ptr(addr+0x00),
    x            = s32(addr+0x04),
    y            = s32(addr+0x08),
    z            = s32(addr+0x0C),
    depthSortVal = s32(addr+0x10),
    numImages    = s32(addr+0x14),
  }
end

local function event_prog_state(addr)
  local sp_ptr = ptr(addr+0x00)
  return sp_ptr ~= 0 and s32(sp_ptr) or nil
  --return {
  --  sp         = ptr(addr+0x00),
  --  parameters = ptr(addr+0x04),
  --  var1       = s32(addr+0x08),  
  --  var2       = s32(addr+0x0C),
  --  var3       = s32(addr+0x10),  
  --  var4       = s32(addr+0x14),
  --  var5       = s32(addr+0x18),  
  --  var6       = s32(addr+0x1C),
  --  var7       = s32(addr+0x20),  
  --  var8       = s32(addr+0x24),
  --  var9       = s32(addr+0x28),  
  --  result     = s32(addr+0x2C),
  --  _30        = s32(addr+0x30),
  --}
end


local function read_entity(addr)
  local E = {}

  E.index                = s32(addr+0x00)
  E.index2               = s32(addr+0x04)
  E.childEntity          = ptr(addr+0x08)
  E.parentEntity         = ptr(addr+0x0C)
  E.status               = s32(addr+0x10)
  E.hp                   = s32(addr+0x14)
  E.hpMax                = s32(addr+0x18)
  E.frameCounter      = s32(addr+0x1C)
  E.isNotProcessable     = s32(addr+0x20)
  E.flags2               = s32(addr+0x24)
  E.platformEntity       = ptr(addr+0x28)
  E.actionState          = s32(addr+0x2C)
  E.relativeWarpOffsetX  = u32(addr+0x30)
  E.relativeWarpOffsetY  = s32(addr+0x34)
  E.relativeWarpOffsetZ  = s32(addr+0x38)
  E.contentsItemId       = s32(addr+0x3C)
  E.contentsGameFlag     = s32(addr+0x40)
  E.entityRecord         = ptr(addr+0x44)
  E.entityRefId          = s32(addr+0x48)

  E.programIndexes       = int_array (addr+0x4C , 6)

  E.spriteRecord         = ptr(addr+0x64)
  E.spriteTableIndex     = s32(addr+0x68)
  E.flags                = s32(addr+0x6C)
  E.spriteProgramIndexes = int_array (addr+0x70 , 6)

  E.targetAnimationId    = u32(addr+0x88)
  E.targetDirection      = u32(addr+0x8C)
  E.currentAnimationId   = u32(addr+0x90)
  E.currentDirection     = u32(addr+0x94)  E.currentFrameIndex    = s32(addr+0x98)
  E.animSet              = ptr(addr+0x9C)
  local initialFrame = ptr(addr+0xA0)
  local frame = ptr(addr+0xA4)
  E.frameIndex           = initialFrame ~= 0 and frame ~= 0 and (frame - initialFrame) / 0x5 or -1
  E.nextFrameDelay       = s32(addr+0xA8)
  E.forceResetAnimationFlag = s32(addr+0xAC)
  E.animCompleteCounter  = s32(addr+0xB0)
  E.animFlags            = s32(addr+0xB4)

  E.zForce               = s32(addr+0xB8)
  E.targetXForce         = s32(addr+0xBC)
  E.targetYForce         = s32(addr+0xC0)
  E.xForce               = s32(addr+0xC4)
  E.yForce               = s32(addr+0xC8)
  E.previousAdjustedXForce = s32(addr+0xCC)
  E.previousAdjustedYForce = s32(addr+0xD0)
  E.xForceStep           = s32(addr+0xD4)
  E.yForceStep           = s32(addr+0xD8)
  E.adjustedXForce       = s32(addr+0xDC)
  E.adjustedYForce       = s32(addr+0xE0)
  E.finalXForce          = s32(addr+0xE4)
  E.finalYForce          = s32(addr+0xE8)
  E.finalZForce          = s32(addr+0xEC)
  E.acceleration         = s32(addr+0xF0)
  E.speed                = s32(addr+0xF4)
  E.isZForceApplied      = s32(addr+0xF8)

  E.screenClipX          = s32(addr+0xFC)
  E.screenClipY          = s32(addr+0x100)
  E.screenClipZ          = s32(addr+0x104)
  E.negXMod              = s32(addr+0x108)
  E.negYMod              = s32(addr+0x10C)
  E.negZMod              = s32(addr+0x110)

  E.xPos                 = s32(addr+0x114)
  E.yPos                 = s32(addr+0x118)
  E.zPos                 = s32(addr+0x11C)
  E.tileX                = s32(addr+0x120)
  E.tileY                = s32(addr+0x124)
  E.tileZ                = s32(addr+0x128)

  E.ridingEntity         = ptr(addr+0x12C)
  E.xCollisionEntity     = ptr(addr+0x130)
  E.floorHeight          = s32(addr+0x134)
  E.terrainHeight        = s32(addr+0x138)
  E.forceAdjusted        = s32(addr+0x13C)
  E.collidedWithEntityZ  = s32(addr+0x140)
  E.isAboveGround        = s32(addr+0x144)

  E.mapTiles             = serializeMapTiles(addr+0x148)
  E.mapHeights           = int_array(addr+0x158 , 4)

  E.platformUpdateFlag   = s32(addr+0x168)
  E._16c                 = s32(addr+0x16C)
  E.hitboxOriginX        = s32(addr+0x170)
  E.hitboxOriginY        = s32(addr+0x174)
  E.hitboxOriginZ        = s32(addr+0x178)
  E._17c                 = s32(addr+0x17C)
  E.combinedVramFlagsOR  = s32(addr+0x180)
  E.combinedVramFlagsAND = s32(addr+0x184)
  E.tileAttributes       = s32(addr+0x188)
  E.slope_18c            = s32(addr+0x18C)
  E.slope_190            = s32(addr+0x190)

  E.spriteRef            = sprite_ref(addr+0x194)

  E.spriteImageIndex     = s32(addr+0x1AC)
  E.paletteIndex         = s32(addr+0x1B0)
  E.sheetSize            = s32(addr+0x1B4)
  E.activeEffect         = ptr(addr+0x1B8)
  E.zSortValue           = s32(addr+0x1BC)
  E.zSortDepth           = s32(addr+0x1C0)
  E.balanceRecord        = ptr(addr+0x1C4)
  E.balanceAnimValRef    = ptr(addr+0x1C8)
  E.damagedTickCounter   = s32(addr+0x1CC)
  E.frameColTickCounter  = s32(addr+0x1D0)
  E.frameCollisionData   = ptr(addr+0x1D4)
  E.moddedXPos           = s32(addr+0x1D8)
  E.moddedYPos           = s32(addr+0x1DC)
  E.moddedZPos           = s32(addr+0x1E0)
  E.xMod                 = s32(addr+0x1E4)
  E.yMod                 = s32(addr+0x1E8)
  E.zMod                 = s32(addr+0x1EC)
  E.width                = s32(addr+0x1F0)
  E.height               = s32(addr+0x1F4)
  E.depth                = s32(addr+0x1F8)
  E.hitBoxX              = s32(addr+0x1FC)
  E.hitBoxY              = s32(addr+0x200)
  E.hitBoxZ              = s32(addr+0x204)
  E.transformX           = s32(addr+0x208)
  E.transformY           = s32(addr+0x20C)
  E.transformZ           = s32(addr+0x210)
  E.transformWidth       = u32(addr+0x214)
  E.transformDepth       = u32(addr+0x218)
  E.transformHeight      = u32(addr+0x21C)
  E.hitCounter           = s32(addr+0x220)
  E.touchingEntity       = ptr(addr+0x224)
  E.eventTrigger         = s32(addr+0x228)
  E.mapEventProgramId    = s32(addr+0x22C)
  E.logicContextEntity   = ptr(addr+0x230)

  E.eventProgramState    = event_prog_state(addr+0x234)

  E._268                 = string.format("0x%02X", ffi.cast("uint8_t*", mem + (addr-0x80000000))[0x268])
  E._269                 = string.format("0x%02X", ffi.cast("uint8_t*", mem + (addr-0x80000000))[0x269])
  E._26A                 = string.format("0x%02X", ffi.cast("uint8_t*", mem + (addr-0x80000000))[0x26A])
  E._26B                 = string.format("0x%02X", ffi.cast("uint8_t*", mem + (addr-0x80000000))[0x26B])

  E.lastTargetAnimationId = s32(addr+0x26C)
  E.lastTargetDirection   = s32(addr+0x270)
  E.bytes                 = {
                               string.format("0x%02X", ffi.cast("uint8_t*", mem + (addr-0x80000000))[0x274]),
                               string.format("0x%02X", ffi.cast("uint8_t*", mem + (addr-0x80000000))[0x275]),
                               string.format("0x%02X", ffi.cast("uint8_t*", mem + (addr-0x80000000))[0x276]),
                               string.format("0x%02X", ffi.cast("uint8_t*", mem + (addr-0x80000000))[0x277]) }

  E.initialXPos          = s32(addr+0x278)
  E.initialYPos          = s32(addr+0x27C)

  E.aiValues             = short_array(addr+0x280, 10)

  return E
end

function serializeMapTiles(addr)
    local serializedMapTiles = {}
    for i = 1, 4 do
        local mapTileAddr = addr + (i - 1) * 0x8
        local mapTile = {
            walkability = u8(mapTileAddr),
            groundProperty = u8(mapTileAddr + 0x1),
            slope = u8(mapTileAddr + 0x2),
            height = u8(mapTileAddr + 0x3),
            tileId = s16(mapTileAddr + 0x4),
            tilesOffset = s16(mapTileAddr + 0x6)
        }
        table.insert(serializedMapTiles, mapTile)
    end
    return serializedMapTiles
end

-- {name, addr, kind, count}
local G = {
  {"g_mapFlags",                0x801EB344, "u32arr", 1024},
  {"g_globalFlags",             0x801EBA40, "u32arr", 1024},
  {"g_gameRandomSeed",          0x80098708, "u32"},
  {"g_lastWarpEntityIndex",     0x800986F0, "s32"},
  {"g_TileAnimFrameCounter",    0x800986FC, "s32"},
  {"DAT_80098f24",              0x80098F24, "s32"},
  {"g_soundFadeTimer",          0x800A825E, "s16"},
  {"INT_ARRAY_800a8284",        0x800A8284, "s32arr", 10},
  {"g_globalTransitionState",   0x800C4980, "s32"},
  {"g_defaultWarpDestinations", 0x800C659C, "u32arr", 483},
  {"g_soundGroupByMapId",       0x800C6D28, "u32arr", 483},
  {"g_orderingTableBuffer",     0x800CC050, "s32arr", 4},
  {"g_warpDelayFrames",         0x800DC4B4, "s32"},
  {"g_playerControlFlags",      0x800DC4B8, "s32"},
  {"g_isWarpDisabled",          0x800DC4C0, "s32"},
  {"g_isGameEnding",            0x800DC4C4, "s32"},
  {"g_mapTransitionEffectId",   0x800DC4C8, "s32"},
  {"g_desiredMap",              0x800DC4CC, "s32"},
  {"g_warpTriggerType",         0x800DC4D0, "s32"},
  {"g_warpExtraParam",          0x800DC4D4, "s32"},
  {"g_cameraTargetX",           0x800DC4D8, "s32"},
  {"g_cameraTargetY",           0x800DC4DC, "s32"},
  {"g_cameraTargetZ",           0x800DC4E0, "s32"},
  {"g_currentMap",              0x800DC5A0, "s32"},
  {"g_isCameraScrolling",       0x800E42B8, "s32"},
  {"g_cameraScrollingX",        0x800E4328, "s32"},
  {"g_cameraScrollingY",        0x800E432C, "s32"},
  {"g_bossCutsceneFlag",        0x800E4338, "s32"},
  {"g_cutsceneScrollLimitX",    0x800E433C, "s32"},
  {"g_cutsceneScrollLimitY",    0x800E4340, "s32"},
  {"g_cutsceneScrollSpeedX",    0x800E4344, "s32"},
  {"g_cutsceneScrollSpeedY",    0x800E4348, "s32"},
  {"g_cameraOffsetX",           0x800E434C, "s32"},
  {"g_cameraOffsetY",           0x800E4350, "s32"},
  {"g_cutsceneXReachedMin",     0x800E4354, "s32"},
  {"g_cutsceneYReachedMin",     0x800E4358, "s32"},
  {"g_padState1",               0x80126F18, "padstate"},
  {"g_gravityFlag",             0x80127000, "s32"},
  {"g_activeCollisionEntity",   0x80127108, "eptr"},
  {"g_warpLockTimer",           0x80127164, "s32"},
  {"g_activeEntities",          0x80127B28, "eptrarr", 64},
  {"g_collideableEntities",     0x80127C28, "eptrarr", 64},
  {"g_activeEntityCount",       0x80127D28, "s32"},
  {"g_collideableEntitiesCount",0x80127D2C, "s32"},
  {"g_visibleEntities",         0x80134250, "eptrarr", 64},
  {"g_cameraLookAtX",           0x80134350, "s32"},
  {"g_cameraLookAtY",           0x80134354, "s32"},
  {"g_cameraLookAtZ",           0x80134358, "s32"},
  {"g_visibleEntityCount",      0x8013435C, "s32"},
  {"g_numberOfEntity",          0x80134360, "s32"},
  {"g_entityFollowedByCamera",  0x801345fc, "eptr"},
  {"g_nextEntityIndex",         0x80134600, "s32"},
  {"g_mapOffsetX",              0x8013FB68, "s32"},
  {"g_mapOffsetY",              0x8013FB6C, "s32"},
  {"g_mapScreenPosX",           0x8013FB70, "s32"},
  {"g_mapScreenPosY",           0x8013FB74, "s32"},
  {"g_warpFlags",               0x8013FBB8, "s32"},
  {"g_playerLastX",             0x8013FBBC, "s32"},
  {"g_playerLastY",             0x8013FBC0, "s32"},
  {"g_playerLastZ",             0x8013FBC4, "s32"},
  {"g_hudDeltaX",               0x801800D8, "s32"},
  {"g_hudDeltaY",               0x801800DC, "s32"},
  {"g_hudCurrentX",             0x801800E0, "s32"},
  {"g_hudCurrentY",             0x801800E4, "s32"},
  {"g_hudX",                    0x801800E8, "s32"},
  {"g_hudY",                    0x801800EC, "s32"},
  {"g_savedGameplayTime",       0x801EB330, "u32"},
  {"g_initialWarpMap",          0x801EB334, "s32"},
  {"g_initialWarpTileX",        0x801EB338, "s32"},
  {"g_initialWarpTileY",        0x801EB33C, "s32"},
  {"g_initialWarpZ",            0x801EB340, "s32"},
  {"g_systemFlags",             0x801EB410, "s32"},
}

local function read_global(g)
  local name, addr, kind, count = table.unpack(g)
  if kind == "s32"  then return s32(addr)
  elseif kind == "u32" then return u32(addr)
  elseif kind == "s16" then return s16(addr)
  elseif kind == "u32arr" then return uint_array(addr, count)
  elseif kind == "s32arr" then return int_array(addr, count)
  elseif kind == "padstate" then
    return {
      maxNbFrameHeld = u32(addr + 0x0),
      repeatInterval = u32(addr + 0x4),
      isOverThanMaxNbFrameHeld = u32(addr + 0x8),
      numberOfFrameHold = u32(addr + 0xC),
      buttonsHold = s16(addr + 0x10),
      buttonsJustPressed = s16(addr + 0x12),
      buttonReleased = s16(addr + 0x14),
      buttonsJustPressedByInterval = s16(addr + 0x16)
    }
  elseif kind == "eptr" then
    local p = u32(addr)
    if p == 0 then
      return -1
    else
      local idx = idx_from_ptr(p)
      return idx or string.format("0x%08X", p)
    end
  elseif kind == "eptrarr" then
    local result = {}
    for i = 0, count - 1 do
      local p = u32(addr + i * 4)
      if p == 0 then
        result[i+1] = -1
      else
        local idx = idx_from_ptr(p)
        result[i+1] = idx or string.format("0x%08X", p)
      end
    end
    return result
  end
end

local function clear_output_dir()
  -- Windows : rmdir /S /Q ; Linux/mac : rm -rf
  if package.config:sub(1,1) == '\\' then
    os.execute('rmdir /S /Q "' .. OUTPUT_DIR .. '"')
  else
    os.execute('rm -rf "' .. OUTPUT_DIR .. '"')
  end
  os.execute('mkdir "' .. OUTPUT_DIR .. '"')
end

function DrawImguiFrame()

  imgui.Begin("Alundra – Entity Saver", true)

    if imgui.Button("Start recording") then
      clear_output_dir()
      frame_no  = 0
      recording = true
    end
    imgui.SameLine()
    
    if imgui.Button("Stop recording") then
      recording = false
    end
    imgui.SameLine()
    
    imgui.TextUnformatted(recording
        and string.format("Recording… (frame %d)", frame_no)
        or  "Paused")
		

	imgui.TextUnformatted(string.format("Output directory %s", OUTPUT_DIR))

  imgui.End()
    if not recording then return end
  
  frame_no = frame_no + 1
  local dump  = { entities = {} }
  
  -- entities
  for i = 0, ENTITY_COUNT - 1 do
    dump.entities[i+1] = read_entity(BASE_ADDR + i * ENTITY_SIZE)
  end

  -- globals
  for _,g in ipairs(G) do
    dump[g[1]] = read_global(g)
  end

  local filepath = string.format("%s/alundra_frame_%06d.json", OUTPUT_DIR, frame_no)
  local f = io.open(filepath, "w")
  if f then
    f:write(json.encode(dump))
    f:close()
  end
end
