using System;

namespace MarvBotV3.Dto
{
    public class Duel
    {
        public long DuelId { get; set; }
        public ulong Challenger { get; set; }
        public ulong Challenge { get; set; }
        public int BetAmount { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
