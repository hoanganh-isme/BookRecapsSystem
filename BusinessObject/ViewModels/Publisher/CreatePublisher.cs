using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Publisher
{
    public class CreatePublisher
    {
        public Guid UserId { get; set; }
        public string? ContactInfo { get; set; }
    }
}
