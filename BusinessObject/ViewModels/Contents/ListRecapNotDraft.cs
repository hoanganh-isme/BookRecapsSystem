using BusinessObject.Enums;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Contents
{
    public class ListRecapNotDraft
    {
        public string StaffName {  get; set; }
        public string VersionName { get; set; }
        public string RecapName {  get; set; }
        public string BookTitle {  get; set; }
        public string BookDescription { get; set; }
        public string ContributorName {  get; set; }
        public DateTime CreateAt { get; set; }
        public Guid? ReviewId { get; set; }
        public Guid RecapVersionId {  get; set; }
        public string Status { get; set; }
    }
}
