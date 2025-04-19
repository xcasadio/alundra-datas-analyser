<p align="center">
  <img src="./gitHub/alundra-logo.jpg">
</p>

This project is fork from [surixurient](https://github.com/surixurient/alundra).

## Objectives
- Understand how Alundra game data is structured.
- Use my game engine [CasaEngineMonoGame](https://github.com/xcasadio/CasaEngineMonogame) to recreate the game.

## Contributors
You are welcome, you can contact me with [GitHub Discussions](https://github.com/xcasadio/alundra-datas-analyser/discussions).

# Tools
## Source code analyser
- I use Ghidra (11.3.1) to analyse the source code, the project is in the folder Ghidra.
    >To open the project, you must edit the file \Ghidra\Alundra.rep\project.prp\
    Modify the node State specifying your Windows account name in VALUE.
- I use pcsx-redux to debug the code
- [Connecting Ghidra to PCSX-Redux](https://pcsx-redux.consoledev.net/Debugging/ghidra/)

## Datas analyser
Open GraphicsTool.sln and open the Datas.bin file (you'll need to extract the data from your Alundra game).
![Screenshot of GraphicsTool](/gitHub/GraphisTool_screenshot.jpg)


## Datas.bin structure
Datas.bin  
├── DbHeader  
├── GameMap[]  
│   ├── GameMapHeader  
│   ├── GameMapInfo (Infoblock)  
│   ├── Map (Mapblock)  
│   │   └── MapTile[] → WallTiles  
│   ├── Tilesheet (Tilesheets)  
│   ├── SpriteInfo (Spriteinfo)  
│   │   ├── Sprites, Effets  
│   │   ├── Animations  
│   │   ├── Palettes  
│   │   ├── Entities  
│   │   └── EventCodes A–F  
│   ├── Spritesheet (Spritesheets)  
│   ├── ScrollScreen  
│   └── StringTable[128]  
