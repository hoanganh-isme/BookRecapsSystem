using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Likes
{
    public class CreateLike
    {
        public DateTime LikeAt { get; set; }
        public Guid UserId { get; set; }
    }
}
