namespace Moby.Modules;

using Common;
using Discord;
using Discord.Interactions;
using Enums;
using Services;
using System;
using System.Threading.Tasks;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;

[Discord.Commands.Name("Music")]
public sealed class MusicModule : MobyModuleBase
{
    private readonly LavaNode<MobyPlayer, LavaTrack> _lava;

    public MusicModule(LavaNode<MobyPlayer, LavaTrack> lava, ConsoleLogger logger) : base(logger)
    {
        _lava = lava;
    }

    [RequireRole("DJ")]
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
            await FollowupAsync($"Joined **{voiceState.VoiceChannel.Name}**");
        }
        catch (Exception ex)
        {
            await FollowupAsync($"Couldn't join **{voiceState.VoiceChannel.Name}**", ephemeral: true);
            _console.LogError("Couldn't join a voice channel on Guild: " + Context.Guild.Id, ex);
        }
    }

    [RequireRole("DJ")]
    [SlashCommand("leave", "The player leaves your channel")]
    public async Task LeaveAsync()
    {
        await DeferAsync(ephemeral: true);

        if (!_lava.TryGetPlayer(Context.Guild, out var player))
        {
            await FollowupAsync("I'm not connected to any voice channel", ephemeral: true);
            return;
        }

        var voiceChannel = ((IVoiceState)Context.User).VoiceChannel ?? player.VoiceChannel;
        if (voiceChannel is null)
        {
            await FollowupAsync("I'm not sure which voice channel to disconnect from", ephemeral: true);
            return;
        }

        try
        {
            await _lava.LeaveAsync(voiceChannel);
            await FollowupAsync($"Lefted **{voiceChannel.Name}**");
        }
        catch (Exception ex)
        {
            await FollowupAsync($"Couldn't leave **{voiceChannel.Name}**", ephemeral: true);
            _console.LogError("Couldn't left a voice channel on Guild: " + Context.Guild.Id, ex);
        }
    }

    [RequireRole("DJ")]
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

        if (!_lava.TryGetPlayer(Context.Guild, out var player))
        {
            var voiceState = (IVoiceState)Context.User;

            if (voiceState.IsInvalidVoiceState())
            {
                await FollowupAsync("You have to be in a voice channel and not deafened", ephemeral: true);
                return;
            }

            try
            {
                player = await _lava.JoinAsync(voiceState.VoiceChannel, (ITextChannel)Context.Channel);
                await FollowupAsync($"Joined **{voiceState.VoiceChannel.Name}**");
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Couldn't join **{voiceState.VoiceChannel.Name}**", ephemeral: true);
                _console.LogError("Couldn't join a voice channel on Guild: " + Context.Guild.Id, ex);
            }
        }

        if (string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
        {
            var track = searchResponse.Tracks.First();

            player.Vueue.Enqueue(track);
            await FollowupAsync(embed: MobyUtil.GetTrackEnqueuedEmbed(track, source));
        }
        else
        {
            var tracks = searchResponse.Tracks;

            player.Vueue.Enqueue(tracks);
            await FollowupAsync(embed: MobyUtil.GetTracksEnqueuedEmbed(source, tracks.ToArray()));
        }

        if (player.PlayerState is PlayerState.Playing or PlayerState.Paused) return;

        player.Vueue.TryDequeue(out var lavaTrack);
        await player.PlayAsync(lavaTrack);
    }

    [RequireRole("DJ")]
    [SlashCommand("pause", "Pause the player")]
    public async Task PauseAsync()
    {
        await DeferAsync(ephemeral: true);

        if (!_lava.TryGetPlayer(Context.Guild, out var player))
        {
            await FollowupAsync("I'm not connected to any voice channel", ephemeral: true);
            return;
        }

        if (player.PlayerState is not PlayerState.Playing)
        {
            await FollowupAsync("I cannot pause when I'm not playing", ephemeral: true);
            return;
        }

        try
        {
            await player.PauseAsync();
            await FollowupAsync($"\\⏸️ Paused");
        }
        catch (Exception ex)
        {
            await FollowupAsync($"Couldn't pause **{player.Track.Title}**", ephemeral: true);
            _console.LogError("Couldn't pause a track on Guild: " + Context.Guild.Id, ex);
        }
    }

    [RequireRole("DJ")]
    [SlashCommand("resume", "Resume the player")]
    public async Task ResumeAsync()
    {
        await DeferAsync(ephemeral: true);

        if (!_lava.TryGetPlayer(Context.Guild, out var player))
        {
            await FollowupAsync("I'm not connected to any voice channel", ephemeral: true);
            return;
        }

        if (player.PlayerState is not PlayerState.Paused)
        {
            await FollowupAsync("I cannot resume when I'm not paused", ephemeral: true);
            return;
        }

        try
        {
            await player.ResumeAsync();
            await FollowupAsync($"\\▶️ Resumed");
        }
        catch (Exception ex)
        {
            await FollowupAsync($"Couldn't resume **{player.Track.Title}**", ephemeral: true);
            _console.LogError("Couldn't resume a track on Guild: " + Context.Guild.Id, ex);
        }
    }
}