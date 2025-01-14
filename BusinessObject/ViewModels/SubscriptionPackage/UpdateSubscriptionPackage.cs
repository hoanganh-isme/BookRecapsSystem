﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.SubscriptionPackage
{
    public class UpdateSubscriptionPackage
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Duration { get; set; }
        public string? Description { get; set; }
        public int? ExpectedViewsCount { get; set; }
    }
}
