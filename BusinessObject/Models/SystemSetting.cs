﻿using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class SystemSetting : BaseEntity
    {
        public string Name { get; set; }
        public SettingType SettingType { get; set; }
        public decimal? Value { get; set; }
        public string Description { get; set; }
    }
}