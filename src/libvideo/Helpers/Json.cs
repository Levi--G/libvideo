﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    internal static class Json
    {
        public static bool TryGetKey(string key, string source, out string target)
        {
            target = GetKey(key, source);
            return target != null;
        }

        public static string GetKey(string key, string source)
        {
            // Example scenario: "key" : "value"

            string quotedKey = '"' + key + '"';
            int index = 0;

            while (true)
            {
                index = source.IndexOf(quotedKey, index); // '"'
                if (index == -1) return null;
                index += quotedKey.Length; // ' '

                int start = index;
                start = source.SkipWhitespace(start); // ':'
                if (source[start++] != ':') // ' '
                    continue;
                start = source.SkipWhitespace(start); // '"'
                if (source[start++] != '"') // 'v'
                    continue;
                int end = start;
                while (source[end] != '"' || source[end - 1] == '\\') // "value\""
                    end++;
                return source.Substring(start, end - start);
            }
        }
    }
}
