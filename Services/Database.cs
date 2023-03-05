namespace Moby.Services;

using Common;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;
using System.Diagnostics;

public interface IDatabase
{
    public Task AddGuildAsync(ulong guildId);

    public Task RemoveGuildAsync(ulong guildId);

    public IAsyncEnumerable<string> GetPlaylistTracksAsync(int trackId);

    public ValueTask<int> GetPlaylistTrackIdAsync(ulong guildId, string name);

    public IAsyncEnumerable<int> GetAllPlaylistsTrackIdsAsync(ulong guildId);

    public Task AddPlaylistAsync(ulong guildId, string name);

    public ValueTask<bool> TryAddTrackToPlaylistAsync(ulong guildId, string name, string trackUrl);

    public ValueTask<bool> TryRemoveTrackFromPlaylistAsync(int trackId, int playlistPosition);

    public ValueTask<bool> TryRemovePlaylistAsync(ulong guildId, string name);

    public Task RemoveAllPlaylistsAsync(ulong guildId);

    public ValueTask<DbGuild> GetGuildInfoAsync(ulong guildId);

    public IAsyncEnumerable<DbGuild> GetAllGuildsAsync();

    public Task AddUserAsync(ulong userId);

    public Task RemoveUserAsync(ulong userId);

    public Task AddScoreAsync(ulong userId, long score);

    public ValueTask<DbUser> GetUserInfoAsync(ulong userId);

    public IAsyncEnumerable<DbUser> GetAllUsersAsync();

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

