using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Withdrawal
{
    public class ProcessWithdrawal
    {
        public string? Description { get; set; }
        public WithdrawalStatus Status { get; set; }
    }
}
