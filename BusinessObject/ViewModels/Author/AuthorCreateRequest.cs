﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Author
{
    public class AuthorCreateRequest
    {
        public string Name { get; set; }
        public string? Image { get; set; }
        public string Description { get; set; }
    }
}
