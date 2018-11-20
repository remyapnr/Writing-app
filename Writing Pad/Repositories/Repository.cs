using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Writing_Pad.DomainModels;
using Writing_Pad.Utility;
using Writing_Pad.ViewModels;

namespace Writing_Pad.Repositories
{
    public class Repository
    {
        private QAContext ctx;
        public Repository(DbContext ctx)
        {
            this.ctx = (QAContext)ctx;
        }
        public void SaveHandWrittenNotes(HandWritingNoteDTO notes)
        {
            try
            {
                var note = new HandWrittenNotes();
                note.Notes = notes.Notes;
                note.GroupKey = GroupKeyGenerator();
                ctx.HandWrittenNotes.Add(note);
                ctx.SaveChanges();

            }
            catch (Exception ex)
            {
                ;
            }
        }
        public void SaveNotes(string notes)
        {
            try
            {
                var note = new RecognizedNotes();
                note.Notes = notes;
                ctx.RecognizedNotes.Add(note);
                ctx.SaveChanges();

            }
            catch (Exception ex)
            {
                ;
            }
        }
       

        public List<HandWritingNoteDTO> GetHandWrittenNotes()
        {
            return ctx.HandWrittenNotes.Select(x => new HandWritingNoteDTO { Idx = x.Idx, Notes = x.Notes, GroupKey = x.GroupKey }).ToList();

        }
        public string GetRecognizedNotes()
        {
            var notes=  ctx.RecognizedNotes.Select(x => x.Notes).ToList();
            return string.Join(" ", notes);
        }
        public string GroupKeyGenerator()
        {
            var newkey = AppConstants.GroupKey + 1;
            AppConstants.GroupKey = newkey;
            return "Note" + newkey;
        }
        public string GetHandWritingNote(string groupKey)
        {
            return ctx.HandWrittenNotes.Where(x => x.GroupKey.Trim() == groupKey.Trim()).Select(x => x.Notes).FirstOrDefault();
        }
        public List<string> GetHandWritingNote()
        {
            return ctx.HandWrittenNotes.Select(x => x.Notes).ToList();
        }
    }
}
