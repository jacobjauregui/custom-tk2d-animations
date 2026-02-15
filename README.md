# Custom TK2D Animation Editor

***Custom TK2D Animations Editor*** is a set of scripts that allow you to view, create, edit and export TK2D animations from you game. This is a work in progress, and only is available for Unity because it uses the Unity Editor API to work. 

This is a personal tool that I created to export and sort more easily the sprites of the animations from a game. Whatever, if you want to use it, please feel free to do it. Any feedback, suggestion or contribution is welcome.

**Note:** For now only the ***TK2D Frames Exporter*** is available, but the ***TK2D Animation Viewer*** and ***TK2D Sprite Editor*** will be added in the future.

## How the exporter works?

Is very easy but, is important to understand how TK2D animations are structured. When I refer to *Animation* it means an animated object (bells, silk threads, effects, etc) or character (NPC, boss, enemie, etc), which has a specific **GameObject** or **Prefab** inside of the *Unity/Project/Assets/Animations* path, so each animation can have several clips, depending on its movements; each clip represents a different movement and each clip is made up of a set of frames. For example, the boss *"Mossbone Mother"* is an animation that has 16 movements or clips, one of those is the clip called *"Fly"*, which is made up of 4 frames to 12 fps.

So, to export the sprites in all clips of an animation you need to:

1. Open the *TK2D Animation Exporter* window from the menu: ***Tools > TK2D > Animation Frames Exporter***.
2. Drag and drop the animation's Prefab into the *GameObject / Prefab* field.
3. Select the main folder to save the exported animations. For example: *C:/Users/User/Pictures/Game Animations*
4. Click on the *Export* button.

This will create a directory with the same name as the animation inside the main folder selected, then will export all frames as image files with *.png* extension for each clip, sorted in folders with the clip name. For example, if you export the *Mossbone Mother* animation, you will get the following structure:
***C:/Users/User/Pictures/Game Animations/Mossbone Mother Anim/Fly/MBM_fly0001.png***

In addition to the image files, a file with *.json* extension will also be created with additional information for each clip, such as frame names, fps, duration, wrap mode, etc. This file can be used to show more details above the clips and allows you to share the animation with other software using serialization and deserialization of the data.

...


### IMPORTANT

- This is not an official tool of the original ***Tool Kit 2D (TK2D)*** package, and is not intended to be used as a replacement or modification of the original tool. It is a custom implementation that uses the Unity Editor API to access and export the animation data from the game.
- This project is not affiliated with any game or company, and is only intended for educational and personal use. Please respect the intellectual property rights of the original creators of the animations and do not use this tool for commercial purposes without permission.
- This tool was tested just for the game ***Hollow Knight: Silksong***, so it may not work properly with other games that use TK2D animations, or with different versions of the TK2D package. Use it at your own risk and feel free to report any issues or bugs you find.
