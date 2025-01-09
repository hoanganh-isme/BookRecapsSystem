using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class ContractAttachment : BaseEntity
    {
        public string? Name {  get; set; }
        public string AttachmentURL {  get; set; }
        public Guid? ContractId {  get; set; }
        public Contract? Contract { get; set; }

    }
}
