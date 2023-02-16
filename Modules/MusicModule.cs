namespace Moby.Modules;

using Common;
using Discord;
using Discord.Interactions;
using Enums;
using Services;
using Victoria.Node;
using Victoria.Responses.Search;

[Discord.Commands.Name("Music")]
public sealed class MusicModule : MobyModuleBase
{
    private readonly LavaNode<MobyPlayer, MobyTrack> _lava;

    public MusicModule(LavaNode<MobyPlayer, MobyTrack> lava, ConsoleLogger logger) : base(logger)
    {
        _lava = lava;
    }

    [SlashCommand("join", "The player joins your channel")]
    public async Task JoinAsync()
    {
        await DeferAsync(ephemeral: true);

        if (_lava.HasPlayer(Context.Guild))
        {
            await FollowupAsync("I'm already connected to a voice channel", ephemeral: true);
            return;
        }

        var voiceState = (IVoiceState)Context.User;

        if (voiceState.IsInvalidVoiceState())
        {
            await FollowupAsync("You have to be in a voice channel and not deafened", ephemeral: true);
            return;
        }

        try
        {
            await _lava.JoinAsync(voiceState.VoiceChannel, (ITextChannel)Context.Channel);
            await FollowupAsync($"Joined **{voiceState.VoiceChannel.Name}**", ephemeral: true);
        }
        catch(Exception ex)
        {
            await FollowupAsync($"Couldn't join **{voiceState.VoiceChannel.Name}**", ephemeral: true);
            _console.LogError("Couldn't join a voice channel on Guild: " + Context.Guild.Id, ex);
        }
    }

    [SlashCommand("play", "Play anything from anywhere")]
    public async Task PlayAsync([Summary("source", "The source of the music")] MusicSource source,
        [Summary("query", "A link or a search term")] [MinLength(1)] [MaxLength(250)] string query)
    {
        await DeferAsync(ephemeral: true);

        if (((IVoiceState)Context.User).IsInvalidVoiceState())
        {
            await FollowupAsync("You have to be in a voice channel and not deafened", ephemeral: true);
            return;
        }

        var searchResponse = await _lava.SearchAsync(source.GetSearchType(), source.GetFormattedQuery(query));

        if (searchResponse.Status is SearchStatus.LoadFailed)
        {
            await FollowupAsync($"Load failed: {searchResponse.Exception.Message}", ephemeral: true);
            return;
        }

        if (searchResponse.Status is SearchStatus.NoMatches)
        {
            await FollowupAsync($"Couldn't find any matches for '{query}' on {source.GetString()}", ephemeral: true);
            return;
        }
    }
}