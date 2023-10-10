using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MIDImporter;

internal static class Utils
{
    // Credit to delta for this method https://github.com/XDelta/
    internal static string GenerateMD5(string filepath)
    {
        using var hasher = MD5.Create();
        using var stream = File.OpenRead(filepath);
        var hash = hasher.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "");
    }

    internal static bool ContainsUnicodeCharacter(string input)
    {
        const int MaxAnsiCode = 255;
        return input.Any(c => c > MaxAnsiCode);
    }
}