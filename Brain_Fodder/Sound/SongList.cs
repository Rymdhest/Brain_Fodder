using System.Reflection;
using System.Linq;
using SpaceEngine.Util;

public static class SongList
{
    // This automatically finds all 'public static readonly Song' fields in the class
    public static readonly List<Song> AllSongs = typeof(Song)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(Song))
        .Select(f => (Song)f.GetValue(null)!)
        .ToList();


    public static Song GetRandomSong()
    {
        return AllSongs[MyMath.rand.Next(AllSongs.Count)];
    }
}