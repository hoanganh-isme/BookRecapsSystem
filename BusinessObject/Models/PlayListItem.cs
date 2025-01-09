using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class PlayListItem : BaseEntity
    {
        public Guid RecapId { get; set; }
        public Guid PlayListId {  get; set; }
        public int OrderPlayList {  get; set; }
        public Recap Recap { get; set; }
        public PlayList PlayList { get; set; }
    }
}
