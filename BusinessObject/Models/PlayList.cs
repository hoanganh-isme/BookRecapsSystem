using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class PlayList : BaseEntity
    {
        public Guid UserId { get; set; }
        public string PlayListName { get; set;}
        public ICollection<PlayListItem> PlayListItems { get; set; }
        public User User { get; set; }
    }
}
