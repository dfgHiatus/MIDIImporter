# MIDIImporter

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that converts MID files to WAV and imports them as usable audio.

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
1. Place and extract [MIDImporter_v1.0.2.rar](https://github.com/dfgHiatus/MIDIImporter/releases/tag/v1.0.2) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Resonite logs, or import a MID file.

(This may appear to do nothing at first, give it a few seconds to convert)

## Installing Custom SoundFonts
Soundfonts allow to change the way MID files are converted into WAV. I have provided one by default, but installing your own is straightforward:

1. Drop your SoundFont (`sf2`/`sf3`) into `midi_importer/bands` inside `rml_mods`.
2. Use modconfig to enter the full name of the SoundFont to use it.

Additional SoundFonts can be found [here](https://github.com/FluidSynth/fluidsynth/wiki/SoundFont)

## Credits
- [fluidSynth](https://github.com/FluidSynth/fluidsynth) - Conversion Library, LGPL-2.1 Licensed
- [SF2](http://www.schristiancollins.com/generaluser.php) - SoundFont Provider, GeneralUser GS v1.471 Licensed
