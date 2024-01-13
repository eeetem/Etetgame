using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DefconNull;

public static class Log
{
    private static long startTime;
    private static Dictionary<string, StreamWriter> logStreams = new Dictionary<string, StreamWriter>();
    private static StreamWriter GetLogStream(string category)
    {
        if(logStreams.ContainsKey(category))
            return logStreams[category];
        
        FileStream filestream = new FileStream("Logs/"+startTime+"/"+category+".txt", FileMode.OpenOrCreate);
        var streamwriter = new StreamWriter(filestream);
        streamwriter.AutoFlush = true;
        
        logStreams.Add(category,streamwriter);  
        
        return streamwriter;
    }

    public static void Init()
    {
        Console.WriteLine("Log Init");
        startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var files = new DirectoryInfo("Logs").EnumerateDirectories()
            .OrderByDescending(f => f.CreationTime)
            .Skip(10)
            .ToList();
        files.ForEach(f => f.Delete(true));
        
        
        Task.Run(() =>
        {
            while (true)
            {
                string msg;
                while (consoleQueue.TryDequeue(out msg!))
                { 
                    Console.WriteLine(msg);
                }

                Thread.Sleep(500);
                
            }
        });
        
    }
    public static void Message(string category, string message)
    {
        
        String msg = "["+DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()+"]["+category  +"]"+message;
        Directory.CreateDirectory("Logs");
        Directory.CreateDirectory("Logs/"+startTime);
        GeneralLog(msg);

        GetLogStream(category).WriteLine(msg);

    
    }
    private static ConcurrentQueue<string> consoleQueue = new ConcurrentQueue<string>();
    private static void GeneralLog(string msg)
    {
        GetLogStream("Main").WriteLine(msg);
        
        consoleQueue.Enqueue(msg);
      
    }
}