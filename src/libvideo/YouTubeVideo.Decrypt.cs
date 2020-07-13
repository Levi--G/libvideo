﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public partial class YouTubeVideo
    {
        private static readonly string[] DecryptionFunctionRegex = {
            @"\bc\s*&&\s*a\.set\([^,]+,\s*(?:encodeURIComponent\s*\()?\s*([\w$]+)\(",
            @"\b[cs]\s*&&\s*[adf]\.set\([^,]+\s*,\s*encodeURIComponent\s*\(\s*([a-zA-Z0-9$]+)\(",
            @"\b[a-zA-Z0-9]+\s*&&\s*[a-zA-Z0-9]+\.set\([^,]+\s*,\s*encodeURIComponent\s*\(\s*([a-zA-Z0-9$]+)\(",
            @"([a-zA-Z0-9$]+)\s*=\s*function\(\s*a\s*\)\s*{\s*a\s*=\s*a\.split\(\s*""\s*\)"
        };
        private static readonly Regex FunctionRegex = new Regex(@"\w+\.(\w+)\(");

        private async Task DecryptAsync(Func<DelegatingClient> makeClient)
        {
            var signature = query.Signature;
            if (signature == null)
                return;

            if (string.IsNullOrWhiteSpace(signature))
                throw new Exception("Signature not found.");

            var js =
                await makeClient()
                .GetStringAsync(jsPlayer)
                .ConfigureAwait(false);

            query.Signature = DecryptSignature(js, signature);
        }

        private string DecryptSignature(string js, string signature)
        {
            var functionLines = GetDecryptionFunctionLines(js);
            var decryptor = new Decryptor();
            var deciphererDefinitionName = Regex.Match(string.Join(";", functionLines), "(\\w+).\\w+\\(\\w+,\\d+\\);").Groups[1].Value;
            if (string.IsNullOrEmpty(deciphererDefinitionName))
            {
                throw new Exception("Could not find signature decipherer definition name. Please report this issue to us.");
            }
            var deciphererDefinitionBody = Regex.Match(js, @"var\s+" + Regex.Escape(deciphererDefinitionName) + @"=\{(\w+:function\(\w+(,\w+)?\)\{(.*?)\}),?\};", RegexOptions.Singleline).Groups[0].Value;
            if (string.IsNullOrEmpty(deciphererDefinitionBody))
            {
                throw new Exception("Could not find signature decipherer definition body. Please report this issue to us.");
            }
            foreach (var functionLine in functionLines)
            {
                if (decryptor.IsComplete)
                {
                    break;
                }

                var match = FunctionRegex.Match(functionLine);
                if (match.Success)
                {
                    decryptor.AddFunction(deciphererDefinitionBody, match.Groups[1].Value);
                }
            }

            foreach (var functionLine in functionLines)
            {
                var match = FunctionRegex.Match(functionLine);
                if (match.Success)
                {
                    signature = decryptor.ExecuteFunction(signature, functionLine, match.Groups[1].Value);
                }
            }

            return signature;
        }

        private string[] GetDecryptionFunctionLines(string js)
        {
            var deciphererFuncName = Regex.Match(js, @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}");
            if (deciphererFuncName.Success)
            {
                var deciphererFuncBody = Regex.Match(js, @"(?!h\.)" + Regex.Escape(deciphererFuncName.Groups[1].Value) + @"=function\(\w+\)\{(.*?)\}", RegexOptions.Singleline);
                if (deciphererFuncBody.Success)
                {
                    return deciphererFuncBody.Groups[1].Value.Split(';');
                }
            }
            throw new Exception("Could not find signature DecryptionFunctionLines. Please report this issue to us.");
        }

        private string GetDecryptionFunction(string js)
        {
            foreach (var regex in DecryptionFunctionRegex)
            {
                var match = Regex.Match(js, regex);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            throw new Exception($"{nameof(GetDecryptionFunction)} failed");
        }

        private class Decryptor
        {
            private static readonly Regex ParametersRegex = new Regex(@"\(\w+,(\d+)\)");

            private readonly Dictionary<string, FunctionType> _functionTypes = new Dictionary<string, FunctionType>();
            private readonly StringBuilder _stringBuilder = new StringBuilder();

            public bool IsComplete =>
                _functionTypes.Count == Enum.GetValues(typeof(FunctionType)).Length;

            public void AddFunction(string js, string function)
            {
                var escapedFunction = Regex.Escape(function);
                FunctionType? type = null;

                /* Pass  "do":function(a){} or xa:function(a,b){} */
                if (Regex.IsMatch(js, $@"(\"")?{escapedFunction}(\"")?:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    type = FunctionType.Slice;
                }
                else if (Regex.IsMatch(js, $@"(\"")?{escapedFunction}(\"")?:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    type = FunctionType.Swap;
                }
                if (Regex.IsMatch(js, $@"(\"")?{escapedFunction}(\"")?:\bfunction\b\(\w+\){{\w+\.reverse"))
                {
                    type = FunctionType.Reverse;
                }

                if (type.HasValue)
                {
                    _functionTypes[function] = type.Value;
                }
            }

            public string ExecuteFunction(string signature, string line, string function)
            {
                if (!_functionTypes.TryGetValue(function, out var type))
                {
                    return signature;
                }

                switch (type)
                {
                    case FunctionType.Reverse:
                        return Reverse(signature);
                    case FunctionType.Slice:
                    case FunctionType.Swap:
                        var index =
                            int.Parse(
                                ParametersRegex.Match(line).Groups[1].Value,
                                NumberStyles.AllowThousands,
                                NumberFormatInfo.InvariantInfo);
                        return
                            type == FunctionType.Slice
                                ? Slice(signature, index)
                                : Swap(signature, index);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type));
                }
            }

            private string Reverse(string signature)
            {
                _stringBuilder.Clear();
                for (var index = signature.Length - 1; index >= 0; index--)
                {
                    _stringBuilder.Append(signature[index]);
                }

                return _stringBuilder.ToString();
            }

            private string Slice(string signature, int index) =>
                signature.Substring(index);

            private string Swap(string signature, int index)
            {
                _stringBuilder.Clear();
                _stringBuilder.Append(signature);
                _stringBuilder[0] = _stringBuilder[index % _stringBuilder.Length];
                _stringBuilder[index % _stringBuilder.Length] = signature[0];
                return _stringBuilder.ToString();
            }

            private enum FunctionType
            {
                Reverse,
                Slice,
                Swap
            }
        }
    }
}