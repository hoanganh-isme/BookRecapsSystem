﻿using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IRecapEarningRepository : IBaseRepository<RecapEarning>
    {
        Task<RecapEarning> GetRecapEarningByRecapId(Guid recapId);
    }
}