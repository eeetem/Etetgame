using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DefconNull;

public static class Log
{
    private static long startTime;
    private static Dictionary<string, StreamWriter> logStreams = new Dictionary<string, StreamWriter>();
    public static object lockObject = new object();
    private static string logFolder = "Logs";
    private static StreamWriter GetLogStream(string category)
    {
        if(logStreams.ContainsKey(category))
            return logStreams[category];
        
        FileStream filestream = new FileStream(logFolder+"/"+startTime+"/"+category+".txt", FileMode.OpenOrCreate);
        var streamwriter = new StreamWriter(filestream);
        streamwriter.AutoFlush = true;
        
        logStreams.Add(category,streamwriter);  
        
        return streamwriter;
    }

    public static void Init()
    {
        Console.WriteLine("Log Init");
#if SERVER
        logFolder = "ServerLogs";
#endif
        startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        Directory.CreateDirectory(logFolder);
        Directory.CreateDirectory(logFolder+"/"+startTime);
        var files = new DirectoryInfo(logFolder).EnumerateDirectories()
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
    
    private static List<string> generalIgnoreList = new List<string>()
    {
        "WARNING",
        "ERROR"
    };
    public static void Message(string category, string message)
    {
       
        StringBuilder msg = new StringBuilder();
        msg.Append("[");
        msg.Append(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        msg.Append("][");
        msg.Append(category);
        msg.Append("]");
        msg.Append(message);

        if (generalIgnoreList.Contains(category))
        {
            lock (lockObject)
            {
                GetLogStream(category).WriteLine(msg);
            }
        }
        else
        {
            GeneralLog(msg.ToString());
        }
           

 

    }
    private static ConcurrentQueue<string> consoleQueue = new ConcurrentQueue<string>();
    private static void GeneralLog(string msg)
    {
        lock (lockObject)
        {
            GetLogStream("Main").WriteLine(msg);
        }

        consoleQueue.Enqueue(msg);
      
    }

    public static void Crash(object exceptionObject)
    {
        Message("CRASH",exceptionObject.ToString());
        lock (lockObject)
        {
            try
            {
                foreach (var stream in logStreams)
                {
                    stream.Value.Close();
                }

                Directory.Move(logFolder + "/" + startTime, logFolder + "/[CRASH]" + startTime);
            }catch(Exception e)
            {
                Message("CRASH",e.ToString());
            }
        }
    }
}