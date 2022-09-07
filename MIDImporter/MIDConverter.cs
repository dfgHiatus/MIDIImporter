using BaseX;
using System;
using System.IO;
using System.Diagnostics;
using FrooxEngine;
using CodeX;
using System.Threading.Tasks;

namespace MIDImporter
{
    public class MIDConverter
    {
        private static readonly string executablePath = Path.Combine("nml_mods", "midi_importer");
        private static readonly string windowsExecutable = "fluidsynth.exe";
        private static readonly string windowsArgs = "-ni \"{0}\" \"{1}\" --fast-render \"{2}.wav\"";
        private static readonly string macOSXCommand;
        private static readonly string unixCommand;

        public async static Task Convert(string band, string inputFile, string outputPath, string outputName, bool showDebugWindow)
        {
            var platform = Environment.OSVersion.Platform;
            switch (platform)
            {
                // According to https://docs.microsoft.com/en-us/dotnet/api/system.platformid?view=netframework-4.7a.2
                // some platform enums are no longer in use. I've left them here:
                // - case PlatformID.Win32S
                // - case PlatformID.Win32Windows
                // - case PlatformID.WinCE
                // - case PlatformID.Xbox:

                case PlatformID.Win32NT:
                    await PerformWindowsUnpack(band, inputFile, outputPath, outputName, showDebugWindow);
                    break;
                case PlatformID.Unix:
                    throw new PlatformNotSupportedException("Unix support has not been added yet! Scream at dfg if you see this!");
                case PlatformID.MacOSX:
                    throw new PlatformNotSupportedException("MacOS support has not been added yet! Scream at dfg if you see this!");
                default:
                    throw new PlatformNotSupportedException("You've managed to run this on a platform .NET doesn't recognize, how are you playing Neos?");
            }
        }
        private static Task PerformWindowsUnpack(string band, string inputFile, string outputPath, string outputName, bool showDebugWindow)
        {
            var windowsExecutablePath = Path.GetFullPath(Path.Combine(executablePath, windowsExecutable));

            if (!File.Exists(windowsExecutablePath))
                throw new FileNotFoundException("Could not find fluidsynth. Is it present under nml_mods/midi_converter/fluidsynth.exe?");

            string formattedWindowsArgs = string.Format(
                windowsArgs, 
                band, 
                inputFile, 
                Path.Combine(outputPath, outputName));

            UniLog.Log($"Starting MIDI conversion with the following command: {windowsExecutablePath} {formattedWindowsArgs}");

            var process = new Process();
            process.StartInfo.FileName = windowsExecutablePath;
            process.StartInfo.Arguments = formattedWindowsArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = !showDebugWindow;
            process.StartInfo.WindowStyle = showDebugWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();

            UniLog.Log("MIDI extraction complete!");

            return Task.CompletedTask;
        }

        private static void PerformMacOSXConvert(string inputFile, string outputPath, int? optionalThreads)
        {
            throw new NotImplementedException();
        }

        private static void PerformUniXConvert(string inputFile, string outputPath, int? optionalThreads)
        {
            throw new NotImplementedException();
        }
    }
}
