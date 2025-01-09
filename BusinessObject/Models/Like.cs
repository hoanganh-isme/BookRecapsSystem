using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Like : BaseEntity
    {
        public DateTime LikeAt { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid RecapId { get; set; }
        public Recap Recap { get; set; }
    }
}
