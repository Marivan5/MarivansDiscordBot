using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbUserActivity
    {
        [Key]
        public int ID { get; set; }
        public ulong UserID { get; set; }
        [MaxLength(64)]
        public string Username { get; set; }
        [MaxLength(200)]
        public string BeforeActivity { get; set; }
        [MaxLength(200)]
        public string AfterActivity { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
