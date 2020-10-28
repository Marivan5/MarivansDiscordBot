using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbGoldGambles
    {
        [Key]
        public int ID { get; set; }
        public ulong UserID { get; set; }
        [MaxLength(64)]
        public string Username { get; set; }
        public bool Won { get; set; }
        public long BetAmount { get; set; }
        public long ChangeAmount { get; set; }
        public int Roll { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
