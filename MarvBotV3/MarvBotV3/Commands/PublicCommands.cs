﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MarvBotV3
{
    public class PublicCommands : ModuleBase<ShardedCommandContext>
    {
        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        [Alias("vem", "info")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;
            await ReplyAsync(user.ToString());
        }

        [Command("VideoChat")]
        [Alias("video")]
        public async Task VideoChannelInfo()
        {
            if(Program.serverConfig.videoChannel == 0)
            {
                await ReplyAsync("Video channel is not set.");
            }
            else
            {
                await ReplyAsync("Video channel is set to: " + MentionUtils.MentionChannel(Program.serverConfig.videoChannel));
            }
        }

        [Command("AFKChat")]
        [Alias("AFK")]
        public async Task AfkChannelInfo()
        {
            if (Program.serverConfig.videoChannel == 0)
            {
                await ReplyAsync("AFK channel is not set.");
            }
            else
            {
                await ReplyAsync("AFK channel is set to: " + MentionUtils.MentionChannel(Program.serverConfig.afkChannel));
            }
        }

        [Command("PublicChat")]
        [Alias("public")]
        public async Task PublicChannelInfo()
        {
            if (Program.serverConfig.publicChannel == 0)
            {
                await ReplyAsync("Public channel is not set.");
            }
            else
            {
                await ReplyAsync("Public channel is set to: " + MentionUtils.MentionChannel(Program.serverConfig.publicChannel));
            }
        }

        [Command("WhoWhiteList")]
        [Alias("whosSafe")]
        public async Task WhiteListInfo()
        {
            var whitelist = Program.serverConfig.whiteList;

            await ReplyAsync("Whitelist contains:");

            foreach (var user in whitelist)
            {
                await ReplyAsync(MentionUtils.MentionUser(user));
            }
        }

        [Command("fu"), Summary("FU!")]
        [Alias("fackyou")]
        public async Task FUReaction()
        {
            var msg = ((await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, 1).FlattenAsync()).FirstOrDefault() as IUserMessage);
            await Context.Message.DeleteAsync();

            if (msg == null)
            {
                await Context.Channel.SendMessageAsync("Cant find a message.");
                return;
            }

            const int delay = 1010; // Keep it just above a second so that i dont get hit with a preemtive rate limit

            Emoji Femoji = new Emoji("🇫");
            Emoji Aemoji = new Emoji("🇦");
            Emoji Cemoji = new Emoji("🇨");
            Emoji Kemoji = new Emoji("🇰");
            Emoji Yemoji = new Emoji("🇾");
            Emoji Oemoji = new Emoji("🇴");
            Emoji Uemoji = new Emoji("🇺");
            Emoji MFemoji = new Emoji("🖕");
            await msg.AddReactionAsync(Femoji);
            await Task.Delay(delay);
            await msg.AddReactionAsync(Aemoji);
            await Task.Delay(delay);
            await msg.AddReactionAsync(Cemoji);
            await Task.Delay(delay);
            await msg.AddReactionAsync(Kemoji);
            await Task.Delay(delay);
            await msg.AddReactionAsync(MFemoji);
            await Task.Delay(delay);
            await msg.AddReactionAsync(Yemoji);
            await Task.Delay(delay);
            await msg.AddReactionAsync(Oemoji);
            await Task.Delay(delay);
            await msg.AddReactionAsync(Uemoji);
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

            highValue = highValue + 1;
            var rng = new Random();
            string reply = "";

            for (int i = 0; i < amount; i++)
            {
                reply += "Roll " + (i+1).ToString() + ": " + rng.Next(lowValue, highValue) + Environment.NewLine;
            }

            await ReplyAsync(reply);
        }
    }
}