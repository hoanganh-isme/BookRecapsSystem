using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class ReviewNote : BaseEntity
    {
        public Guid ReviewId { get; set; }
        public string? TargetText {  get; set; }
        public string? StartIndex { get; set; }
        public string? EndIndex { get; set; }
        public string? SentenceIndex { get; set; }
        public string? Feedback {  get; set; }
        public Review Review { get; set; }
    }
}
