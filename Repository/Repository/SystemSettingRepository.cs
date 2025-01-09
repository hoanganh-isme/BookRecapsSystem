using BusinessObject.Data;
using BusinessObject.Enums;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class SystemSettingRepository : BaseRepository<SystemSetting>, ISystemSettingRepository
    {
        public SystemSettingRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<decimal?> GetRevenueSharePercentageAsync()
        {
            var setting = await context.SystemSettings
                .Where(s => s.SettingType == SettingType.RevenueSharePercentage)
                .Select(s => s.Value)
                .FirstOrDefaultAsync();

            return setting;
        }
    }
}
