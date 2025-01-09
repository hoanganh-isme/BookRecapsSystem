using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.KeyIdea
{
    public class UpdateKeyIdeaRequest
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Image { get; set; }
        public int Order { get; set; }
        public bool RemoveImage { get; set; }
    }

}
