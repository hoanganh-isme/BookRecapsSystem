using BusinessObject.Enums;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Appeals
{
    public class CreateAppealRequest
    {
        public Guid ReviewId {  get; set; }
        public Guid ContributorId { get; set; }
        public string Reason { get; set; }
    }
}
