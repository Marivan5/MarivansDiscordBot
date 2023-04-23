using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MarvBotV3.Commands;

public class MusicModule : ModuleBase<CommandContext>
{
    private IAudioClient _audioClient;

    [Command("join", RunMode = RunMode.Async)]
    public async Task JoinVoiceChannel()
    {
        var user = Context.User as SocketGuildUser;
        var voiceChannel = user.VoiceChannel;

        if (voiceChannel == null)
        {
            await ReplyAsync("You need to be in a voice channel!");
            return;
        }

        _audioClient = await voiceChannel.ConnectAsync();
    }

    [Command("leave", RunMode = RunMode.Async)]
    public async Task LeaveVoiceChannel()
    {
        if (_audioClient == null)
        {
            await ReplyAsync("I'm not connected to a voice channel!");
            return;
        }

        await _audioClient.StopAsync();
        await ReplyAsync("Disconnected from the voice channel!");
    }

    [Command("play", RunMode = RunMode.Async)]
    public async Task PlayYouTubeVideo(string youtubeUrl)
    {
        if (_audioClient == null)
        {
            var user = Context.User as SocketGuildUser;
            var voiceChannel = user.VoiceChannel;

            if (voiceChannel == null)
            {
                await ReplyAsync("You need to be in a voice channel!");
                return;
            }

            _audioClient = await voiceChannel.ConnectAsync();
        }

        var youtube = new YoutubeClient();
        var video = await youtube.Videos.GetAsync(youtubeUrl);
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
        var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

        if (audioStreamInfo != null)
        {
            await using var audioStream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);
            await PlayAudioWithFFmpegAsync(audioStream);
        }
    }

    private async Task PlayAudioWithFFmpegAsync(Stream input)
    {
        var ffmpeg = "ffmpeg"; // Change this to the full path of the FFmpeg executable if necessary
        var arguments = "-i pipe:0 -f s16le -ac 2 -ar 48000 -acodec pcm_s16le pipe:1";
        var startInfo = new ProcessStartInfo(ffmpeg, arguments)
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        // Copy the audio stream to FFmpeg's stdin
        var inputCopyTask = input.CopyToAsync(process.StandardInput.BaseStream);

        using var output = process.StandardOutput.BaseStream;
        byte[] buffer = new byte[3840];
        int bytesRead;

        while ((bytesRead = await output.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            if (_audioClient.ConnectionState != ConnectionState.Connected)
            {
                break;
            }

            await SendAudioAsync(_audioClient, buffer, bytesRead);
        }

        await inputCopyTask;
        process.StandardInput.Close();
        await process.WaitForExitAsync();
    }

    private async Task SendAudioAsync(IAudioClient audioClient, byte[] buffer, int bytesRead)
    {
        using var audioOutStream = audioClient.CreatePCMStream(AudioApplication.Music, 128 * 1024);
        await audioOutStream.WriteAsync(buffer, 0, bytesRead);
    }

    private async Task<string> ConvertAudioWithFFmpegAsync(string inputFilePath)
    {
        string outputFilePath = Path.ChangeExtension(inputFilePath, ".wav");

        var ffmpeg = "ffmpeg"; // Change this to the full path of the FFmpeg executable if necessary
        var arguments = $"-i \"{inputFilePath}\" -ac 2 -ar 48000 \"{outputFilePath}\"";
        var startInfo = new ProcessStartInfo(ffmpeg, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using (var process = new Process { StartInfo = startInfo })
        {
            process.Start();
            await process.WaitForExitAsync();
        }

        return outputFilePath;
    }
}
