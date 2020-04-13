using System;

namespace MarvBotV3.DTO
{
    public class Duel
    {
        public ulong Challenger { get; set; }
        public ulong Challenge { get; set; }
        public int BetAmount { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
