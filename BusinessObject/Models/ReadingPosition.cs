using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class ReadingPosition : BaseEntity
    {
        public Guid RecapVersionId { get; set; }
        public Guid UserId { get; set; }
        public int SentenceIndex {  get; set; }
        public decimal CompletedRate { get; set; }
        public RecapVersion RecapVersions { get; set; }
        public User User { get; set; }
    }
}
