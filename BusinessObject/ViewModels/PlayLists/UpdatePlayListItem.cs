using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.PlayLists
{
    public class UpdatePlayListItem
    {
        public Guid PlayListId { get; set; }
        public Guid RecapId { get; set; }
        public int OrderPlayList { get; set; }
    }

}
