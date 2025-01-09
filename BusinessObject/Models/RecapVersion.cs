using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class RecapVersion : BaseEntity
    {
        public decimal? VersionNumber { get; set; }
        public string? VersionName {  get; set; }
        public string? AudioURL { get; set; }
        public string? TranscriptUrl {  get; set; }
        public bool isGenAudio { get; set; }
        public decimal? AudioLength { get; set; }
        public PlagiarismCheckStatus PlagiarismCheckStatus { get;set; }
        public TranscriptStatus TranscriptStatus { get; set; }
        public RecapStatus Status { get; set; }
        public Guid RecapId { get; set; }
        public Recap Recap { get; set; }
        public ICollection<KeyIdea> KeyIdeas { get; set; }
        public ICollection<ReadingPosition> ReadingPositions { get; set; }
        public ICollection<Highlight> Highlights { get; set; }
        public Review Review { get; set; }
    }      
}
