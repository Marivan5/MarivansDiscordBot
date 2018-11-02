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

        [Command("SetVideoChat")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task SetVideoChannel([Remainder] SocketGuildChannel _channel)
        {
            Program.EnsureServerConfigExists(_channel, "Video");
            await ReplyAsync(_channel + " was succefully added.");
        }

        [Command("SetPublicChat")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task SetPublicChannel([Remainder] SocketGuildChannel _channel)
        {
            Program.EnsureServerConfigExists(_channel, "Public");
            await ReplyAsync(_channel + " was succefully added.");
        }

        [Command("VideoChat")]
        [Alias("video")]
        public async Task VideoChannelInfo()
        {
            if(ServerConfig.Load().videoChannel == 0)
            {
                await ReplyAsync("Video channel is not set.");
            }
            else
            {
                await ReplyAsync("Video channel is set to: " + MentionUtils.MentionChannel(ServerConfig.Load().videoChannel));
            }
        }

        [Command("PublicChat")]
        [Alias("public")]
        public async Task PublicChannelInfo()
        {
            if (ServerConfig.Load().publicChannel == 0)
            {
                await ReplyAsync("Public channel is not set.");
            }
            else
            {
                await ReplyAsync("Public channel is set to: " + MentionUtils.MentionChannel(ServerConfig.Load().publicChannel));
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

            [Command("fu"), Summary("FU!")]
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

        //// Ban a user
        //[Command("ban")]
        //[RequireContext(ContextType.Guild)]
        //// make sure the user invoking the command can ban
        //[RequireUserPermission(GuildPermission.BanMembers)]
        //// make sure the bot itself can ban
        //[RequireBotPermission(GuildPermission.BanMembers)]
        //public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = null)
        //{
        //    await user.Guild.AddBanAsync(user, reason: reason);
        //    await ReplyAsync("ok!");
        //}

        //// [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        //[Command("echo")]
        //public Task EchoAsync([Remainder] string text)
        //    // Insert a ZWSP before the text to prevent triggering other bots!
        //    => ReplyAsync('\u200B' + text);

        //// 'params' will parse space-separated elements into a list
        //[Command("list")]
        //public Task ListAsync(params string[] objects)
        //    => ReplyAsync("You listed: " + string.Join("; ", objects));
    }
}
