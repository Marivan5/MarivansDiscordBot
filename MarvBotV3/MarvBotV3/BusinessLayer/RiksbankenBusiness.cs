using MarvBotV3.Database;
using MarvBotV3.Database.Tables; // TODO: fix DTO instead of table modell
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MarvBotV3.BusinessLayer
{
    public class RiksbankenBusiness
    {
        readonly DataAccess _da;

        public RiksbankenBusiness(DataAccess da)
        {
            _da = da;
        }

        public async Task<List<TbCalendarDays>> GetCalendarDaysFromYear(int year)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var riksbankClient = new RiksbankenService.SweaWebServicePortTypeClient();

            riksbankClient.InnerChannel.OperationTimeout = new TimeSpan(0, 5, 0);
            var fromDate = new DateTime(year, 1, 1);
            var toDate = new DateTime(year + 1, 1, 1);
            var calendarDays = await riksbankClient.getCalendarDaysAsync(fromDate, toDate).Pipe(x => x.@return.ToList());

            var tbCalendarDays = new List<TbCalendarDays>();
            foreach (var item in calendarDays)
            {
                tbCalendarDays.Add(new TbCalendarDays
                {
                    BankDay = item.bankday.ToUpper() == "Y",
                    CalendarDate = (DateTime)item.caldate,
                    WeekNumber = Convert.ToInt32(item.week),
                    WeekYear = item.weekyear
                });
            }

            await _da.SetCalendarDays(tbCalendarDays);
            return tbCalendarDays;
        }

        public async Task<List<TbCalendarDays>> GetWeekDaysThatAreNotBankDays(int year)
        {
            var calendarDays = await _da.GetCalendarDaysForYear(year);
            if (calendarDays.Count <= 1)
                calendarDays = await GetCalendarDaysFromYear(year);

            return calendarDays.Where(x => x.CalendarDate.DayOfWeek != DayOfWeek.Sunday 
            && x.CalendarDate.DayOfWeek != DayOfWeek.Saturday 
            && !x.BankDay).ToList();
        }
    }
}
