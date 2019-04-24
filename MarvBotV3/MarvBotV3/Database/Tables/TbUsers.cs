using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbUsers
    {
        [Key]
        public ulong UserID { get; set; }
        public string UserName { get; set; }
    }
}
