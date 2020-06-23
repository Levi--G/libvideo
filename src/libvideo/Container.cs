using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public enum Container
    {
        Flv,
        _3Gp,
        MP4,
        WebM,
        Mkv,
        Mp3,
        Aac,
        Opus,
        SB0,
        AC3,
        Unknown
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
                case Container.Mp3:
                    return ".mp3";
                case Container.Aac:
                    return ".aac";
                case Container.Opus:
                    return ".opus";
                case Container.SB0:
                    return ".sb0";
                case Container.AC3:
                    return ".ac3";
                case Container.Unknown: return string.Empty;
                default:
                    throw new NotImplementedException($"Container {c} is unrecognized! Please file an issue at libvideo on GitHub.");
            }
        }
    }
}
