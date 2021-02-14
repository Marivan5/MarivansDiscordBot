using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbBirthdays
    {
        [Key]
        public ulong UserID { get; set; }
        [MaxLength(64)]
        public string Username { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime LastGiftGiven { get; set; }
    }
}
