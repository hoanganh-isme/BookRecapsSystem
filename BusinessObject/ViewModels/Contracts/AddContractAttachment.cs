﻿using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Contracts
{
    public class AddContractAttachment
    {
        public ICollection<Guid> AttachmentIds { get; set; }
    }


}