    public async Task AddGuildAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(AddGuildAsync)}** for Guild Id: {guildId}");

        try
        {
            await new MySqlCommand($"INSERT INTO guilds(Guild_Id, Guild_Prefix, Guild_Repeat) VALUES('{guildId}', '', '0')", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Added Guild with Guild Id: {guildId} to database");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to add Guild with Guild Id: {guildId} to database");
        }
    }

    public async Task RemoveGuildAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(RemoveGuildAsync)}** for Guild Id: {guildId}");

        try
        {
            await new MySqlCommand($"DELETE FROM guilds WHERE Guild_Id = {guildId}", _connection).ExecuteNonQueryAsync();

            var id = await new MySqlCommand($"SELECT ID FROM playlists WHERE Guild_Id = {guildId}", _connection).ExecuteScalarAsync();

            await new MySqlCommand($"DELETE FROM playlists WHERE Guild_Id = {id}", _connection).ExecuteNonQueryAsync();
            await new MySqlCommand($"DELETE FROM tracks WHERE Track_Id = {id}", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Removed Guild with Guild Id: {guildId} from all database tables");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to remove Guild with Guild Id: {guildId} from all database tables");
        }
    }

    public async IAsyncEnumerable<string> GetPlaylistTracksAsync(int trackId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(GetPlaylistTracksAsync)}** for Track Id: {trackId}");

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

    /// <returns>-1 if the playlist does not exist, else the track number</returns>
    public async ValueTask<int> GetPlaylistTrackIdAsync(ulong guildId, string name)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(GetPlaylistTrackIdAsync)}** for Guild Id: {guildId} and Name: {name}");

        try
        {
            var trackId = await new MySqlCommand($"SELECT ID FROM playlists WHERE Guild_Id = {guildId} AND Name = '{name}'", _connection).ExecuteScalarAsync();

            if (trackId is null)
            {
                await _logger.LogDebugAsync($"Playlist Track Id for Guild Id: {guildId} and Name: {name} does not exist");

                return -1;
            }

            await _logger.LogDebugAsync($"Returned Playlist Track Id for Guild Id: {guildId} and Name: {name}");

            return Convert.ToInt32(trackId);
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to get Playlist Track Id for Guild Id: {guildId} and Name: {name}");

            return -1;
        }
    }

    public async IAsyncEnumerable<int> GetAllPlaylistsTrackIdsAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(GetAllPlaylistsTrackIdsAsync)}** for Guild Id: {guildId}");

        MySqlDataReader reader;
        try
        {
            reader = await new MySqlCommand($"SELECT ID FROM tracks WHERE Guild_Id = {guildId}", _connection).ExecuteReaderAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to get Playlist Track Ids for Guild Id: {guildId}");

            yield break;
        }

        using (reader)
        {
            while (await reader.ReadAsync())
            {
                yield return reader.GetInt32(0);
            }
        }

        await _logger.LogDebugAsync($"Returned Playlist Track Ids for Guild Id: {guildId}");
    }

    public async Task AddPlaylistAsync(ulong guildId, string name)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(AddPlaylistAsync)}** for Guild Id: {guildId} and Name: {name}");

        try
        {
            await new MySqlCommand($"INSERT INTO playlists(Name, Guild_Id) VALUES('{name}', '{guildId}')", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Added Playlist for Guild Id: {guildId} with Name {name}");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to add Playlist for Guild Id: {guildId} with Name: {name}");
        }
    }

    public async ValueTask<bool> TryAddTrackToPlaylistAsync(ulong guildId, string name, string trackUrl)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(TryAddTrackToPlaylistAsync)}** for Guild Id: {guildId} and Name: {name} and Track Url {trackUrl}");

        try
        {
            var trackId = await GetPlaylistTrackIdAsync(guildId, name);

            if (trackId == -1) throw new Exception($"Couldn't get Playlist");

            var playlistPosition = Convert.ToInt32(await new MySqlCommand($"SELECT MAX(Playlist_Position) FROM tracks WHERE Track_Id = {trackId}", _connection).ExecuteScalarAsync() ?? -1);

            if (playlistPosition == -1) throw new Exception($"Couldn't get Playlist Position for Track Id: {trackId}");

            await new MySqlCommand($"INSERT INTO tracks(Track_Id, Url, Playlist_Position) VALUES({trackId}, '{trackUrl}', {playlistPosition + 1})", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Added Track to Playlist for Guild Id: {guildId} with Name {name} and Track Url: {trackUrl}");

            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to add Track to Playlist for Guild Id: {guildId} with Name {name} and Track Url: {trackUrl}");

            return false;
        }
    }

    public async ValueTask<bool> TryRemoveTrackFromPlaylistAsync(int trackId, int playlistPosition)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(TryRemoveTrackFromPlaylistAsync)}** for Guild Id: {trackId} and Playlist Position: {playlistPosition}");

        try
        {
            var succeeded = await new MySqlCommand($"DELETE FROM tracks WHERE Track_Id = {trackId} AND Playlist_Position = {playlistPosition}", _connection).ExecuteNonQueryAsync();

            if (succeeded != 1) throw new Exception($"Couldn't remove Track with Track Id: {trackId} and Playlist Position: {playlistPosition}");

            using (var reader = await new MySqlCommand($"SELECT Playlist_Position FROM tracks WHERE Track_Id = {trackId} AND Playlist_Position > {playlistPosition}", _connection).ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    await new MySqlCommand($"UPDATE tracks SET Playlist_Position = {reader.GetInt32(3) - 1} WHERE Track_Id = {trackId} AND Playlist_Position = {reader.GetInt32(3)}", _connection).ExecuteNonQueryAsync();
                }
            }

            await _logger.LogDebugAsync($"Removed Track from Playlist for Track Id: {trackId} and Playlist Position: {playlistPosition}");

            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to remove Track from Playlist for Track Id: {trackId} and Playlist Position: {playlistPosition}");

            return false;
        }
    }

    public async ValueTask<bool> TryRemovePlaylistAsync(ulong guildId, string name)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(TryRemovePlaylistAsync)}** for Guild Id: {guildId} and Name: {name}");

        try
        {
            var trackId = await GetPlaylistTrackIdAsync(guildId, name);

            if (trackId == -1) throw new Exception($"Couldn't get Playlist");

            var succeeded = await new MySqlCommand($"DELETE FROM playlists WHERE ID = {trackId}", _connection).ExecuteNonQueryAsync();

            if (succeeded != 1) throw new Exception($"Couldn't remove Playlist with Track Id: {trackId}");

            succeeded = await new MySqlCommand($"DELETE FROM tracks WHERE Track_Id = {trackId}", _connection).ExecuteNonQueryAsync();

            if (succeeded == 0) throw new Exception($"Couldn't remove Playlist Tracks with Track Id: {trackId}");

            await _logger.LogDebugAsync($"Removed Playlist for Guild Id: {guildId} with Name {name}");

            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to remove Playlist for Guild Id: {guildId} with Name: {name}");

            return false;
        }
    }

    public async Task RemoveAllPlaylistsAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(RemoveAllPlaylistsAsync)}** for Guild Id: {guildId}");

        try
        {
            using (var reader = await new MySqlCommand($"SELECT ID FROM playlists WHERE Guild_Id = {guildId}", _connection).ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    await new MySqlCommand($"DELETE FROM tracks WHERE Track_Id = {reader.GetInt32(0)}", _connection).ExecuteNonQueryAsync();
                }
            }

            await new MySqlCommand($"DELETE FROM playlists WHERE Guild_Id = {guildId}", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Removed Playlists for Guild Id: {guildId}");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to remove Playlists for Guild Id: {guildId}");
        }
    }

    public async ValueTask<DbGuild> GetGuildInfoAsync(ulong guildId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(GetGuildInfoAsync)}** for Guild Id: {guildId}");

        using (var reader = await new MySqlCommand($"SELECT * FROM guilds WHERE Guild_Id = {guildId}", _connection).ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                await _logger.LogDebugAsync($"Returned Guild Info for Guild Id: {guildId}");

                return new DbGuild(reader.GetInt32(0), guildId, reader.GetBoolean(2));
            }
            else
            {
                await _logger.LogWarningAsync($"Couldn't find Guild Info for Guild Id: {guildId}");

                return DbGuild.Empty();
            }
        }
    }

    public async IAsyncEnumerable<DbGuild> GetAllGuildsAsync()
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(GetAllGuildsAsync)}**");

        using (var reader = await new MySqlCommand($"SELECT Guild_Id FROM guilds", _connection).ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                yield return new DbGuild(reader.GetInt32(0), reader.GetUInt64(1), reader.GetBoolean(2));
            }

            await _logger.LogDebugAsync($"Returned all Guilds from database");
        }
    }

    public async Task AddUserAsync(ulong userId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(AddUserAsync)}** for User Id: {userId}");

        try
        {
            await new MySqlCommand($"INSERT INTO users(User_Id, Score) VALUES('{userId}', '0')", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Added a user with User Id: {userId}");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to add a user with User Id: {userId}");
        }
    }

    public async Task RemoveUserAsync(ulong userId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(RemoveUserAsync)}** for User Id: {userId}");

        try
        {
            await new MySqlCommand($"DELETE FROM users WHERE User_Id = {userId}", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Removed a user with User Id: {userId}");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to remove a user with User Id: {userId}");
        }
    }

    public async Task AddScoreAsync(ulong userId, long score)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(AddScoreAsync)}** for User Id: {userId} and Score: {score}");

        long userScore = 0;
        try
        {
            var id = Convert.ToInt32(await new MySqlCommand($"SELECT ID FROM users WHERE User_Id = {userId}", _connection).ExecuteScalarAsync());

            if (id < 1) await AddUserAsync(userId);

            userScore = Convert.ToInt64(await new MySqlCommand($"SELECT Score FROM users WHERE User_Id = {userId}", _connection).ExecuteScalarAsync());

            if (userScore > (long.MaxValue - score)) userScore = long.MaxValue;
            else userScore += score;

            await new MySqlCommand($"UPDATE users SET Score = {userScore} WHERE User_Id = {userId}", _connection).ExecuteNonQueryAsync();

            await _logger.LogDebugAsync($"Updated score: {userScore} to user with User Id: {userId}");
        }
        catch (Exception ex)
        {
            await _logger.LogCriticalAsync(ex, $"Failed to update score: {userScore} to user with User Id: {userId}");
        }
    }

    public async ValueTask<DbUser> GetUserInfoAsync(ulong userId)
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(GetUserInfoAsync)}** for User Id: {userId}");

        using (var reader = await new MySqlCommand($"SELECT * FROM users WHERE User_Id = {userId}", _connection).ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                await _logger.LogDebugAsync($"Returned User Info for User Id: {userId}");

                return new DbUser(reader.GetInt32(0), userId, reader.GetInt64(2));
            }
            else
            {
                await _logger.LogWarningAsync($"Couldn't find User Info for User Id: {userId}");

                return DbUser.Empty();
            }
        }
    }

    public async IAsyncEnumerable<DbUser> GetAllUsersAsync()
    {
        await ConnectAsync();

        await _logger.LogDebugAsync($"Executes **{nameof(GetAllUsersAsync)}**");

        using (var reader = await new MySqlCommand($"SELECT User_Id FROM users", _connection).ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                yield return new DbUser(reader.GetInt32(0), reader.GetUInt64(1), reader.GetInt64(2));
            }

            await _logger.LogDebugAsync($"Returned all Users from database");
        }
    }

    public async ValueTask<TimeSpan> PingAsync()
    {
        await _logger.LogDebugAsync($"Executes **{nameof(PingAsync)}**");

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

            if (_connection.State is not ConnectionState.Closed) await _connection.CloseAsync();
        }
    }
}