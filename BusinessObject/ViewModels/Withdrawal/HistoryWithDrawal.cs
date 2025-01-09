using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Withdrawal
{
    public class HistoryWithDrawal
    {
        public decimal TotalEarning {  get; set; }
        public decimal Withdrawal {  get; set; }
        public int? NumberOfWithdrawals { get; set; }
        public string? BankAccount { get; set; }
        public List<ContributorWithdrawalDTO> Contributors { get; set; }
    }
}
