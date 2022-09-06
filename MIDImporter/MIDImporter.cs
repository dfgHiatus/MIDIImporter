using BaseX;
using CodeX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MIDImporter
{
    public class MIDImporter : NeosMod
    {
        public override string Name => "MIDImporter";
        public override string Author => "dfgHiatus";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/dfgHiatus/MIDImporter";

        private static readonly string convertedMidPath = Path.Combine(Engine.Current.CachePath, "Cache", "ConvertedMIDIs");
        private static readonly string bandPath = Path.Combine("nml_mods", "midi_importer", "bands");
        private static readonly string defaultBandName = "GeneralUser_GS_v1.471.sf2";
        private static ModConfiguration config;

        public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
        {
            builder
                .Version(new Version(1, 0, 0))
                .AutoSave(true);
        }

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> importAsRawFiles =
            new("importAsRawFiles",
            "Import binary variants of audio files",
            () => false);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> showDebugWindow =
            new("showDebugWindow",
            "Show debug window during import",
            () => false);
        [AutoRegisterConfigKey]
        public static ModConfigurationKey<string> bandName = 
            new("band", 
            "MIDI Converter file name", 
            () => defaultBandName);

        public override void OnEngineInit()
        {
            new Harmony("net.dfgHiatus.MIDImporter").PatchAll();
            Directory.CreateDirectory(convertedMidPath);
            config = GetConfiguration();
            Engine.Current.RunPostInit(() => AssetPatch());
        }

        public static void AssetPatch()
        {
            var aExt = Traverse.Create(typeof(AssetHelper)).Field<Dictionary<AssetClass, List<string>>>("associatedExtensions");
            aExt.Value[AssetClass.Special].Add("mid");
        }

        [HarmonyPatch(typeof(UniversalImporter), "ImportTask", typeof(AssetClass), typeof(IEnumerable<string>), 
            typeof(World), typeof(float3), typeof(floatQ), typeof(float3), typeof(bool))]
        public class UniversalImporterPatch
        {
            static bool Prefix(IEnumerable<string> files, ref Task __result, World world)
            {
                var query = files.Where(x => x.ToLower().EndsWith(".mid"));
                if (query.Count() > 0)
                {
                    __result = ProcessMIDImport(query, world);
                }
                return true;
            }
        }

		private static async Task ProcessMIDImport(IEnumerable<string> files, World world)
		{
			await default(ToBackground);

            if (!File.Exists(Path.Combine(bandPath, config.GetValue(bandName))))
            {
                if (!File.Exists(Path.Combine(bandPath, defaultBandName)))
                {
                    Error($"The default MID converter {defaultBandName} was not found. Are you missing this file under nml_mods/midi_converter/bands?");
                    return;
                }
                config.Set(bandName, defaultBandName);
            }

            var fileToHash = files.ToDictionary(file => file, Utils.GenerateMD5);
            HashSet<string> dirsToImport = new();
            HashSet<string> midisToConvert = new();
            foreach (var element in fileToHash)
            {
                var dir = Path.Combine(convertedMidPath, element.Value);
                if (!Directory.Exists(dir))
                    midisToConvert.Add(element.Key);
                else
                    dirsToImport.Add(dir);
            }

            await default(ToWorld);
            var preConvertedslot = Engine.Current.WorldManager.FocusedWorld.AddSlot("Preconverted Audio");
            preConvertedslot.PositionInFrontOfUser();
            float3 offset = float3.Zero;
            foreach (var dirs in dirsToImport)
            {
                foreach (var audio in Directory.GetFiles(dirs)) // This will be 1 file per directory/hash
                {
                    UniversalImporter.Import(audio, world, preConvertedslot.GlobalPosition + offset, floatQ.Identity, false, config.GetValue(importAsRawFiles));
                    offset = offset + new float3(0.2f, 0f, 0f);
                }
            }
            await default(ToBackground);

            var fullBandPath = Path.Combine(bandPath, config.GetValue(bandName));
            offset = float3.Zero;
            foreach (var inputMid in midisToConvert)
            {
                if (Utils.ContainsUnicodeCharacter(inputMid))
                {
                    Error($"Imported MIDI {inputMid} cannot have unicode characters in its file name.");
                    continue;
                }
                else if (inputMid.IndexOf(" ") > -1)
                {
                    Error($"Imported MIDI {inputMid} cannot have spaces in its file name.");
                    continue;
                }

                var extractedPath = Path.Combine(convertedMidPath, fileToHash[inputMid]);
                Directory.CreateDirectory(extractedPath);

                var convertedMidName = Path.GetFileNameWithoutExtension(inputMid);
                await MIDConverter.Convert(fullBandPath, inputMid, extractedPath, convertedMidName, config.GetValue(showDebugWindow)).ConfigureAwait(false);
                var wavName = convertedMidName + ".wav";
                var final = Path.GetFullPath(Path.Combine(extractedPath, wavName));

                await default(ToWorld);
                var slot = Engine.Current.WorldManager.FocusedWorld.AddSlot(wavName);
                slot.PositionInFrontOfUser();
                UniversalImporter.Import(final, world, slot.GlobalPosition + offset, floatQ.Identity, false, config.GetValue(importAsRawFiles));
                offset = offset + new float3(0.2f, 0f, 0f);
                await default(ToBackground);
            }
        }
	}
}