using System;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public partial class YouTubeVideo : Video
    {
        private readonly string jsPlayer;

        private SignatureQuery query;

        internal YouTubeVideo(string title,
            SignatureQuery query, string jsPlayer)
        {
            this.Title = title;
            this.query = query;
            this.jsPlayer = jsPlayer;
            this.FormatCode = int.Parse(query.Query["itag"]);
        }

        public override string Title { get; }
        public override WebSites WebSite => WebSites.YouTube;

        public override string Uri =>
            GetUriAsync().GetAwaiter().GetResult();

        public string GetUri(Func<DelegatingClient> makeClient) =>
            GetUriAsync(makeClient).GetAwaiter().GetResult();

        public override Task<string> GetUriAsync() =>
            GetUriAsync(() => new DelegatingClient());

        public async Task<string> GetUriAsync(Func<DelegatingClient> makeClient)
        {
            if (query.IsEncrypted)
            {
                await DecryptAsync(makeClient)
                    .ConfigureAwait(false);
                query.IsEncrypted = false;
            }

            return query.Uri;
        }

        public int FormatCode { get; }
        public bool IsEncrypted => query.IsEncrypted;
    }
}
