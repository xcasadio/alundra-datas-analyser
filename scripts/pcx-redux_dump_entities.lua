-----------------------------------------------------------------
--  Alundra – Full Entity dump per frame (PCSX-Redux + LuaJIT) --
-----------------------------------------------------------------

PCSX  = PCSX
ffi   = ffi
imgui = imgui
json  = require("json")

local BASE_ADDR    = 0x80127D30   -- french version of Alundra
local ENTITY_SIZE  = 0x294        -- 660 octets
local ENTITY_COUNT = 17
local OUTPUT_DIR   = "D:/development/repo/Alundra Remake/dump"

local mem = PCSX.getMemPtr()
os.execute('mkdir "' .. OUTPUT_DIR .. '"')
local frame_no = 0


local function u32(addr) return ffi.cast("uint32_t*", mem + (addr-0x80000000))[0] end
local function s32(addr) return ffi.cast("int32_t*" , mem + (addr-0x80000000))[0] end
local function s16(addr) return ffi.cast("int16_t*" , mem + (addr-0x80000000))[0] end
local function ptr(addr) return u32(addr) end

local function int_array(addr, n)
  local t = {}
  for i = 0, n-1 do t[i+1] = s32(addr + i*4) end
  return t
end

local function short_array(addr, n)
  local t = {}
  for i = 0, n-1 do t[i+1] = s16(addr + i*2) end
  return t
end

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
  return {
    sp         = ptr(addr+0x00),
    parameters = ptr(addr+0x04),
    var1       = s32(addr+0x08),  var2 = s32(addr+0x0C),
    var3       = s32(addr+0x10),  var4 = s32(addr+0x14),
    var5       = s32(addr+0x18),  var6 = s32(addr+0x1C),
    var7       = s32(addr+0x20),  var8 = s32(addr+0x24),
    var9       = s32(addr+0x28),  result = s32(addr+0x2C),
    _30        = s32(addr+0x30),
  }
end


local function read_entity(addr)
  local E = {}

  -- 0x00 – 0x48
  E.index                = s32(addr+0x00)
  E.index2               = s32(addr+0x04)
  E.childEntity          = ptr(addr+0x08)
  E.parentEntity         = ptr(addr+0x0C)
  E.status               = s32(addr+0x10)
  E.hp                   = s32(addr+0x14)
  E.hpMax                = s32(addr+0x18)
  E.hitFrameCounter      = s32(addr+0x1C)
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
  E.currentDirection     = u32(addr+0x94)
  E.currentFrameIndex    = s32(addr+0x98)
  E.animSet              = ptr(addr+0x9C)
  E.initialFrame         = ptr(addr+0xA0)
  E.frame                = ptr(addr+0xA4)
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

  E.mapTiles             = int_array(addr+0x148 , 4)
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

---------------------------------------------------------------
-- Hook principal : appelé par PCSX-Redux chaque frame
---------------------------------------------------------------
function DrawImguiFrame()
  frame_no = frame_no + 1

  local out = {}
  for i = 0, ENTITY_COUNT - 1 do
    out[i+1] = read_entity(BASE_ADDR + i * ENTITY_SIZE)
  end

  local filepath = string.format("%s/alundra_frame_%06d.json", OUTPUT_DIR, frame_no)
  local f = io.open(filepath, "w")
  if f then
    f:write(json.encode(out))
    f:close()
  end

  if imgui.Begin("Alundra – Entity Saver", true) then
    imgui.TextUnformatted(string.format("Frame %6d | %s", frame_no, filepath))
  end
  imgui.End()
end
