param(
    [Parameter(Mandatory = $true)]
    [string]$DataPath,

    [string]$MapName = "map_0"
)

$ErrorActionPreference = "Stop"

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Read-JsonFile {
    param([string]$Path)

    Assert-True (Test-Path -LiteralPath $Path) "Missing file: $Path"
    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Get-TiledPropertyMap {
    param($Properties)

    $map = @{}
    if ($null -eq $Properties) {
        return $map
    }

    foreach ($property in @($Properties)) {
        $map[$property.name] = $property.value
    }

    return $map
}

function Assert-TiledProperties {
    param(
        $Properties,
        [string[]]$Names,
        [string]$Context
    )

    $map = Get-TiledPropertyMap $Properties
    foreach ($name in $Names) {
        Assert-True ($map.ContainsKey($name)) "$Context is missing property '$name'"
    }

    return $map
}

function Get-LayerByName {
    param(
        $Map,
        [string]$Name
    )

    $layers = @($Map.layers | Where-Object { $_.name -eq $Name })
    Assert-True ($layers.Count -eq 1) "Expected exactly one layer named '$Name', found $($layers.Count)"
    return $layers[0]
}

function Assert-ObjectLayer {
    param(
        $Map,
        [string]$Name,
        [string[]]$RequiredObjectProperties
    )

    $layer = Get-LayerByName $Map $Name
    Assert-True ($layer.type -eq "objectgroup") "Layer '$Name' must be an objectgroup"

    foreach ($object in @($layer.objects)) {
        Assert-TiledProperties $object.properties $RequiredObjectProperties "Object '$($object.name)' in layer '$Name'" | Out-Null
    }

    return $layer
}

$tiledPath = Join-Path $DataPath "tiled"
$tmjPath = Join-Path $tiledPath "$MapName.tmj"
$tilesetPath = Join-Path $tiledPath "${MapName}_tileset.tsj"
$companionPath = Join-Path $tiledPath "$MapName.alundra.json"

$map = Read-JsonFile $tmjPath
$tileset = Read-JsonFile $tilesetPath
$companion = Read-JsonFile $companionPath

Assert-True ([int]$map.width -gt 0) "Map width must be positive"
Assert-True ([int]$map.height -gt 0) "Map height must be positive"
Assert-True ([int]$map.tilewidth -eq 24) "Map tile width must be 24"
Assert-True ([int]$map.tileheight -eq 16) "Map tile height must be 16"

$mapProperties = Assert-TiledProperties $map.properties @("AlundraCompanionJson", "MapId", "Gravity", "ZViscosity") "Map '$MapName'"
Assert-True ($mapProperties["AlundraCompanionJson"] -eq "$MapName.alundra.json") "Map companion property points to '$($mapProperties["AlundraCompanionJson"])'"

Assert-True ([int]$companion.width -eq [int]$map.width) "Companion width mismatch"
Assert-True ([int]$companion.height -eq [int]$map.height) "Companion height mismatch"
Assert-True (@($companion.cells).Count -eq ([int]$map.width * [int]$map.height)) "Companion cell count mismatch"

Assert-True ([int]$tileset.tilecount -eq @($tileset.tiles).Count) "Tileset tilecount does not match tiles array count"
Assert-True ([int]$tileset.columns -gt 0) "Tileset columns must be positive"
Assert-True ([int]$tileset.imagewidth -gt 0) "Tileset image width must be positive"
Assert-True ([int]$tileset.imageheight -gt 0) "Tileset image height must be positive"

$gidByRawTileId = @{}
foreach ($tile in @($tileset.tiles)) {
    $tileProperties = Get-TiledPropertyMap $tile.properties
    Assert-True ($tileProperties.ContainsKey("TileId")) "Tileset tile '$($tile.id)' is missing TileId"
    $gidByRawTileId[[string]$tileProperties["TileId"]] = [int]$tile.id + 1
}

$tilesetImagePath = Join-Path (Split-Path -Parent $tilesetPath) $tileset.image
Assert-True (Test-Path -LiteralPath $tilesetImagePath) "Tileset image is missing: $tilesetImagePath"
Add-Type -AssemblyName System.Drawing
$tilesetImage = [System.Drawing.Image]::FromFile($tilesetImagePath)
try {
    Assert-True ($tilesetImage.Width -eq [int]$tileset.imagewidth) "Tileset image width mismatch"
    Assert-True ($tilesetImage.Height -eq [int]$tileset.imageheight) "Tileset image height mismatch"
}
finally {
    $tilesetImage.Dispose()
}

$cellCount = [int]$map.width * [int]$map.height
foreach ($layer in @($map.layers | Where-Object { $_.type -eq "tilelayer" })) {
    Assert-True (@($layer.data).Count -eq $cellCount) "Tile layer '$($layer.name)' has invalid data length"
    foreach ($gid in @($layer.data)) {
        Assert-True ([int]$gid -ge 0) "Tile layer '$($layer.name)' contains a negative gid"
        Assert-True ([int]$gid -le [int]$tileset.tilecount) "Tile layer '$($layer.name)' contains gid $gid above tilecount $($tileset.tilecount)"
    }
}

foreach ($wallLayer in @($map.layers | Where-Object { $_.name -like "Walls_*" })) {
    $expectedData = New-Object 'int[]' $cellCount
    $stackIndex = [int]($wallLayer.name -replace '^Walls_', '')

    foreach ($cell in @($companion.cells)) {
        if ($null -eq $cell.wallTiles -or $stackIndex -ge @($cell.wallTiles.tiles).Count) {
            continue
        }

        $rawTileId = [int]$cell.wallTiles.tiles[$stackIndex]
        if ($rawTileId -eq 65535) {
            continue
        }

        $targetY = [int]$cell.y - [int]$cell.height - [int]$cell.wallTiles.offset + $stackIndex + 1
        if ($targetY -lt 0 -or $targetY -ge [int]$map.height) {
            continue
        }

        Assert-True ($gidByRawTileId.ContainsKey([string]$rawTileId)) "Wall layer '$($wallLayer.name)' references raw tile id $rawTileId missing from tileset"
        $targetIndex = $targetY * [int]$map.width + [int]$cell.x
        $expectedData[$targetIndex] = [int]$gidByRawTileId[[string]$rawTileId]
    }

    for ($index = 0; $index -lt $cellCount; $index++) {
        Assert-True ([int]$wallLayer.data[$index] -eq $expectedData[$index]) "Wall layer '$($wallLayer.name)' mismatch at cell index $index"
    }
}

$renderLayers = @($map.layers | Where-Object { $_.name -match '^Render_\d+$' })
Assert-True ($renderLayers.Count -gt 0) "Expected visible renderer-packed Render_* layers"

$rawGroundLayer = Get-LayerByName $map "Ground"
Assert-True ($rawGroundLayer.visible -eq $false) "Raw Ground layer must be hidden; visible Render_* layers provide game renderer ordering"
foreach ($wallLayer in @($map.layers | Where-Object { $_.name -like "Walls_*" })) {
    Assert-True ($wallLayer.visible -eq $false) "Raw wall layer '$($wallLayer.name)' must be hidden; visible Render_* layers provide game renderer ordering"
}

function Add-ExpectedRenderTile {
    param(
        [System.Collections.ArrayList]$Planes,
        [int]$TargetX,
        [int]$TargetY,
        [int]$Gid
    )

    if ($Gid -eq 0 -or $TargetX -lt 0 -or $TargetX -ge [int]$map.width -or $TargetY -lt 0 -or $TargetY -ge [int]$map.height) {
        return
    }

    $targetIndex = $TargetY * [int]$map.width + $TargetX
    foreach ($layerData in @($Planes)) {
        if ([int]$layerData[$targetIndex] -eq 0) {
            $layerData[$targetIndex] = $Gid
            return
        }
    }

    $newPlaneData = New-Object 'int[]' $cellCount
    $newPlaneData[$targetIndex] = $Gid
    [void]$Planes.Add($newPlaneData)
}

$expectedRenderPlanes = [System.Collections.ArrayList]::new()
foreach ($cell in @($companion.cells | Sort-Object { [int]$_.index })) {
    $rawTileId = [int]$cell.tileId
    if ($rawTileId -ne 65535) {
        Assert-True ($gidByRawTileId.ContainsKey([string]$rawTileId)) "Render ground cell $($cell.index) references raw tile id $rawTileId missing from tileset"
        Add-ExpectedRenderTile $expectedRenderPlanes ([int]$cell.x) ([int]$cell.y - [int]$cell.height) ([int]$gidByRawTileId[[string]$rawTileId])
    }

    if ($null -eq $cell.wallTiles) {
        continue
    }

    for ($stackIndex = 0; $stackIndex -lt [int]$cell.wallTiles.count -and $stackIndex -lt @($cell.wallTiles.tiles).Count; $stackIndex++) {
        $rawTileId = [int]$cell.wallTiles.tiles[$stackIndex]
        if ($rawTileId -eq 65535) {
            continue
        }

        Assert-True ($gidByRawTileId.ContainsKey([string]$rawTileId)) "Render wall cell $($cell.index) stack $stackIndex references raw tile id $rawTileId missing from tileset"
        $targetY = [int]$cell.y - [int]$cell.height - [int]$cell.wallTiles.offset + $stackIndex + 1
        Add-ExpectedRenderTile $expectedRenderPlanes ([int]$cell.x) $targetY ([int]$gidByRawTileId[[string]$rawTileId])
    }
}

$expectedRenderLayers = @()
for ($planeIndex = 0; $planeIndex -lt $expectedRenderPlanes.Count; $planeIndex++) {
    $expectedRenderLayers += [pscustomobject]@{
        Name = "Render_{0}" -f $planeIndex
        Data = $expectedRenderPlanes[$planeIndex]
    }
}

Assert-True ($renderLayers.Count -eq $expectedRenderLayers.Count) "Render layer count mismatch: expected $($expectedRenderLayers.Count), found $($renderLayers.Count)"
for ($layerIndex = 0; $layerIndex -lt $expectedRenderLayers.Count; $layerIndex++) {
    $expectedLayer = $expectedRenderLayers[$layerIndex]
    $actualLayer = $renderLayers[$layerIndex]
    Assert-True ($actualLayer.name -eq $expectedLayer.Name) "Render layer order mismatch at index ${layerIndex}: expected '$($expectedLayer.Name)', found '$($actualLayer.name)'"
    Assert-True ($actualLayer.visible -ne $false) "Render layer '$($actualLayer.name)' must be visible"
    $renderProperties = Assert-TiledProperties $actualLayer.properties @("Z", "RenderPlane", "Placement", "MergeStrategy") "Render layer '$($actualLayer.name)'"
    Assert-True ([int]$renderProperties["Z"] -eq $layerIndex) "Render layer '$($actualLayer.name)' Z property mismatch"
    Assert-True ([int]$renderProperties["RenderPlane"] -eq $layerIndex) "Render layer '$($actualLayer.name)' RenderPlane property mismatch"

    for ($index = 0; $index -lt $cellCount; $index++) {
        Assert-True ([int]$actualLayer.data[$index] -eq $expectedLayer.Data[$index]) "Render layer '$($actualLayer.name)' mismatch at cell index $index"
    }
}

$portalsLayer = Assert-ObjectLayer $map "Portals" @("Index", "X1", "Y1", "X2", "Y2", "DestMapId", "DestTileX", "DestTileY", "ZLevel", "Flags")
Assert-TiledProperties $portalsLayer.properties @("ValidityFilter", "Placement") "Portals layer" | Out-Null
Assert-ObjectLayer $map "MapEvents" @("Index", "X1", "Y1", "X2", "Y2", "EventCodesBIndex", "Ub1", "Ub2", "Ub3") | Out-Null
$entitiesLayer = Assert-ObjectLayer $map "Entities" @("Index", "XMin", "YMin", "XMax", "YMax", "IsEnabled", "SpriteDirection", "SpriteTableIndex", "XPos", "YPos", "Height", "EventCodesA_LoadIndex", "EventCodesB_MapIndex", "EventCodesC_TickIndex", "EventCodesD_TouchIndex", "EventCodesE_DeactivateIndex", "EventCodesF_InteractIndex", "_10", "Contents", "DisplayX", "DisplayY", "DisplayHeight", "DisplayPixelX", "DisplayPixelY")
foreach ($object in @($entitiesLayer.objects)) {
    $entityProperties = Get-TiledPropertyMap $object.properties
    $displayTileX = [int][Math]::Truncate([int]$entityProperties["XPos"] / 2)
    $displayTileY = [int][Math]::Truncate([int]$entityProperties["YPos"] / 2)
    $displayHeight = [int][Math]::Truncate([int]$entityProperties["Height"] / 2)
    $expectedX = $displayTileX * [int]$map.tilewidth
    $expectedY = ($displayTileY - $displayHeight) * [int]$map.tileheight

    Assert-True ([int]$object.x -eq $expectedX) "Entity '$($object.name)' x mismatch"
    Assert-True ([int]$object.y -eq $expectedY) "Entity '$($object.name)' y mismatch"
    Assert-True ([int]$object.width -eq [int]$map.tilewidth) "Entity '$($object.name)' width mismatch"
    Assert-True ([int]$object.height -eq [int]$map.tileheight) "Entity '$($object.name)' height mismatch"
    Assert-True ([int]$entityProperties["DisplayPixelX"] -eq $expectedX) "Entity '$($object.name)' DisplayPixelX mismatch"
    Assert-True ([int]$entityProperties["DisplayPixelY"] -eq $expectedY) "Entity '$($object.name)' DisplayPixelY mismatch"
}

foreach ($tile in @($tileset.tiles | Where-Object { $_.animation -ne $null })) {
    $animationProperties = Assert-TiledProperties $tile.properties @("AnimationSpriteIndex", "AnimationFrameCount", "AnimationTileHeight", "AnimationFrameDuration", "AnimationFrameDurationPsxFrames", "AnimationFrameDurationMs", "AnimationPsxFrameRateHz") "Animated tile '$($tile.id)'"
    Assert-True (@($tile.animation).Count -eq [int]$animationProperties["AnimationFrameCount"]) "Animated tile '$($tile.id)' frame count mismatch"
    Assert-True ([int]$animationProperties["AnimationFrameDuration"] -eq [int]$animationProperties["AnimationFrameDurationPsxFrames"]) "Animated tile '$($tile.id)' raw frame duration property mismatch"
    $expectedFrameDurationMs = [Math]::Max(1, [int][Math]::Round([int]$animationProperties["AnimationFrameDurationPsxFrames"] * 1000.0 / [int]$animationProperties["AnimationPsxFrameRateHz"], [MidpointRounding]::AwayFromZero))
    Assert-True ([int]$animationProperties["AnimationFrameDurationMs"] -eq $expectedFrameDurationMs) "Animated tile '$($tile.id)' converted frame duration mismatch"

    foreach ($frame in @($tile.animation)) {
        Assert-True ([int]$frame.tileid -ge 0) "Animated tile '$($tile.id)' has a negative frame tileid"
        Assert-True ([int]$frame.tileid -lt [int]$tileset.tilecount) "Animated tile '$($tile.id)' references tileid $($frame.tileid) outside tilecount $($tileset.tilecount)"
        Assert-True ([int]$frame.duration -eq [int]$animationProperties["AnimationFrameDurationMs"]) "Animated tile '$($tile.id)' duration mismatch"
    }
}

Write-Output "tiled-export-ok map=$MapName cells=$cellCount tilecount=$($tileset.tilecount) layers=$(@($map.layers).Count)"