# CustomLevels
This is a mod for the game Storyteller, to add the ability to load custom levels. Right now there is no in-game editor, you need to use a text editor if you want to create your own levels. I designed the mod so the custom levels can only be accessed after you have completed the game and got the crown (since I feel like something should be unlocked at that point).

## Installation instructions:
* Download the correct version of BepInEx. You will need Bleeding Edge #667 rather than the release version, for those who know what that means. The windows link is [here](https://builds.bepinex.dev/projects/bepinex_be/667/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.667%2B6b500b3.zip), the mac link is [here](https://builds.bepinex.dev/projects/bepinex_be/667/BepInEx-Unity.IL2CPP-macos-x64-6.0.0-be.667%2B6b500b3.zip).
* Install BepInEx by following [these instructions](https://docs.bepinex.dev/articles/user_guide/installation/index.html). Ignore the bit at the start about about downloading, since you've already done that.
* Download the latest version of this mod from [the releases page](https://github.com/plokmijnuhby/StorytellerCustomLevels/releases) (CustomLevels.dll, not the source code). Put it in the folder "BepInEx/plugins". Run the game, and the mod should load and create a folder called "custom_levels" in the game root. You can then add levels into this folder and they will immediately be detected by the mod.
* This is not strictly necessary, but I recommend disabling the BepInEx console. To do this, open the config file, find the `Logging.Console` section, and change the line `Enabled = true` to `Enabled = false`, and save the file.

## Making levels
The level format consists of a series of commands. To explain the format, here's a sample level - you might recognise it:
```
Frames 6
Music OthelloLigeia
Goal Isobel Commits a Crime
    Murders Ligeia None
Subgoal but everyone meets their demise
    DiedBy Edgar
    DiedBy Eleonora
    DiedBy Ligeia
Setting Marriage
Setting Poison
Setting Study
Actor Edgar
Actor Eleonora
Actor Ligeia
```
The level consists of a series of commands in order. I will run through the possible commands here.

* `Music <level>` specifies that the music from the given level should be used.
* `Frames <n>` specifies that `n` frames should be used.
* `Actor <actor>` specifies a specific actor that can be placed.
* `Setting <setting>` specifies a setting that can be placed in a frame.
* `MarriageAloneIsSolitude` specifies that idling characters with no lovers should feel lonely in some settings.
* `WitchStartsHot` specifies that, if present, the witch should begin the story as a young woman.
* `Goal <text>` specifies that a goal for the story should be present with the given text.
* `Subgoal <text>` is as above, but a subgoal instead.

To specify how goals or subgoals work, you will need to give a list of events that the goal is checking for, and the characters that need to be involved. For simple goals like the ones we see here, the goal will be completed only if all the events are found. All events can be specified in the form `<event> <source> <target>`, where `source` and `target` are actors. You can also specify that the any source or target is valid using None, so `Murders Ligeia None` implies that Isobel (who is internally called `Ligeia`) murdered someone, but it doesn't matter who. If you specify only one actor, this will be assumed to be the source and the target will be set to `None`, so `Murders Ligeia None` is equivalent to simply `Murders Ligeia`. If you specify no actors at all both will be taken as `None`.

For some events, like `DiedBy` in this example, only one actor is involved. In this case, it will be as if the source and the target are the same. Also, while some events have extra metadata associated with them (in the case of `DiedBy`, the cause of death), the current level format does not provide a way to access this data.

There are four commands that can affect the way goals are checked - `SEQUENCE`, `ALL`, `ANY` and `WITHOUT`. The `SEQUENCE` command specifies that events must occur in the order specified; the `ALL` command specifies that all events must occur, in any order; the `ANY` command specifies that only one of the given events needs to occur; and the WITHOUT command specifies that the event must NOT occur at any point. To understand these, it's best to look at [the example levels](./examples).

To see possible values for the arguments, you will probably need to consult [the enums directory](./enums). This gives possible values for actor, event, level, and setting names. Some of these names are quite different to the name the game actually displays.

It should be noted that some of the actors are not supposed to be placable, and are related to a specific setting. Also, some of the actors, settings and levels do not appear in the main game, and thus may have missing assets. Even if you avoid these, some sets of interactions (eg pushing Lenora off a cliff) may not work quite correctly. You might choose to avoid these... or not, if you know what you're doing.
