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

$portalsLayer = Assert-ObjectLayer $map "Portals" @("Index", "X1", "Y1", "X2", "Y2", "DestMapId", "DestTileX", "DestTileY", "ZLevel", "Flags")
Assert-TiledProperties $portalsLayer.properties @("ValidityFilter", "Placement") "Portals layer" | Out-Null
Assert-ObjectLayer $map "MapEvents" @("Index", "X1", "Y1", "X2", "Y2", "EventCodesBIndex", "Ub1", "Ub2", "Ub3") | Out-Null
Assert-ObjectLayer $map "Entities" @("Index", "XMin", "YMin", "XMax", "YMax", "IsEnabled", "SpriteDirection", "SpriteTableIndex", "XPos", "YPos", "Height", "EventCodesA_LoadIndex", "EventCodesB_MapIndex", "EventCodesC_TickIndex", "EventCodesD_TouchIndex", "EventCodesE_DeactivateIndex", "EventCodesF_InteractIndex", "_10", "Contents", "DisplayX", "DisplayY", "DisplayHeight") | Out-Null

foreach ($tile in @($tileset.tiles | Where-Object { $_.animation -ne $null })) {
    $animationProperties = Assert-TiledProperties $tile.properties @("AnimationSpriteIndex", "AnimationFrameCount", "AnimationTileHeight", "AnimationFrameDuration") "Animated tile '$($tile.id)'"
    Assert-True (@($tile.animation).Count -eq [int]$animationProperties["AnimationFrameCount"]) "Animated tile '$($tile.id)' frame count mismatch"

    foreach ($frame in @($tile.animation)) {
        Assert-True ([int]$frame.tileid -ge 0) "Animated tile '$($tile.id)' has a negative frame tileid"
        Assert-True ([int]$frame.tileid -lt [int]$tileset.tilecount) "Animated tile '$($tile.id)' references tileid $($frame.tileid) outside tilecount $($tileset.tilecount)"
        Assert-True ([int]$frame.duration -eq [int]$animationProperties["AnimationFrameDuration"]) "Animated tile '$($tile.id)' duration mismatch"
    }
}

Write-Output "tiled-export-ok map=$MapName cells=$cellCount tilecount=$($tileset.tilecount) layers=$(@($map.layers).Count)"