namespace Moby;

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
}