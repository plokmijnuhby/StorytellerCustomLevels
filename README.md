# CustomLevels
This is a mod for the game Storyteller, to add the ability to load custom levels. Right now there is no in-game editor, you need to use a text editor if you want to create your own levels. The custom levels page is placed after the epilogue.

## Installation instructions:
* Download the correct version of BepInEx. You will need Bleeding Edge #667 rather than the release version, for those who know what that means. The windows link is [here](https://builds.bepinex.dev/projects/bepinex_be/667/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.667%2B6b500b3.zip), the mac link is [here](https://builds.bepinex.dev/projects/bepinex_be/667/BepInEx-Unity.IL2CPP-macos-x64-6.0.0-be.667%2B6b500b3.zip).
* Install BepInEx by following [these instructions](https://docs.bepinex.dev/articles/user_guide/installation/index.html). Ignore the bit at the start about about downloading, since you've already done that.
* Download the latest version of this mod from [the releases page](https://github.com/plokmijnuhby/StorytellerCustomLevels/releases) (CustomLevels.dll, not the source code). Put it in the folder "BepInEx/plugins". Run the game, and the mod should load and create a folder called "custom_levels" in the game root. You can then add levels into this folder and they will immediately be detected by the mod.
* You may optionally choose to disable the BepInEx console. To do this, open the config file, find the `Logging.Console` section, and change the line `Enabled = true` to `Enabled = false`, and save the file.

## Making levels
The level format consists of a series of commands. To explain the format, here's a sample level - you might recognise it:
```
Frames 6
Music OthelloLigeia
Goal Murder of Jealousy
    Murders Ligeia None
Subgoal but everyone meets their demise
    Died Edgar
    Died Eleonora
    Died Ligeia
Setting Marriage
Setting Poison
Setting Study
Actor Edgar
Actor Eleonora
Actor Ligeia
```
The level consists of a series of commands in order. I will run through the possible commands here.

* `Music <level>` specifies that the music from the given level should be used. `Music <level> Devil` specifies that music from the devil variant of the level should be used.
* `Frames <n>` specifies that `n` frames should be used.
* `Actor <actor>` specifies a specific actor that can be placed.
* `Setting <setting>` specifies a setting that can be placed in a frame.
* `WitchStartsHot` specifies that, if present, the witch should begin the story as a young woman.
* `Goal <text>` specifies that a goal for the story should be present with the given text.
* `Subgoal <text>` is as above, but a subgoal instead.
* `Verbose` is a debugging feature, that causes all events to be logged to the console when checking goals, and also logs any assets retrieved (see below).

To specify how goals or subgoals work, you will need to give a list of events that the goal is checking for, and the characters that need to be involved. For simple goals like the ones we see here, the goal will be completed only if all the events are found. All events can be specified in the form `<event> <source> <target>`, where `source` and `target` are actors. You can also specify that the any source or target is valid using None, so `Murders Ligeia None` implies that Isobel (who is internally called `Ligeia`) murdered someone, but it doesn't matter who. If you specify only one actor, this will be assumed to be the source and the target will be set to `None`, so `Murders Ligeia None` is equivalent to simply `Murders Ligeia`. If you specify no actors at all both will be taken as `None`.

For some events, like `Idling`, only one actor is involved. In this case, it will be as if the source and the target are the same.

There are four commands that can affect the way goals are checked - `SEQUENCE`, `ALL`, `ANY` and `WITHOUT`. The `SEQUENCE` command specifies that events must occur in the order specified; the `ALL` command specifies that all events must occur, in any order; the `ANY` command specifies that only one of the given events needs to occur; and the WITHOUT command specifies that the event must NOT occur at any point. To understand these, it's best to look at [the example levels](./examples).

To see possible values for the arguments, you will probably need to consult [the enums directory](./enums). This gives possible values for actor, event, level, and setting names. Some of these names are quite different to the name the game actually displays.

Each subfolder in the `custom_levels` folder will be turned into a chapter. If there are no subfolders, then the `custom_levels` folder itself will become a chapter, and levels can be placed in the chapter directly. 

## Extra assets and characters
Some of the available actors, settings and levels do not appear in the main game, and thus may have missing assets. Even if you avoid these, some sets of interactions (eg pushing Lenora off a cliff) may not work quite correctly. For these scenarios, there is a way to add in or replace assets - simply place a png file with the correct name anywhere in the `custom_levels` folder or a subfolder, and it will be used correctly. To add a still image as an asset, the name of the png file should be the name of the asset, eg `adamgen_idle.png`. To add an animation, add the number of frames in the animation, eg `adamgen_idle_5.png`, and the image will be split into that number of frames. You can obtain the names of assets using the `Verbose` command in a level.

There is one asset that has special behaviour. Putting an image called `illustration.png` in a chapter folder will cause that image to be displayed as the illustration, to the left of the levels. The image should have dimensions 818x1228, although the edges of the image will always be overwritten by the image border.

To add another character to the game, you need to overwrite an existing actor. There are some unused actors you can use, such as `BlackCat`. You need to replace all the relevant animations, and also their name (so that they will be correctly shown in the toolbox). To replace the names of actors, create a file called `actors.txt` in the `custom_levels` folder, and write a line for each new character giving the actor id and new name, eg `BlackCat Bard` to rename `BlackCat` to `Bard`.