using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class KeyIdea : BaseEntity
    {
        public Guid RecapVersionId { get; set; }
        public string? Title {  get; set; }
        public string? Body {  get; set; }
        public string? Image {  get; set; }
        public int Order { get; set; }
        public int ViewCount {  get; set; }
        public RecapVersion RecapVersion { get; set; }
        public ICollection<Highlight> Highlights { get; set; }
        public ICollection<ReadingPosition> ReadingPositions { get; set; }



    }
}
