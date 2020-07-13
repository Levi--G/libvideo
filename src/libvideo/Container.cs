using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public enum Container
    {
        Unknown,
        Flv,
        _3Gp,
        MP4,
        WebM,
        Mkv,
        Mp3,
        Aac,
        Opus,
        Ogg,
        AC3
    }

    public static class Extension
    {
        public static string GetExtension(this Container c)
        {
            switch (c)
            {
                case Container.Flv: return ".flv";
                case Container._3Gp: return ".3gp";
                case Container.MP4: return ".mp4";
                case Container.WebM: return ".webm";
                case Container.Mkv: return ".mkv";
                case Container.Mp3: return ".mp3";
                case Container.Aac: return ".aac";
                case Container.Opus: return ".opus";
                case Container.Ogg: return ".ogg";
                case Container.AC3: return ".ac3";
                case Container.Unknown: return string.Empty;
                default:
                    throw new NotImplementedException($"Container {c} is unrecognized! Please file an issue at libvideo on GitHub.");
            }
        }
        public static void GetSupported(this Container c, out VideoFormat[] videos, out AudioFormat[] audios)
        {
            switch (c)
            {
                case Container.Flv: videos = new[] { VideoFormat.H264, VideoFormat.Flash }; audios = new[] { AudioFormat.Aac }; return;
                case Container._3Gp: videos = new[] { VideoFormat.H264, VideoFormat.Mobile }; audios = new[] { AudioFormat.Aac }; return;
                case Container.MP4: videos = new[] { VideoFormat.H264, VideoFormat.AV01 }; audios = new[] { AudioFormat.Aac, AudioFormat.AC3 }; return;
                case Container.WebM: videos = new[] { VideoFormat.H264, VideoFormat.VP9, VideoFormat.AV01 }; audios = new[] { AudioFormat.Aac, AudioFormat.AC3, AudioFormat.Vorbis, AudioFormat.Opus }; return;
                case Container.Mkv: videos = new[] { VideoFormat.H264, VideoFormat.VP9, VideoFormat.AV01 }; audios = new[] { AudioFormat.Aac, AudioFormat.AC3, AudioFormat.Vorbis, AudioFormat.Opus }; return;
                case Container.Mp3: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.Mp3 }; return;
                case Container.Aac: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.Aac }; return;
                case Container.Opus: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.Opus }; return;
                case Container.Ogg: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.Opus, AudioFormat.Vorbis }; return;
                case Container.AC3: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.AC3 }; return;
                case Container.Unknown: videos = new VideoFormat[] { }; audios = new AudioFormat[] { }; return;
                default:
                    throw new NotImplementedException($"Container {c} is unrecognized! Please file an issue at libvideo on GitHub.");
            }
        }
        public static bool Supports(this Container c, VideoFormat video)
        {
            c.GetSupported(out var vf, out var af);
            return vf.Contains(video);
        }
        public static bool Supports(this Container c, AudioFormat audio)
        {
            c.GetSupported(out var vf, out var af);
            return af.Contains(audio);
        }
    }
}
