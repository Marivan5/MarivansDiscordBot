using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarvBotV3.Commands
{
    [RequireOwner]
    public class AdminModules : ModuleBase<ShardedCommandContext>
    {
        [Command("clear"), Summary("Deletes X amount of messages")]
        [Alias("delete", "prune", "purge")]
        public async Task ClearMessages([Summary("Clear X amount of messages")] int amountToClear = 1)
        {
            if (amountToClear > 100)
            {
                amountToClear = 100;
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amountToClear).FlattenAsync();
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
            await Context.Channel.DeleteMessageAsync(Context.Message);
        }

        [Command("clearcon"), Summary("Deletes X amount of messages containing the param")]
        [Alias("deletecon")]
        public async Task ClearMessagesContaining(string param, int amountToClear = 1)
        {
            if (amountToClear > 100)
            {
                amountToClear = 100;
            }

            var toCheck = await (Context.Channel as ITextChannel).GetMessagesAsync(amountToClear).FlattenAsync();
            List<IMessage> toDelete = new List<IMessage>();

            foreach (var msg in toCheck)
            {
                if (msg.Content.ToString().Contains(param))
                {
                    toDelete.Add(msg);
                }
            }
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(toDelete);
        }

        [Command("clearfrom"), Summary("Deletes X amount of messages from a user")]
        [Alias("deletefrom")]
        public async Task ClearMessagesFrom(SocketUser param, int amountToClear = 1)
        {
            if (amountToClear > 100)
            {
                amountToClear = 100;
            }

            var toCheck = await (Context.Channel as ITextChannel).GetMessagesAsync(amountToClear).FlattenAsync();
            List<IMessage> toDelete = new List<IMessage>();

            foreach (var msg in toCheck)
            {
                if (msg.Author.Id == param.Id)
                {
                    toDelete.Add(msg);
                }
            }
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(toDelete);
        }

        [Command("nextroll"), Summary("Set next roll")]
        [Alias("nextgamble", "next")]
        public async Task NextRoll(params int[] list)
        {
            Program.nextRolls.AddRange(list);
            await ReplyAsync($"Added {list}");
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
        
        [Command("SetRichRole")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task SetRichRole([Remainder] SocketRole _role)
        {
            Program.serverConfig.richRole = _role.Id;
            Program.serverConfig.Save();
            await ReplyAsync(_role + " was succefully added.");
        }

        [Command("Whitelist")]
        [Alias("AddToWhitelist", "white")]
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
        [Alias("RemoveFromWhiteList", "black")]
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
