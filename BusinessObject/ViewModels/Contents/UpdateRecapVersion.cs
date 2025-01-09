using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Recaps
{
    public class UpdateRecapVersion
    {
        public string? VersionName { get; set; }
        public Guid RecapVersionId { get; set; }
        public string? AudioURL { get; set; }
        public bool isGenAudio { get; set; }
        public RecapStatus Status;
    }
}
