using BusinessObject.Enums;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Appeals
{
    public class UpdateAppealResponse
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public string Response { get; set; }
    }
}
