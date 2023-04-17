using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Audio;
using VideoLibrary;
using NAudio.Wave;
using Discord;
using System.Diagnostics;
using System.IO;

namespace MarvBotV3.Commands
{
    public class MusicModule : ModuleBase<CommandContext>
    {
        private async Task PlayAudioAsync(IAudioClient client, string path)
        {
            using (var output = CreateStream(path).StandardOutput.BaseStream)
            using (var stream = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {
                    await output.CopyToAsync(stream);
                }
                finally
                {
                    await stream.FlushAsync();
                }
            }
        }

        private static Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{path}\" -ac 2 -f s16le -ar 48000 -vn pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("You must be in a voice channel or provide one.");
                return;
            }

            var audioClient = await channel.ConnectAsync();
            await ReplyAsync($"Connected to {channel.Name}.");
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveChannel()
        {
            if ((Context.User as IGuildUser)?.VoiceChannel == null)
            {
                await ReplyAsync("You must be in a voice channel.");
                return;
            }

            await (Context.Guild as SocketGuild).AudioClient?.StopAsync();
            await ReplyAsync("Disconnected.");
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayAsync(string url)
        {
            var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyAsync("You must be in a voice channel.");
                return;
            }

            var audioClient = await voiceChannel.ConnectAsync();

            var youtube = YouTube.Default;
            var video = youtube.GetVideo(url);

            string tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, video.GetBytes());

            await PlayAudioAsync(audioClient, tempFile);
        }
    }
}
