-- pcsx-redux_apply_savestate.lua
-- Applique les données d'un savestate C# (JSON) aux variables du jeu original dans PCSX-Redux.
-- g_currentMap n'est PAS modifié ; g_desiredMap est défini à InitialMapId.
-- Prérequis : json.lua doit être dans le même dossier de scripts PCSX-Redux.

PCSX = PCSX
bit  = bit
ffi  = ffi
imgui = imgui

local memory = PCSX.getMemPtr()

-- ============================================================
-- Primitives d'écriture en mémoire PSX
-- ============================================================

local function psx_ptr(addr, ctype)
    return ffi.cast(ctype .. "*", memory + (addr - 0x80000000))
end

local function write_u8(addr, v)
    psx_ptr(addr, "uint8_t")[0] = v
end

local function write_s16(addr, v)
    psx_ptr(addr, "int16_t")[0] = v
end

local function write_u16(addr, v)
    psx_ptr(addr, "uint16_t")[0] = v
end

local function write_s32(addr, v)
    psx_ptr(addr, "int32_t")[0] = v
end

local function write_u32(addr, v)
    psx_ptr(addr, "uint32_t")[0] = v
end

local function write_str(addr, s, maxlen)
    local ptr = psx_ptr(addr, "uint8_t")
    for i = 0, maxlen - 1 do
        local c = (i < #s) and string.byte(s, i + 1) or 0
        ptr[i] = c
    end
end

-- ============================================================
-- Adresses PSX (issues de StaticVariables.cs)
-- ============================================================

-- Globals de navigation de carte
local G_IS_GAME_ENDING        = 0x800DC4C4  -- int32  : 1 = déclenche le warp
local G_MAP_TRANSITION_EFFECT = 0x800DC4C8  -- int32  : 0 = fondu standard
local G_DESIRED_MAP           = 0x800DC4CC  -- uint32 : carte cible (à écrire)
-- G_CURRENT_MAP              = 0x800DC5A0  -- uint32 : carte active (NE PAS TOUCHER)
local G_RESET_ANIMATION_ID    = 0x800DC4D0  -- int32  : 0x36 = idle
local G_RESET_DIRECTION_ID    = 0x800DC4D4  -- int32  : 0 = défaut
local G_WARP_SOUND_EFFECT_ID  = 0x800DC4E8  -- uint32 : 0 = sans son

-- Globals de caméra
local G_CAMERA_TARGET_X = 0x800DC4D8  -- int32
local G_CAMERA_TARGET_Y = 0x800DC4DC  -- int32
local G_CAMERA_TARGET_Z = 0x800DC4E0  -- int32
local G_CAMERA_LOOKAT_X = 0x80134350  -- int32
local G_CAMERA_LOOKAT_Y = 0x80134354  -- int32
local G_CAMERA_LOOKAT_Z = 0x80134358  -- int32

-- Temps de jeu
local G_GAMEPLAY_TIME   = 0x8013FB4C  -- uint32

-- Stats joueur en cours d'exécution (structure PlayerStats identique)
local G_PLAYER_STATS    = 0x80176318  -- int16[9]

-- Structure g_saveData (copie de travail utilisée par le gameplay)
local G_SAVE_DATA       = 0x801EB2E8

-- ============================================================
-- Offsets dans g_saveData (en octets depuis G_SAVE_DATA)
--   Taille totale : 0x758 octets (SlotData..Field_757 inclus)
-- ============================================================
local OFF_SLOT_DATA         = 0x000   -- int32
local OFF_LAST_MAP_ID       = 0x004   -- uint32
local OFF_CURRENT_FLAG_NAME = 0x008   -- char[32]
local OFF_GAME_STATE_DESC   = 0x028   -- char[32]
local OFF_GAME_TIME         = 0x048   -- uint32
local OFF_INITIAL_MAP_ID    = 0x04C   -- uint32
local OFF_CAMERA_TILE_X     = 0x050   -- int32
local OFF_CAMERA_TILE_Y     = 0x054   -- int32
local OFF_CAMERA_TILE_Z     = 0x058   -- int32
local OFF_GAME_FLAGS        = 0x05C   -- uint32[64] = 256 octets
local OFF_MAP_ID_TABLE      = 0x15C   -- ushort[500] = 1000 octets
local OFF_PLAYER_STATS      = 0x544   -- int16[9] = 18 octets
local OFF_NUMBER_OF_ITEMS   = 0x556   -- int16[256] = 512 octets
local OFF_SAVE_SLOT_INDEX   = 0x756   -- uint8
local OFF_FIELD_757         = 0x757   -- uint8

-- Dimensions d'une tuile (MapTileWidth=24, MapTileHeight=16)
local TILE_W = 24
local TILE_H = 16

-- ============================================================
-- Application du savestate
-- ============================================================

local function apply_savestate(path, status_out)
    -- Chargement du fichier JSON (avant pause)
    local f = io.open(path, "r")
    if not f then
        status_out[1] = "ERREUR : impossible d'ouvrir " .. path
        return false
    end
    local content = f:read("*all")
    f:close()

    local json_ok, json = pcall(require, "json")
    if not json_ok then
        status_out[1] = "ERREUR : json.lua introuvable : " .. tostring(json)
        return false
    end

    local save, err = json.decode(content)
    if not save then
        status_out[1] = "ERREUR JSON : " .. tostring(err)
        return false
    end

    -- --------------------------------------------------------
    -- g_saveData @ 0x801EB2E8
    -- --------------------------------------------------------
    write_s32(G_SAVE_DATA + OFF_SLOT_DATA,        save.SlotData or 1)
    write_u32(G_SAVE_DATA + OFF_LAST_MAP_ID,       save.LastMapId or 0xFFFFFFFF)
    write_str(G_SAVE_DATA + OFF_CURRENT_FLAG_NAME, save.CurrentFlagName or "", 32)
    write_str(G_SAVE_DATA + OFF_GAME_STATE_DESC,   save.GameStateDescription or "", 32)
    write_u32(G_SAVE_DATA + OFF_GAME_TIME,         save.GameTime or 0)
    write_u32(G_SAVE_DATA + OFF_INITIAL_MAP_ID,    save.InitialMapId or 0)
    write_s32(G_SAVE_DATA + OFF_CAMERA_TILE_X,     save.CameraTileX or 0)
    write_s32(G_SAVE_DATA + OFF_CAMERA_TILE_Y,     save.CameraTileY or 0)
    write_s32(G_SAVE_DATA + OFF_CAMERA_TILE_Z,     save.CameraTileZ or 0)

    if save.GameFlags then
        for i = 0, 63 do
            write_u32(G_SAVE_DATA + OFF_GAME_FLAGS + i * 4, save.GameFlags[i + 1] or 0)
        end
    end

    if save.MapIdToInternalMapIndexTable then
        for i = 0, 499 do
            write_u16(G_SAVE_DATA + OFF_MAP_ID_TABLE + i * 2,
                      save.MapIdToInternalMapIndexTable[i + 1] or 0)
        end
    end

    if save.PlayerStats then
        local ps   = save.PlayerStats
        local base = G_SAVE_DATA + OFF_PLAYER_STATS
        write_s16(base + 0x00, ps.Hp          or 0)
        write_s16(base + 0x02, ps.HpMax       or 0)
        write_s16(base + 0x04, ps.Mp          or 0)
        write_s16(base + 0x06, ps.MpMax       or 0)
        write_s16(base + 0x08, ps.MoneyAmount or 0)
        write_s16(base + 0x0A, ps.WeaponId    or 0)
        write_s16(base + 0x0C, ps.ItemId      or 0)
        write_s16(base + 0x0E, ps.FalconTemp  or 0)
        write_s16(base + 0x10, ps.Falcon      or 0)
        -- g_playerStats @ 0x80176318 est un POINTEUR vers la struct live.
        -- On le déréférence pour écrire dans la vraie struct.
        local ptr_val = tonumber(ffi.cast("uint32_t*", memory + (G_PLAYER_STATS - 0x80000000))[0])
        local phys    = bit.band(ptr_val, 0x1FFFFFFF)
        if phys > 0 and phys < 0x200000 then
            local live = 0x80000000 + phys
            write_s16(live + 0x00, ps.Hp          or 0)
            write_s16(live + 0x02, ps.HpMax       or 0)
            write_s16(live + 0x04, ps.Mp          or 0)
            write_s16(live + 0x06, ps.MpMax       or 0)
            write_s16(live + 0x08, ps.MoneyAmount or 0)
            write_s16(live + 0x0A, ps.WeaponId    or 0)
            write_s16(live + 0x0C, ps.ItemId      or 0)
            write_s16(live + 0x0E, ps.FalconTemp  or 0)
            write_s16(live + 0x10, ps.Falcon      or 0)
        else
            print(string.format("[SaveState] WARN g_playerStats ptr invalide: 0x%08X", ptr_val))
        end
    end

    if save.NumberOfItems then
        for i = 0, 255 do
            write_s16(G_SAVE_DATA + OFF_NUMBER_OF_ITEMS + i * 2,
                      save.NumberOfItems[i + 1] or 0)
        end
    end

    write_u8(G_SAVE_DATA + OFF_SAVE_SLOT_INDEX, save.SaveSlotIndex or 1)
    write_u8(G_SAVE_DATA + OFF_FIELD_757,        save.Field_757    or 0)

    -- --------------------------------------------------------
    -- Caméra (InitializeMapWarpPosition @ 0x800315B0)
    -- --------------------------------------------------------
    local tx    = save.CameraTileX or 0
    local ty    = save.CameraTileY or 0
    local tz    = save.CameraTileZ or 0
    local cam_x = (tx * TILE_W + TILE_W / 2) * 65536
    local cam_y = (ty * TILE_H + TILE_H / 2) * 65536
    local cam_z = tz * 1048576  -- << 20

    write_s32(G_CAMERA_TARGET_X, cam_x)
    write_s32(G_CAMERA_TARGET_Y, cam_y)
    write_s32(G_CAMERA_TARGET_Z, cam_z)
    write_s32(G_CAMERA_LOOKAT_X, cam_x)
    write_s32(G_CAMERA_LOOKAT_Y, cam_y)
    write_s32(G_CAMERA_LOOKAT_Z, cam_z)
    write_u32(G_GAMEPLAY_TIME, save.GameTime or 0)

    -- --------------------------------------------------------
    -- Déclenchement du warp vers la map sauvegardée
    -- g_currentMap sera mis à jour par le jeu via son mécanisme warp normal.
    -- g_isGameEnding DOIT être écrit EN DERNIER.
    -- --------------------------------------------------------
    local map_id = save.InitialMapId or 0
    write_u32(G_DESIRED_MAP,           map_id)
    write_s32(G_MAP_TRANSITION_EFFECT, 0)      -- fondu standard
    write_s32(G_RESET_ANIMATION_ID,    0x36)   -- animation idle
    write_s32(G_RESET_DIRECTION_ID,    0)      -- direction par défaut
    write_u32(G_WARP_SOUND_EFFECT_ID,  0)      -- sans son
    write_s32(G_IS_GAME_ENDING,        1)      -- déclenche le warp (EN DERNIER)

    local fname = path:match("[^\\/]+$") or path
    status_out[1] = string.format("OK carte=%d tuile=(%d,%d) %s", map_id, tx, ty, fname)
    return true
end


-- ============================================================
-- Listage des fichiers .json dans un dossier
-- ============================================================

local function list_json_files(dir)
    local files = {}
    -- Utilise PCSX.listDirectory si disponible, sinon io.popen
    if PCSX.listDirectory then
        for _, entry in ipairs(PCSX.listDirectory(dir)) do
            if entry:match("%.json$") then
                files[#files + 1] = entry
            end
        end
    else
        -- Garde contre io nil (sandbox PCSX-Redux peut restreindre io)
        local popen_fn = type(io) == "table" and io.popen
        if popen_fn then
            local ok, pipe = pcall(popen_fn, 'dir /b "' .. dir .. '\\*.json" 2>nul')
            if ok and pipe then
                for line in pipe:lines() do
                    if line ~= "" then
                        files[#files + 1] = line
                    end
                end
                pipe:close()
            end
        end
    end
    table.sort(files)
    return files
end

-- ============================================================
-- État de l'interface ImGui
-- ============================================================

local DEFAULT_DIR = "D:\\development\\repo\\alundra-datas-analyser\\AlundraTools\\AlundraTools\\bin\\Debug\\net9.0-windows7.0\\SaveStates"
local savestate_dir   = DEFAULT_DIR
local savestate_files = {}
local selected_index  = 1
local status_box      = { "Cliquer Scan pour lister les fichiers" }

-- ============================================================
-- Rendu ImGui  (style pcx-redux_dump_entities.lua)
-- ============================================================

local _frame_count = 0

function DrawImguiFrame()
    _frame_count = _frame_count + 1

    -- Force la position au premier frame pour qu'elle soit visible
    if _frame_count == 1 then
        imgui.SetNextWindowPos(30, 30)
        imgui.SetNextWindowSize(350, 130)
    end

    local is_open = imgui.Begin("Alundra SaveState")

    if _frame_count <= 2 then
        print(string.format("[SaveState] frame #%d  Begin=%s", _frame_count, tostring(is_open)))
    end

    if is_open then

        if imgui.Button("Scan##scan") then
            savestate_files = list_json_files(savestate_dir)
            selected_index = 1
            print(string.format("[SaveState] %d fichier(s) dans: %s", #savestate_files, savestate_dir))
        end

        imgui.Spacing()

        local display_name = (selected_index >= 1 and selected_index <= #savestate_files)
            and savestate_files[selected_index] or "(aucun)"

        if imgui.BeginCombo("Fichier JSON##combo", display_name) then
            for i, fname in ipairs(savestate_files) do
                local is_sel = (i == selected_index)
                if imgui.Selectable(fname, is_sel) then
                    selected_index = i
                end
                if is_sel then
                    imgui.SetItemDefaultFocus()
                end
            end
            imgui.EndCombo()
        end

        imgui.Spacing()

        if imgui.Button("Appliquer##apply") then
            if selected_index >= 1 and selected_index <= #savestate_files then
                local full_path = savestate_dir .. "\\" .. savestate_files[selected_index]
                apply_savestate(full_path, status_box)
                print("[SaveState] " .. status_box[1])
            else
                print("[SaveState] Aucun fichier selectionne")
            end
        end

    end
    imgui.End()
end
