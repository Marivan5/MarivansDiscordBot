using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbTempData
    {
        [Key]
        public ulong Id { get; set; }
        public string Room { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float Altitude { get; set; }
        public float AirPressure { get; set; }
        public DateTime Time { get; set; }
    }
}
