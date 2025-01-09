using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Appeals
{
    public class UpdateAppealStatus
    {
        public Guid Id { get; set; }
        public AppealStatus AppealStatus { get; set; }
    }
}
