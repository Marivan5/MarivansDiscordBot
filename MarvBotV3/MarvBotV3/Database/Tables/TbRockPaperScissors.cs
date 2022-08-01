using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbRockPaperScissors
    {
        [Key]
        public int ID { get; set; }
        public ulong Challenger { get; set; }
        public ulong Challenge { get; set; }
        public ulong Winner { get; set; }
        public int BetAmount { get; set; }
        public string ChallengerChoice { get; set; }
        public string ChallengeChoice { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
