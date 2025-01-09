using BusinessObject.Data;
using BusinessObject.Models;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class ReadingPositionRepository : BaseRepository<ReadingPosition>, IReadingPositionRepository
    {
        public ReadingPositionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
