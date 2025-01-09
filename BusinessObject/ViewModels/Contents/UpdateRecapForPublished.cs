using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Recaps
{
    public class UpdateRecapForPublished
    {
        public Guid RecapId {  get; set; }
        public bool isPublished { get; set; }
        public bool isPremium { get; set; }
    }
}
