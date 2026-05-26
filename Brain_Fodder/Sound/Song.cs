using System;
using System.Runtime.CompilerServices;

public class Song
{
    // Properties
    public string Name { get; }
    public string FileName { get; }
    public int TrackID { get; }
    public int Shift { get; }

    // SINGLE LINE DEFINITIONS (No redundant names!)
    public static readonly Song Levels = new("Avicii - ID (Levels)", 1, -1);
    public static readonly Song Stan = new("eminem-stan", 3, -1);
    public static readonly Song JingleBells = new("jingle-bells-keyboard", 1, 0);
    public static readonly Song Kalinka = new("kalinka", 1, -1);
    public static readonly Song SuperMario = new("Mario Bros. - Super Mario Bros. Theme", 0, -1);
    public static readonly Song ZeldaLostWoods = new("Zelda - Ocarina of Time - Lost Woods Theme", 1, -1);
    public static readonly Song YoureBeautiful = new("YoureBeautiful", 3, 0);
    public static readonly Song Megalovania = new("UndertaleMegalovania", 0, -1);
    public static readonly Song FinalCountdown = new("FinalCountdown", 2, -1);
    public static readonly Song Blue = new("Blue", 8, -1);
    public static readonly Song LAmourToujours = new("Gigi D'Agostino - L'Amour Toujours", 0, -1);
    public static readonly Song TrilliumHardtekk = new("Trillium Hardtekk", 0, -1);

    // The magic happens in the constructor:
    private Song(
        string fileName,
        int trackID,
        int shift,
        [CallerMemberName] string name = "") // Compiler automatically injects the variable name here
    {
        FileName = fileName;
        TrackID = trackID;
        Shift = shift;
        Name = name;
    }
}