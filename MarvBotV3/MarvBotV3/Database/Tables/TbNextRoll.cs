using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbNextRoll
    {
        [Key]
        public ulong ID { get; set; }
        public ulong UserID { get; set; }
        [MaxLength(64)]
        public string Username { get; set; }
        public int NextRoll { get; set; }
        public int AmountOver { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
