using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Highlight
{
    public class CreateHighlightRequest
    {
        public Guid RecapVersionId { get; set; }
        public Guid UserId { get; set; }
        public string? Note { get; set; }
        public string? TargetText { get; set; }
        public string? StartIndex { get; set; }
        public string? EndIndex { get; set; }
        public string? SentenceIndex { get; set; }
    }
}
