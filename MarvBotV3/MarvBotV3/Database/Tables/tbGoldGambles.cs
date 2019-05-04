using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class tbGoldGambles
    {
        [Key]
        public int ID { get; set; }
        public ulong UserID { get; set; }
        public string Username { get; set; }
        public bool Won { get; set; }
        public ulong Amount { get; set; }
        public int Roll { get; set; }
    }
}
