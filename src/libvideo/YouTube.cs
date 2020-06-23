using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public class YouTube : ServiceBase<YouTubeVideo>
    {
        private const string Playback = "videoplayback";

        public static YouTube Default { get; } = new YouTube();

        internal async override Task<IEnumerable<YouTubeVideo>> GetAllVideosAsync(
            string videoUri, Func<string, Task<string>> sourceFactory)
        {
            if (!TryNormalize(videoUri, out videoUri))
                throw new ArgumentException("URL is not a valid YouTube URL!");

            string source = await
                sourceFactory(videoUri)
                .ConfigureAwait(false);

            return ParseVideos(source);
        }

        private bool TryNormalize(string videoUri, out string normalized)
        {
            // If you fix something in here, please be sure to fix in 
            // DownloadUrlResolver.TryNormalizeYoutubeUrl as well.

            normalized = null;

            var builder = new StringBuilder(videoUri);

            videoUri = builder.Replace("youtu.be/", "youtube.com/watch?v=")
                .Replace("youtube.com/embed/", "youtube.com/watch?v=")
                .Replace("/v/", "/watch?v=")
                .Replace("/watch#", "/watch?")
                .ToString();

            var query = new Query(videoUri);

            string value;

            if (!query.TryGetValue("v", out value))
                return false;

            normalized = "https://youtube.com/watch?v=" + value;
            return true;
        }

        private IEnumerable<YouTubeVideo> ParseVideos(string source)
        {
            string title = Html.GetNode("title", source);

            string jsPlayer = ParseJsPlayer(source);
            if (jsPlayer == null)
            {
                yield break;
            }

            string map = Json.GetKey("url_encoded_fmt_stream_map", source);
            if (!string.IsNullOrEmpty(map))
            {
                var queries = map.Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(Unscramble);
                foreach (var query in queries)
                    yield return new YouTubeVideo(title, query, jsPlayer);
            }
            string adaptiveMap = Json.GetKey("adaptive_fmts", source);// ?? Json.GetKey("adaptiveFormats", source);
            if (!string.IsNullOrEmpty(adaptiveMap))
            {
                var queries = adaptiveMap.Split(',').Select(Unscramble);
                foreach (var query in queries)
                    yield return new YouTubeVideo(title, query, jsPlayer);
            }
            else
            {
                string temp = Json.GetKey("dashmpd", source);
                if (!string.IsNullOrEmpty(temp))
                {
                    using (HttpClient hc = new HttpClient())
                    {
                        temp = WebUtility.UrlDecode(temp).Replace(@"\/", "/");

                        var manifest = hc.GetStringAsync(temp)
                            .GetAwaiter().GetResult()
                            .Replace(@"\/", "/");

                        var uris = Html.GetUrisFromManifest(manifest);

                        foreach (var v in uris)
                        {
                            yield return new YouTubeVideo(title,
                                UnscrambleManifestUri(v),
                                jsPlayer);
                        }
                    }
                }
            }
            var playerResponseMap = Json.GetKey("player_response", source);
            if (!string.IsNullOrEmpty(playerResponseMap))
            {
                var playerResponseJToken = JToken.Parse(Regex.Unescape(playerResponseMap).Replace(@"\u0026", "&"));
                if (playerResponseJToken.SelectToken("playabilityStatus.status")?.Value<string>().ToLower() == "error")
                {
                    yield break;
                }
                if (string.IsNullOrWhiteSpace(playerResponseJToken.SelectToken("playabilityStatus.reason")?.Value<string>()))
                {
                    if (playerResponseJToken.SelectToken("videoDetails.isLive")?.Value<bool>() == true)
                    {
                        yield break;
                    }
                    var streams = (playerResponseJToken.SelectToken("streamingData.formats", false)?.Children() ?? Enumerable.Empty<JToken>())
                        .Concat(playerResponseJToken.SelectToken("streamingData.adaptiveFormats", false)?.Children() ?? Enumerable.Empty<JToken>());
                    foreach (var item in streams)
                    {
                        YouTubeVideo video = null;
                        var urlValue = item.SelectToken("url")?.Value<string>();
                        if (!string.IsNullOrEmpty(urlValue))
                        {
                            var query = new SignatureQuery(urlValue, null, false);
                            video = new YouTubeVideo(title, query, jsPlayer);
                        }
                        var cipherValue = item.SelectToken("cipher")?.Value<string>() ?? item.SelectToken("signatureCipher")?.Value<string>();
                        if (!string.IsNullOrEmpty(cipherValue))
                        {
                            video = new YouTubeVideo(title, Unscramble(cipherValue), jsPlayer);
                        }
#if DEBUG
                        if (((video.Format == VideoFormat.Unknown || video.Resolution == -1) && video.AdaptiveKind != AdaptiveKind.Audio) || (video.AudioFormat == AudioFormat.Unknown && video.AdaptiveKind != AdaptiveKind.Video))
                        {
                            Debugger.Break();
                        }
#endif
                        if (video != null)
                        {
                            yield return video;
                        }
                    }
                }
            }
        }

        private string ParseJsPlayer(string source)
        {
            string jsPlayer = Json.GetKey("js", source)?.Replace(@"\/", "/");
            //<script src="/yts/jsbin/player_ias-vfl9X5OgR/en_US/base.js"  name="player_ias/base" ></script>
            if (string.IsNullOrWhiteSpace(jsPlayer))
            {
                try
                {
                    string pattern = @"<script\s+src=""(\/yts[^""]+\.js)""\s*name=""player_ias\/base""\s*>";
                    Match match = Regex.Match(source, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    jsPlayer = match?.Groups.OfType<Group>().Skip(1).FirstOrDefault()?.Value;
                }
                catch { }
            }
            if (string.IsNullOrWhiteSpace(jsPlayer))
            {
                return null;
            }

            if (jsPlayer.StartsWith("/"))
            {
                return $"https://www.youtube.com{jsPlayer}";
            }

            // Fall back on old implementation (not sure it's needed)
            if (!jsPlayer.StartsWith("http"))
            {
                jsPlayer = $"https:{jsPlayer}";
            }

            return jsPlayer;
        }
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        // TODO: Consider making this static...
        private SignatureQuery Unscramble(string queryString)
        {
            queryString = queryString.Replace(@"\u0026", "&");
            var query = new Query(queryString);
            string uri = query["url"];

            bool encrypted = false;
            string signature;

            query.TryGetValue("sp", out var _signatureKey);

            _signatureKey = string.IsNullOrWhiteSpace(_signatureKey) ? "signature" : _signatureKey;

            if (query.TryGetValue("s", out signature))
            {
                encrypted = true;
            }
            else if (query.TryGetValue("sig", out signature))
            {
                encrypted = false;
            }

            uri = WebUtility.UrlDecode(
                WebUtility.UrlDecode(uri));

            var q = new SignatureQuery(uri, _signatureKey, encrypted);

            if (signature != null)
            {
                q.Query.AddIfNotExists(_signatureKey, WebUtility.UrlDecode(WebUtility.UrlDecode(signature)));
            }

            if (query.TryGetValue("fallback_host", out var host))
                q.Query.AddIfNotExists("fallback_host", host);

            q.Query.AddIfNotExists("ratebypass", "yes");

            return q;
        }

        private SignatureQuery UnscrambleManifestUri(string manifestUri)
        {
            int start = manifestUri.IndexOf(Playback) + Playback.Length;
            string baseUri = manifestUri.Substring(0, start);
            string parametersString = manifestUri.Substring(start, manifestUri.Length - start);
            var parameters = parametersString.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var builder = new StringBuilder(baseUri);
            builder.Append("?");
            for (var i = 0; i < parameters.Length; i += 2)
            {
                builder.Append(parameters[i]);
                builder.Append('=');
                builder.Append(parameters[i + 1].Replace("%2F", "/"));
                if (i < parameters.Length - 2)
                {
                    builder.Append('&');
                }
            }

            return new SignatureQuery(builder.ToString(), null, false);
        }
    }
}
