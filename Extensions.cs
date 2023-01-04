namespace Moby;

using Discord;
using Discord.WebSocket;
using global::Moby.Common;
using System.Text;

public static class Extensions
{
    public static Stream ToStream(this string str, Encoding? encoding = null)
        => new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(str));

    public static string RemoveAfter(this string str, int index, char character)
        => str.Length <= index ? str : str.Remove(str.IndicesOf(character).Where(x => x < index).Max());

    public static IEnumerable<int> IndicesOf(this string str, char character)
    {
        for (int i = str.IndexOf(character); i > -1; i = str.IndexOf(character, i + 1))
        {
            yield return i;
        }
    }

    public static async ValueTask<bool> TrySendAnnouncement(this SocketGuild guild, Embed announcement)
    {
        foreach (var channel in guild.TextChannels)
        {
            if (guild.CurrentUser.GetPermissions(channel).SendMessages)
            {
                await channel.SendMessageAsync(embed: announcement);

                return true;
            }
        }

        return false;
    }

    public static SocketTextChannel? GetFirstTextChannel(this SocketGuild guild)
    {
        foreach (var channel in guild.TextChannels)
        {
            if (guild.CurrentUser.GetPermissions(channel).SendMessages)
            {
                return channel;
            }
        }

        return null;
    }

    public static IEnumerable<Avatar> GetAllAvatarResolutions(this IUser user, ImageFormat imageFormat)
    {
        string? url;

        for (ushort size = 16; size <= 2048; size *= 2)
        {
            url = user.GetAvatarUrl(imageFormat, size);

            if (url is null) continue;

            yield return new Avatar(url, size, imageFormat);
        }
    }
}