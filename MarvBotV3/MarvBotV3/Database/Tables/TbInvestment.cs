using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbInvestment
    {
        [Key]
        public int ID { get; set; }
        public ulong UserID { get; set; }
        [MaxLength(64)]
        public string Username { get; set; }
        public long InvestAmount { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
