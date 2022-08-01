using System;

namespace MarvBotV3.Dto
{
    public class RockPaperScissors
    {
        public long Id { get; set; }
        public ulong Challenger { get; set; }
        public ulong Challenge { get; set; }
        public int BetAmount { get; set; }
        public string ChallengerChoice { get; set; }
        public string ChallengeChoice { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
