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
        AC3,
        M4a
    }

    public static class Extension
    {
        static (Container, string)[] Containers => new[] {
            (Container.Flv, ".flv"),
            (Container._3Gp, ".3gp"),
            (Container.MP4, ".mp4"),
            (Container.WebM, ".webm"),
            (Container.Mkv, ".mkv"),
            (Container.Mp3, ".mp3"),
            (Container.Aac, ".aac"),
            (Container.Opus, ".opus"),
            (Container.Ogg, ".ogg"),
            (Container.AC3, ".ac3"),
            (Container.M4a, ".m4a")
        };

        public static bool TryGetContainer(string fileExt, out Container container)
        {
            container = Container.Unknown;
            if (string.IsNullOrEmpty(fileExt))
            {
                return false;
            }
            if (!fileExt.StartsWith("."))
            {
                fileExt = $".{fileExt}";
            }
            container = Containers.FirstOrDefault(t => string.Equals(t.Item2, fileExt, StringComparison.OrdinalIgnoreCase)).Item1;
            return container != Container.Unknown;
        }

        public static bool TryGetExtension(Container container, out string fileExt)
        {
            fileExt = null;
            if (container == Container.Unknown)
            {
                return false;
            }
            fileExt = Containers.FirstOrDefault(t => t.Item1 == container).Item2;
            return fileExt != null;
        }

        public static string GetExtension(this Container c)
        {
            if (TryGetExtension(c, out var s))
            {
                return s;
            }
            throw new NotImplementedException($"Container {c} is unrecognized! Please file an issue at libvideo on GitHub.");
        }

        public static Container GetContainer(string ext)
        {
            if (TryGetContainer(ext, out var c))
            {
                return c;
            }
            throw new NotImplementedException($"Extension {c} is unrecognized! Please file an issue at libvideo on GitHub.");
        }

        public static void GetSupported(this Container c, out VideoFormat[] videos, out AudioFormat[] audios)
        {
            switch (c)
            {
                case Container.Flv: videos = new[] { VideoFormat.H264, VideoFormat.Flash }; audios = new[] { AudioFormat.Aac }; return;
                case Container._3Gp: videos = new[] { VideoFormat.H264, VideoFormat.Mobile }; audios = new[] { AudioFormat.Aac }; return;
                case Container.MP4: videos = new[] { VideoFormat.H264, VideoFormat.AV01 }; audios = new[] { AudioFormat.Aac, AudioFormat.AC3, AudioFormat.Mp3 }; return;
                case Container.WebM: videos = new[] { VideoFormat.H264, VideoFormat.VP9, VideoFormat.AV01 }; audios = new[] { AudioFormat.Aac, AudioFormat.AC3, AudioFormat.Vorbis, AudioFormat.Opus }; return;
                case Container.Mkv: videos = new[] { VideoFormat.H264, VideoFormat.VP9, VideoFormat.AV01 }; audios = new[] { AudioFormat.Aac, AudioFormat.AC3, AudioFormat.Vorbis, AudioFormat.Opus }; return;
                case Container.Mp3: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.Mp3 }; return;
                case Container.Aac: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.Aac }; return;
                case Container.Opus: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.Opus }; return;
                case Container.Ogg: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.Opus, AudioFormat.Vorbis }; return;
                case Container.AC3: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.AC3 }; return;
                case Container.M4a: videos = new VideoFormat[] { }; audios = new[] { AudioFormat.Aac, AudioFormat.AC3, AudioFormat.Mp3 }; return;
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
