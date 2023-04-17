using Discord.Commands;
using Discord.WebSocket;
using MarvBotV3.Database;
using MarvBotV3.Database.Tables; 
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MarvBotV3.BusinessLayer
{
    public class UserCountModule
    {
        private readonly DiscordSocketClient _client;
        private int _userCount;
        private Timer _timer;
        private int averageUser = 3;
        private double factorUser;
        private int[] usersDuringDay = new int[13];
        private double[] weeklyFactor = new double[7];
        private double sumWeeklyFactor;
        public UserCountModule(DiscordSocketClient client)
        {
            _client = client;
            _client.Ready += OnReady;
        }
        private async Task OnReady()
        {
            var now = DateTime.Now.TimeOfDay;
            var startTime = new TimeSpan(15, 0, 0);
            var endTime = new TimeSpan(4, 0, 0);

            if (now >= startTime || now < endTime)
            {

                var channel = _client.GetChannel(CHANNEL_ID) as SocketVoiceChannel;
                if (channel != null)
                {
                    _userCount = channel.Users.Count;
                    _timer = new Timer(GetUserCount, null, TimeSpan.Zero, TimeSpan.FromHours(1));
                }
            }
        }
        private void GetUserCount(object state)
        {
            var channel = _client.GetChannel(CHANNEL_ID) as SocketVoiceChannel;
            if (channel != null)
            {
                int userCount = channel.Users.Count;
                if (userCount != _userCount)
                {
                    _userCount = userCount;
                }
            }
        }
        private void UpdateUserCount(object state) 
        { 
            /* Calculate amount of users during the day (active day 15:00 - 04:00) */
            var channel = _client.GetChannel(CHANNEL_ID) as SocketVoiceChannel;
            DateTime now = DateTime.Now;
            usersDuringDay[now.Hour] = channel.Users.Count;

            double avgUsersDuringDay = usersDuringDay.Average();

            factorUser = avgUsersDuringDay / averageUser;

            /* Calculate amount of users during the week (mon - sun)*/
            int currentDay = (int)DateTime.Now.DayOfWeek;
            int currentDayOld = 0;

            if (currentDay != currentDayOld)
            {
                weeklyFactor[currentDay] = factorUser;
                currentDay = currentDayOld;
            }
            sumWeeklyFactor = weeklyFactor.Average();
        }

    }

    private const ulong CHANNEL_ID = 158194839215669248;

}
