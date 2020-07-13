using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public abstract class Video
    {
        internal Video()
        {
        }

        public abstract string Uri { get; }
        public abstract string Title { get; }
        public abstract WebSites WebSite { get; }
        public virtual VideoFormat Format => VideoFormat.Unknown;
        public virtual AudioFormat AudioFormat => AudioFormat.Unknown;
        public virtual int Resolution => -1;
        public virtual int AudioBitrate => -1;
        public virtual bool Is3D => false;
        public bool IsAdaptive =>
            this.AdaptiveKind != AdaptiveKind.None;
        public virtual AdaptiveKind AdaptiveKind => AdaptiveKind.None;

        public virtual Task<string> GetUriAsync() =>
            Task.FromResult(Uri);

        public byte[] GetBytes() =>
            GetBytesAsync().GetAwaiter().GetResult();

        public async Task<byte[]> GetBytesAsync()
        {
            using (var client = new VideoClient())
            {
                return await client
                    .GetBytesAsync(this)
                    .ConfigureAwait(false);
            }
        }

        public Stream Stream() =>
            StreamAsync().GetAwaiter().GetResult();

        public async Task<Stream> StreamAsync()
        {
            using (var client = new VideoClient())
            {
                return await client
                    .StreamAsync(this)
                    .ConfigureAwait(false);
            }
        }

        public virtual string FileExtension => FileContainer.GetExtension();

        public virtual Container FileContainer
        {
            get
            {
                switch (Format)
                {
                    case VideoFormat.Flash: return Container.Flv;
                    case VideoFormat.Mobile: return Container._3Gp;
                    case VideoFormat.AV01:
                    case VideoFormat.H264: return Container.MP4;
                    case VideoFormat.VP9: return Container.WebM;
                    case VideoFormat.Unknown:
                        {
                            switch (AudioFormat)
                            {
                                case AudioFormat.Mp3:
                                    return Container.Mp3;
                                case AudioFormat.Aac:
                                    return Container.Aac;
                                case AudioFormat.Vorbis:
                                    return Container.Ogg;
                                case AudioFormat.Unknown:
                                    return Container.Unknown;
                                case AudioFormat.Opus:
                                    return Container.Opus;
                                case AudioFormat.AC3:
                                    return Container.AC3;
                                default:
                                    break;
                            }
                            throw new NotImplementedException($"Format {AudioFormat} is unrecognized! Please file an issue at libvideo on GitHub.");
                        }
                    default:
                        throw new NotImplementedException($"Format {Format} is unrecognized! Please file an issue at libvideo on GitHub.");
                }
            }
        }

        public string FullName
        {
            get
            {
                var builder =
                    new StringBuilder(Title)
                    .Append(FileExtension);

                foreach (char bad in Path.GetInvalidFileNameChars())
                    builder.Replace(bad, '_');

                return builder.ToString();
            }
        }
    }
}
