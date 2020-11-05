using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbDonations
    {
        [Key]
        public ulong UserID { get; set; }
        [MaxLength(64)]
        public string Username { get; set; }
        public ulong GuildID { get; set; }
        public int DonationAmount { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
