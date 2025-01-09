using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Review
{
    public class UpdateReviewRequest
    {
        public Guid Id { get; set; }
        public Guid RecapVersionId { get; set; }
        public string? Comments { get; set; }
    }
}
