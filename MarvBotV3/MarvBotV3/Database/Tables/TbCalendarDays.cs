using System;
using System.ComponentModel.DataAnnotations;

namespace MarvBotV3.Database.Tables
{
    public class TbCalendarDays
    {
        [Key]
        public DateTime CalendarDate { get; set; }
        public bool BankDay { get; set; }
        public int WeekNumber { get; set; }
        public int WeekYear { get; set; }
    }
}
