using System;
using System.IO;
using System.Linq;

namespace DefconNull;

public static partial class Log
{
    private static long startTime;
    public static void Init()
    {
        Console.WriteLine("Log Init");
        startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var files = new DirectoryInfo("/Logs").EnumerateDirectories()
            .OrderByDescending(f => f.CreationTime)
            .Skip(15)
            .ToList();
        files.ForEach(f => f.Delete());
        
    }
    public static void Message(string category, string message)
    {
        
        String msg = "["+category+"]["+ DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() +"]"+message;
        Directory.CreateDirectory("Logs");
        Directory.CreateDirectory("Logs/"+startTime);
        GeneralLog(msg);
        FileStream filestream = new FileStream("Logs/"+startTime+"/"+category+".txt", FileMode.OpenOrCreate);
        var streamwriter = new StreamWriter(filestream);
        streamwriter.AutoFlush = true;
        streamwriter.WriteLine(msg);
        filestream.Close();
    
    }

    private static void GeneralLog(string msg)
    {
        FileStream filestream = new FileStream("Logs/"+startTime+"/Main.txt", FileMode.OpenOrCreate);
        var streamwriter = new StreamWriter(filestream);
        streamwriter.AutoFlush = true;
        streamwriter.WriteLine(msg);
        streamwriter.Close();
          Console.WriteLine(msg);
        return;
    }
}