using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Recaps
{
    public class CreateRecapRequest
    {
        public string? Name { get; set; }
        public bool isPublished { get; set; }
        public bool isPremium { get; set; }
        public Guid BookId { get; set; }
        public Guid ContributorId { get; set; }
    }
}
