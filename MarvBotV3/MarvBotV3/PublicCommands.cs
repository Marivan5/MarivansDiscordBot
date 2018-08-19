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
            Program.EnsureServerConfigExists(_channel);
            await ReplyAsync(_channel + " was succefully added.");
        }

        [Command("VideoChat")]
        public async Task VideoChannelInfo()
        {
            if(ServerConfig.Load().videoChannel == 0)
            {
                await ReplyAsync("Videos channel is not set.");
            }
            else
            {
                await ReplyAsync("Videos channel is set to: " + MentionUtils.MentionChannel(ServerConfig.Load().videoChannel));
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
