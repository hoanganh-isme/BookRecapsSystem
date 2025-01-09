using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.PlayLists
{
    public class CreatePlayList
    {
        public Guid UserId { get; set; }
        public string PlayListName { get; set; }
    }
}
