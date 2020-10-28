using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbCurrency
    {
        [Key]
        public ulong UserID { get; set; }
        [MaxLength(64)]
        public string Username { get; set; }
        public long GoldAmount { get; set; }
        public ulong GuildID { get; set; }
        public int AmountOfGambles { get; set; }
    }
}
