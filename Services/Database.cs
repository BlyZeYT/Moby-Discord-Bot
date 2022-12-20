namespace Moby.Services;

using global::Moby.Common;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;
using System.Diagnostics;

public interface IDatabase
{
    public ValueTask<Prefix?> GetPrefixAsync(ulong guildId);

    public Task SetPrefixAsync(ulong guildId, Prefix prefix);

    public Task AddGuildAsync(ulong guildId);

    public Task RemoveGuildAsync(ulong guildId);

    public ValueTask<bool> GetRepeatAsync(ulong guildId);

    public Task SetRepeatAsync(ulong guildId, bool repeat);

    public IAsyncEnumerable<string> GetPlaylistTracksAsync(DatabasePlaylist databasePlaylist);

    public ValueTask<DatabasePlaylist> GetPlaylistInfoAsync(ulong guildId, string name);

    public IAsyncEnumerable<DatabasePlaylist> GetAllPlaylistsInfoAsync(ulong guildId);

    public Task AddPlaylistAsync(ulong guildId, string name, IEnumerable<string> trackUrls);

    public ValueTask<bool> TryAddTrackToPlaylistAsync(ulong guildId, string name, string trackUrl);

    public ValueTask<bool> TryRemoveTrackFromPlaylistAsync(DatabasePlaylist databasePlaylist, int playlistPosition);

    public ValueTask<bool> TryRemovePlaylistAsync(ulong guildId, string name);

    public Task RemoveAllPlaylistsAsync(ulong guildId);

    public ValueTask<DatabaseGuildInfo> GetGuildInfoAsync(ulong guildId);

    public IAsyncEnumerable<ulong> GetAllGuildsAsync();

    public ValueTask<TimeSpan> PingAsync();
}

public sealed class Database : IDatabase
{
    private readonly IConfiguration _config;
    private readonly IMobyLogger _logger;
    private readonly MySqlConnection _connection;

    public Database(IConfiguration config, IMobyLogger logger)
    {
        _config = config;
        _logger = logger;
        _connection = new MySqlConnection(_config["database"]);
    }

