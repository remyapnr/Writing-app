using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Writing_Pad.DomainModels;
using Writing_Pad.Repositories;

namespace Writing_Pad.ViewModels
{
    public class HandWritingNoteDTO
    {
        public long Idx { get; set; }
        public string GroupKey { get; set; }
        public string Notes { get; set; }
        public int OrginialNoteID { get; set; }
    }
    internal class HandWritingRecognitionVM
    {

        public void SaveHandwritingNotes(string notes)
        {
            using (QAContext context = new QAContext())
            {
                var rep = new Repository(context);
                var note = new HandWritingNoteDTO { Notes = notes };
                rep.SaveHandWrittenNotes(note);
            }

        }

        public void SaveRecognizedNotes(string notes)
        {
            using (QAContext context = new QAContext())
            {
                var rep = new Repository(context);
                rep.SaveNotes(notes);
            }

        }
        public string GetHandWritingNotes(string groupKey)
        {
            using (QAContext context = new QAContext())
            {
                var rep = new Repository(context);
                return rep.GetHandWritingNote(groupKey);
            }
        }

        public List<string> GetAllHandWritingNote()
        {
            using (QAContext context = new QAContext())
            {
                var rep = new Repository(context);
                return rep.GetHandWritingNote();
            }
        }
        public List<HandWritingNoteDTO> GetHandWritingNotes()
        {
            using (QAContext context = new QAContext())
            {
                var rep = new Repository(context);
                return rep.GetHandWrittenNotes();
            }
        }

        public string GetRecognizedNotes()
        {
            using (QAContext context = new QAContext())
            {
                var rep = new Repository(context);
                return rep.GetRecognizedNotes();
            }
        }
    }
}
