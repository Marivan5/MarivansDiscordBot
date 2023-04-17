using Discord.Commands;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace MarvBotV3.Commands
{
    public class YouTubeModule : ModuleBase<CommandContext>
    {
        private readonly YoutubeClient _youtube;

        public YouTubeModule()
        {
            _youtube = new YoutubeClient();
        }

        [Command("audioDl")]
        public async Task PlayAsync([Remainder] string query)
        {
            query = query.Split('&').FirstOrDefault();
            var videoId = await _youtube.Search.GetVideosAsync(query);

            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId.FirstOrDefault().Id);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            if (audioStreamInfo == null)
            {
                await ReplyAsync("Sorry, no suitable audio stream was found for this video.");
                return;
            }

            var stream = await _youtube.Videos.Streams.GetAsync(audioStreamInfo);

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            var attachment = new MemoryStream(memoryStream.ToArray());
            await Context.Channel.SendFileAsync(attachment, $"{videoId.FirstOrDefault().Title}.{audioStreamInfo.Container}");
        }
    }
}
