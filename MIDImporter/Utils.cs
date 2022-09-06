using CodeX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MIDImporter
{
    public static class Utils
    {
        // Credit to delta for this method https://github.com/XDelta/
        public static string GenerateMD5(string filepath)
        {
            using var hasher = MD5.Create();
            using var stream = File.OpenRead(filepath);
            var hash = hasher.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "");
        }

        public static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;
            return input.Any(c => c > MaxAnsiCode);
        }
    }
}