using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MarvBotV3
{
    public class PublicCommands : ModuleBase<SocketCommandContext>
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
        [Alias("public")]
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
            await Context.Message.DeleteAsync();

            var msg = CommandHandler.lastNotCommand;
            if (msg == null)
            {
                await Context.Channel.SendMessageAsync("I dont have a message cached.");
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
    }

    [RequireOwner]
    public class AdminModules : ModuleBase
    {
        [Command("clear"), Summary("Deletes X amount of messages")]
        [Alias("delete", "prune", "purge")]
        public async Task ClearMessages([Summary("Clear X amount of messages")] int amountToClear = 1)
        {
            if (amountToClear < 100)
            {
                ++amountToClear;
            }
            else if (amountToClear > 100)
            {
                await Context.Channel.SendMessageAsync("Can only delete 100 messages.");
            }
            await Context.Channel.DeleteMessagesAsync(await Context.Channel.GetMessagesAsync(amountToClear).Flatten());
        }

        [Command("clearcon"), Summary("Deletes X amount of messages containing the param")]
        [Alias("deletecon")]
        public async Task ClearMessagesContaining(string param, int amountToClear = 1)
        {
            if (amountToClear < 100)
            {
                ++amountToClear;
            }
            else if (amountToClear > 100)
            {
                await Context.Channel.SendMessageAsync("Can only delete 100 messages.");
            }

            var toCheck = await Context.Channel.GetMessagesAsync(amountToClear).Flatten();
            List<IMessage> toDelete = new List<IMessage>();

            foreach (var msg in toCheck)
            {
                if(msg.Content.ToString().Contains(param))
                {
                    toDelete.Add(msg);
                }
            }
            await Context.Channel.DeleteMessagesAsync(toDelete);
        }

        [Command("SetVideoChat")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task SetVideoChannel([Remainder] SocketGuildChannel _channel)
        {
            Program.serverConfig.videoChannel = _channel.Id;
            Program.serverConfig.Save();
            await ReplyAsync(_channel + " was succefully added.");
        }

        [Command("SetPublicChat")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task SetPublicChannel([Remainder] SocketGuildChannel _channel)
        {
            Program.serverConfig.publicChannel = _channel.Id;
            Program.serverConfig.Save();
            await ReplyAsync(_channel + " was succefully added.");
        }

        [Command("SetAFKChat")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task SetAFKChannel([Remainder] SocketGuildChannel _channel)
        {
            Program.serverConfig.afkChannel = _channel.Id;
            Program.serverConfig.Save();
            await ReplyAsync(_channel + " was succefully added.");
        }

        [Command("Whitelist")]
        [Alias("AddToWhitelist")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AddToWhiteList([Remainder] SocketGuildUser user)
        {
            Program.serverConfig.whiteList.Add(user.Id);
            Program.serverConfig.Save();
            await ReplyAsync(MentionUtils.MentionUser(user.Id) + " was succefully added to the whitelist.");
        }

        [Command("RemoveWhitelist")]
        [Alias("RemoveFromWhiteList")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task RemoveFromWhiteList([Remainder] SocketGuildUser user)
        {
            Program.serverConfig.whiteList.Remove(user.Id);
            Program.serverConfig.Save();
            await ReplyAsync(MentionUtils.MentionUser(user.Id) + " was succefully removed to the whitelist.");
        }

        [Command("VideoList")]
        [Alias("AddToVideoList")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AddToVideoList([Remainder] string link)
        {
            Program.serverConfig.videoList.Add(link);
            Program.serverConfig.Save();
            await ReplyAsync(link + " was succefully added to the video channel list.");
        }

        [Command("RemoveVideoList")]
        [Alias("RemoveFromVideoList")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task RemoveFromVideoList([Remainder] string link)
        {
            Program.serverConfig.videoList.Remove(link);
            Program.serverConfig.Save();
            await ReplyAsync(link + " was succefully removed to the video channel list.");
        }
    }
}