    public async ValueTask<Prefix?> GetPrefixAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(GetPrefixAsync)} for Guild: {guildId}");

        try
        {
            var prefix = await new MySqlCommand($"SELECT Guild_Prefix FROM guilds WHERE Guild_Id = {guildId}", _connection).ExecuteScalarAsync();

            await _logger.LogDebugAsync($"Returned Prefix: {prefix} for Guild: {guildId}");

            return prefix is null or "" ? null : Prefix.Create(prefix.ToString()!);
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to get Prefix for Guild: {guildId}");

            return null;
        }
    }

    public async Task SetPrefixAsync(ulong guildId, Prefix prefix)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(SetPrefixAsync)} for Guild: {guildId}");

        try
        {
            await new MySqlCommand($"UPDATE guilds SET Guild_Prefix = '{prefix}' WHERE Guild_Id = {guildId}", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Updated Prefix for Guild: {guildId} to {prefix}");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to set Prefix for Guild: {guildId} to {prefix}");
        }
    }

    public async Task AddGuildAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(AddGuildAsync)} for Guild: {guildId}");

        try
        {
            await new MySqlCommand($"INSERT INTO guilds(Guild_Id, Guild_Prefix, Guild_Repeat) VALUES('{guildId}', '', '0')", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Added Guild: {guildId} to database");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to add Guild: {guildId} to database");
        }
    }

    public async Task RemoveGuildAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(RemoveGuildAsync)} for Guild: {guildId}");

        try
        {
            await new MySqlCommand($"DELETE FROM guilds WHERE Guild_Id = {guildId}", _connection).ExecuteNonQueryAsync();

            var id = await new MySqlCommand($"SELECT ID FROM playlists WHERE Guild_Id = {guildId}", _connection).ExecuteScalarAsync();

            await new MySqlCommand($"DELETE FROM playlists WHERE Guild_Id = {id}", _connection).ExecuteNonQueryAsync();
            await new MySqlCommand($"DELETE FROM tracks WHERE Track_Id = {id}", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Removed Guild: {guildId} from all database tables");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to remove Guild: {guildId} from all database tables");
        }
    }

    public async ValueTask<bool> GetRepeatAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(GetRepeatAsync)} for Guild: {guildId}");

        try
        {
            var repeat = await new MySqlCommand($"SELECT Guild_Repeat FROM guilds WHERE Guild_Id = {guildId}", _connection).ExecuteScalarAsync());

            await _logger.LogDebugAsync($"Returned Repeat: {repeat} for Guild: {guildId}");

            return Convert.ToBoolean(repeat);
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to get Repeat for Guild: {guildId}");

            return false;
        }
    }

    public async Task SetRepeatAsync(ulong guildId, bool repeat)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(SetRepeatAsync)} for Guild: {guildId}");

        try
        {
            await new MySqlCommand($"UPDATE guilds SET Guild_Repeat = {repeat} WHERE Guild_Id = {guildId}", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Updated Repeat for Guild: {guildId} to {repeat}");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to set Repeat for Guild: {guildId} to {repeat}");
        }
    }

    public async IAsyncEnumerable<string> GetPlaylistTracksAsync(int trackId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(GetPlaylistTracksAsync)} for Track Id: {trackId}");

        MySqlDataReader reader;
        try
        {
            reader = await new MySqlCommand($"SELECT Url FROM tracks WHERE Track_Id = {trackId} ORDER BY Playlist_Position ASC", _connection).ExecuteReaderAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to get Tracks for Track Id: {trackId}");

            yield break;
        }

        using (reader)
        {
            while (await reader.ReadAsync())
            {
                yield return reader.GetString(0);
            }
        }

        await _logger.LogDebugAsync($"Returned Tracks for Track Id: {trackId}");
    }

    public async ValueTask<DatabasePlaylist> GetPlaylistInfoAsync(ulong guildId, string name)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(GetPlaylistInfoAsync)} for Guild: {guildId}");

        try
        {
            
        }
        catch (Exception ex)
        {
            
        }
    }

    public async IAsyncEnumerable<DatabasePlaylist> GetAllPlaylistsInfoAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(GetAllPlaylistsInfoAsync)} for Guild: {guildId}");


    }

    public async Task AddPlaylistAsync(ulong guildId, string name, IEnumerable<string> trackUrls)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(AddPlaylistAsync)} for Guild: {guildId}");


    }

    public async ValueTask<bool> TryAddTrackToPlaylistAsync(ulong guildId, string name, string trackUrl)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(TryAddTrackToPlaylistAsync)} for Guild: {guildId}");


    }

    public async ValueTask<bool> TryRemoveTrackFromPlaylistAsync(DatabasePlaylist databasePlaylist, int playlistPosition)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(TryRemoveTrackFromPlaylistAsync)} for Guild: {guildId}");


    }

    public async ValueTask<bool> TryRemovePlaylistAsync(ulong guildId, string name)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(TryRemovePlaylistAsync)} for Guild: {guildId}");


    }

    public async Task RemoveAllPlaylistsAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(RemoveAllPlaylistsAsync)} for Guild: {guildId}");


    }

    public async ValueTask<DatabaseGuildInfo> GetGuildInfoAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(GetGuildInfoAsync)} for Guild: {guildId}");


    }

    public async IAsyncEnumerable<ulong> GetAllGuildsAsync()
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes {nameof(GetAllGuildsAsync)}");


    }

    public async ValueTask<TimeSpan> PingAsync()
    {
        await _logger.LogDebugAsync($"Executes {nameof(PingAsync)}");

        var sw = Stopwatch.StartNew();

        try
        {
            var con = _connection.OpenAsync();
            await con;

            while (!con.IsCompleted) { }

            sw.Stop();

            await _logger.LogDebugAsync($"Connected to database in **{sw.ElapsedMilliseconds}ms**");

            return sw.Elapsed;
        }
        catch (Exception ex)
        {
            sw.Stop();

            await _logger.LogCriticalAsync(ex, "Failed to connect to database");

            return TimeSpan.MaxValue;
        }
    }

    private async Task ConnectAsync()
    {
        await _logger.LogDebugAsync("Started attempt to connect to database");

        try
        {
            if (_connection.State is ConnectionState.Closed)
                await _connection.OpenAsync();

            await _logger.LogDebugAsync("Connected to database successfully");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, "Failed to connect to database");

            await _connection.CloseAsync();
        }
    }
}