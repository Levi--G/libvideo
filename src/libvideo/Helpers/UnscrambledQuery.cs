using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    internal struct SignatureQuery
    {
        public SignatureQuery(
            string uri, string signaturekey, bool encrypted)
        {
            this.Query = new Query(uri);
            this.Signaturekey = signaturekey ?? "signature";
            this.IsEncrypted = encrypted;
        }

        public string Uri => Query.ToString();
        public string Signaturekey { get; }
        public bool IsEncrypted { get; set; }

        public Query Query { get; }

        public string Signature { get => !Query.ContainsKey(Signaturekey) ? null : Query[Signaturekey]; set => Query[Signaturekey] = value; }
    }
}
