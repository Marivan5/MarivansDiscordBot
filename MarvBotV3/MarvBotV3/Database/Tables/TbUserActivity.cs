using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbUserActivity
    {
        [Key]
        public int ID { get; set; }
        public ulong UserID { get; set; }
        public string Username { get; set; }
        public string BeforeActivity { get; set; }
        public string AfterActivity { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
