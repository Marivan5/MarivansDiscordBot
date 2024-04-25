using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MarvBotV3.BusinessLayer;
using MarvBotV3.Database;
using QRCoder;

namespace MarvBotV3
{
    public class PublicCommands : ModuleBase<CommandContext>
    {
        DataAccess da;
        MarvBotBusinessLayer bl;
        RiksbankenBusiness rb;

        public PublicCommands()
        {
            da = new DataAccess(new DatabaseContext());
            bl = new MarvBotBusinessLayer(da);
            rb = new RiksbankenBusiness(da);
        }

        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        [Alias("vem", "info")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user ??= Context.User;
            await ReplyAsync(user.ToString());
        }

        [Command("qrcode")]
        [Alias("qr")]
        public async Task GetQRCode(string data)
        {
            QRCodeData qrCodeData;
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            }
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);
            var stream = new MemoryStream(qrCodeImage);
            stream.Position = 0;
            await Context.Channel.SendFileAsync(stream, "qr-code.png");
        }

        [Command("temp")]
        [Alias("temperature", "grader")]
        public async Task GetTempData()
        {
            var tempData = await da.GetTempDataAsync();
            foreach (var data in tempData)
            {
                var reply = $"At {data.Time} it was {data.Temperature.ToString("0.00", Program.nfi)}C at {data.Room}";
                if(data.Humidity != 0)
                    reply += $" with a humidity of {data.Humidity.ToString("0.00", Program.nfi)}%";

                await ReplyAsync(reply);
            }
        }

        [Command("VideoChat")]
        [Alias("video")]
        public async Task VideoChannelInfo()
        {
            if(Program.serverConfig.videoChannel == 0)
                await ReplyAsync("Video channel is not set.");
            else
                await ReplyAsync("Video channel is set to: " + MentionUtils.MentionChannel(Program.serverConfig.videoChannel));
        }

        [Command("AFKChat")]
        [Alias("AFK")]
        public async Task AfkChannelInfo()
        {
            if (Program.serverConfig.videoChannel == 0)
                await ReplyAsync("AFK channel is not set.");
            else
                await ReplyAsync("AFK channel is set to: " + MentionUtils.MentionChannel(Program.serverConfig.afkChannel));
        }

        [Command("PublicChat")]
        [Alias("public")]
        public async Task PublicChannelInfo()
        {
            if (Program.serverConfig.publicChannel.Any())
            {
                await ReplyAsync("Public channel is not set.");
            }
            else
            {
                foreach (var channel in Program.serverConfig.publicChannel)
                    await ReplyAsync("Public channel is set to: " + MentionUtils.MentionChannel(channel));
            }
        }

        [Command("WhoWhiteList")]
        [Alias("whosSafe")]
        public async Task WhiteListInfo()
        {
            var whitelist = Program.serverConfig.whiteList;

            await ReplyAsync("Whitelist contains:");

            foreach (var user in whitelist)
                await ReplyAsync(MentionUtils.MentionUser(user));
        }

        [Command("fu"), Summary("FU!")]
        [Alias("fackyou")]
        public async Task FUReaction()
        {
            var msg = ((await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, 1).FlattenAsync()).FirstOrDefault() as IUserMessage);
            await Context.Message.DeleteAsync();

            if (msg == null)
            {
                await ReplyAsync("Cant find a message.");
                return;
            }

            var emojis = new IEmote[]
            {
               new Emoji("🇫"),
               new Emoji("🇦"),
               new Emoji("🇨"),
               new Emoji("🇰"),
               new Emoji("🖕"),
               new Emoji("🇾"),
               new Emoji("🇴"),
               new Emoji("🇺"),
            };

            try
            {
                await msg.AddReactionsAsync(emojis);
            }
            catch
            {
                await ReplyAsync("Something went wrong.");
            }
        }

        [Command("Dice")]
        [Alias("roll")]
        public async Task DiceRoll(int lowValue = 1, int highValue = 6, int amount = 1)
        {
            if(amount > 20)
            {
                await ReplyAsync("Amount cant be more than 20");
                amount = 20;
            }

            if (lowValue > highValue)
            {
                var tempValue = highValue;
                highValue = lowValue;
                lowValue = tempValue;
            }

            highValue += 1;
            var rng = new Random();
            string reply = "";

            for (int i = 0; i < amount; i++)
            {
                reply += "Roll " + (i+1).ToString() + ": " + rng.Next(lowValue, highValue) + Environment.NewLine;
            }

            await ReplyAsync(reply);
        }

        [Command("ChangeNickname")]
        [Alias("nick", "changename", "changenick", "nickname")]
        public async Task ChangeNickname(IUser user, params string[] nickname)
        {
            const int costToChangeNick = 100;
            var richBitch = Context.Guild.GetUsersAsync().Result.First(x => x.RoleIds.ToList().Contains(762789255965048833));
            if (richBitch.Id != Context.User.Id)
            {
                await ReplyAsync($"Only {MentionUtils.MentionRole(762789255965048833)} can change nicknames");
                return;
            }
            if (user == null)
            {
                await ReplyAsync("Type '!ChangeNickname *User* *newNickname*'");
                return;
            }

            var currentGold = await da.GetGold(Context.User.Id);
            if (currentGold < costToChangeNick)
            {
                await ReplyAsync($"It costs {costToChangeNick} Gold to change someones nickname, you only have {currentGold}");
                return;
            }

            var nick = "";

            foreach (var s in nickname)
                nick += $"{s} ";

            await bl.SaveGold(Context.User, Context.Guild, -costToChangeNick);
            await Context.Guild.GetUserAsync(user.Id).Result.ModifyAsync(x => x.Nickname = nick.Trim());
        }

        [Command("Help")]
        [Alias("hjälp")]
        public async Task HelpCommand()
        {
            var commands = CommandHandler._commands.Commands.ToList();
            var embedBuilder = new EmbedBuilder();

            foreach (var command in commands)
            {
                // Get the command Summary attribute information
                //string embedFieldText = command.Summary ?? "No description available\n";
                if (command.Summary != null)
                    embedBuilder.AddField($"{command.Name}", command.Summary);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }

        [Command("Birthday")]
        [Alias("Bday")]
        public async Task GetBirthday(IUser user = null)
        {
            if (user == null)
                user = Context.User;
            var birthday = await da.GetBirthday(user);
            if (birthday == null)
            {
                await ReplyAsync("User does not have a registred birthday");
                return;
            }
            await ReplyAsync(BirthdayReply(birthday.UserID, birthday.Birthday));
        }
        
        [Command("Birthdays")]
        [Alias("Bdays", "GetBirthdays", "AllBirthdays")]
        public async Task GetBirthdays()
        {
            var birthdays = await da.GetBirthdays().Pipe(x => x.OrderBy(x => x.Birthday.Month).ThenBy(x => x.Birthday.Day));
            if (birthdays == null)
            {
                await ReplyAsync("No birthdays have been registred");
                return;
            }
            var reply = "";
            var order = birthdays.Where(x => x.Birthday.Month > DateTime.Today.Month || (x.Birthday.Month == DateTime.Today.Month && x.Birthday.Day >= DateTime.Today.Day)).OrderBy(x => x.Birthday.Month).ThenBy(x => x.Birthday.Day).ToList();
            order.AddRange(birthdays.Where(x => x.Birthday.Month < DateTime.Today.Month || (x.Birthday.Month == DateTime.Today.Month && x.Birthday.Day < DateTime.Today.Day)).OrderBy(x => x.Birthday.Month).ThenBy(x => x.Birthday.Day).ToList());

            foreach (var bday in order)
            {
                reply += BirthdayReply(bday.UserID, bday.Birthday);
            }
            await ReplyAsync(reply);
        }

        private string BirthdayReply(ulong userID, DateTime birthday) =>
            $"{MentionUtils.MentionUser(userID)} was born {birthday:yyyy-MM-dd}. " +
            $"They are {bl.CalculateYourAge(birthday)} old. " +
            $"{bl.CalculateDaysUntilNextDate(birthday)} days until next their birthday.{Environment.NewLine}";

        [Command("FreeDays")]
        [Alias("RedDays", "RödaDagar", "Holidays", "Holiday")]
        public async Task GetNotBankDaysThisYear(int year = 0)
        {
            if (year == 0)
                year = DateTime.Today.Year;

            var days = await rb.GetWeekDaysThatAreNotBankDays(year).Pipe(x => x.OrderBy(y => y.CalendarDate));
            var reply = "";
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var index = 1;

            foreach (var day in days)
            {
                var daysToGo = bl.CalculateDaysUntilNextDate(day.CalendarDate, false);
                string afterText = year == DateTime.Today.Year ? $"It is currently { Math.Abs(daysToGo) } days " + (daysToGo > 0 ? "left." : "ago.") : "";
                reply += $"{index}: {day.CalendarDate:dddd yyyy-MM-dd} is a holiday. " + afterText + Environment.NewLine;
                ++index;
            }

            await ReplyAsync(reply);
        }

        [Command("Pogday")]
        [Alias("Pogdag", "pog")]
        public async Task PogDay()
        {
            await ReplyAsync("https://www.twitch.tv/henkeohlsen");
        }
    }
}
