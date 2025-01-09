using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Contents
{
    public class CreateRecapVersion
    {
        public Guid RecapId { get; set; }
        public string? VersionName { get; set; }
        public decimal? VersionNumber { get; set; }
        public Guid ContributorId { get; set; }
        public RecapStatus Status;
    }
}
