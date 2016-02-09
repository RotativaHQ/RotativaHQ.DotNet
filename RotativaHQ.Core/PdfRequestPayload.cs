using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotativaHQ.Core
{
    [ProtoContract]
    public class PdfRequestPayload
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        public string Filename { get; set; }

        [ProtoMember(3)]
        public string Switches { get; set; }

        [ProtoMember(4)]
        public byte[] ZippedHtmlPage { get; set; }
    }
}
