using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.KeyIdea
{
    public class PrepareIdea
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Image { get; set; }
        public int Order { get; set; }
        public Guid RecapVersionId {  get; set; }
    }
}
