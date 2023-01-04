namespace Moby.Common;

using Discord;

public sealed record Avatar
{
    public string Url { get; }
    public ushort Size { get; }
    public ImageFormat ImageFormat { get; }

    public Avatar(string url, ushort size, ImageFormat imageFormat)
    {
        Url = url;
        Size = size;
        ImageFormat = imageFormat;
    }
}