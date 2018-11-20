using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Writing_Pad.DomainModels
{
    public partial class HandWrittenNotes
    {
        public long Idx { get; set; }
        public string Deleted { get; set; }
        public string GroupKey { get; set; }
        public string Notes { get; set; }
        public long? RecognizedNoteID { get; set; }
    }
    public partial class RecognizedNotes
    {
        public long Idx { get; set; }
        public string Deleted { get; set; }
        public string Notes { get; set; }
    }
}
