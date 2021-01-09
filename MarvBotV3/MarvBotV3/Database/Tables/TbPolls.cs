using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbPolls
    {
        [Key]
        public long ID { get; set; }
        public ulong CreatorUserID { get; set; }
        public string Name { get; set; }
        public DateTime CreatedTimeStamp { get; set; }
        public bool? Result { get; set; }
        public DateTime? ResultTimeStamp { get; set; }
    }
}
