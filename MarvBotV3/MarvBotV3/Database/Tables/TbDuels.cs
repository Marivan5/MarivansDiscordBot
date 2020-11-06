using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbDuels
    {
        [Key]
        public int ID { get; set; }
        public ulong Challenger { get; set; }
        public ulong Challenge { get; set; }
        public ulong Winner { get; set; }
        public int BetAmount { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
