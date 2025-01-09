using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.ReviewNotes
{
    public class UpdateReviewNoteRequest
    {
        public Guid Id { get; set; }
        public string? TargetText { get; set; }
        public string? StartIndex { get; set; }
        public string? EndIndex { get; set; }
        public string? SentenceIndex { get; set; }
        public string? Feedback { get; set; }
    }
}
