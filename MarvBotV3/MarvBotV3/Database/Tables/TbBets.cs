using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbBets
    {
        [Key]
        public long ID { get; set; }
        public long PollID { get; set; }
        public ulong UserID { get; set; }
        public bool Bet { get; set; }
        public int BetAmount { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
