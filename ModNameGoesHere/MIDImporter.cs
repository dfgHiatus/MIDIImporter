using BaseX;
using CodeX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
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

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<string> bandName = new ModConfigurationKey<string>("band", "MIDI Converter file path", () => defaultBandName);

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

            var fullBandPath = Path.Combine(bandPath, config.GetValue(bandName));
            LocalDB localDB = world.Engine.LocalDB;
            foreach (var inputMid in files)
            {
                var convertedMidName = Path.GetFileNameWithoutExtension(inputMid);
                await MIDConverter.Convert(fullBandPath, inputMid, convertedMidPath, convertedMidName).ConfigureAwait(false);
                var final = Path.GetFullPath(Path.Combine(convertedMidPath, convertedMidName + ".wav"));
                Msg(final);
                await localDB.ImportLocalAssetAsync(
                    Path.Combine(final),
                    LocalDB.ImportLocation.Copy).
                    ConfigureAwait(continueOnCapturedContext: false);
            }
        }
	}
}