using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbCurrency
    {
        [Key]
        public ulong UserID { get; set; }
        public string Username { get; set; }
        public long GoldAmount { get; set; }
        public ulong GuildID { get; set; }
    }
}
