using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbCommandsLog
    {
        [Key]
        public ulong ID { get; set; }
        public ulong UserID { get; set; }
        [MaxLength(64)]
        public string Username { get; set; }
        public string Command { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
