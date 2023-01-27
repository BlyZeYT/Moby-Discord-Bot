namespace Moby.Common;

public sealed record DbUser
{
    public int Id { get; }

    public ulong UserId { get; }

    public long Score { get; }

    public DbUser(int id, ulong userId, long score)
    {
        Id = id;
        UserId = userId;
        Score = score;
    }

    public bool IsEmpty() => Id == -1 && UserId == 0 && Score == -1;

    public static DbUser Empty() => new(-1, 0, -1);
}