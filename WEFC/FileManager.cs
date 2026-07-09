using System.Diagnostics;
using System.IO;

namespace WEFC;

public class FileManager
{
    private static FileManager? _instance = null;
    private static readonly Lock Lock = new Lock();
    
    private FileManager()
    {
        
    }

    public static FileManager Instance
    {
        get
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    _instance = new FileManager();
                }
                return _instance;
            }
        }
    }
    
    private string FilePath { get; set; }
    private string FileName { get; set; }
    
    public string filePath => FilePath;
    public string fileName => FileName;

    public void SetFile(string fullFilePath)
    {
        
        int lastPosition = 0;
        for (int i = 0; i < fullFilePath.Length; i++)
        {
            if (fullFilePath[i].Equals('\\'))
            {
                lastPosition = i;
            }
        }

        lastPosition += 1;
        FilePath = fullFilePath[..lastPosition];
        FileName = fullFilePath[lastPosition..];

    }

    public bool ConvertFile(string name,string outputFormat, string[] args, string startTime, string duration,string hwaccel, float size)
    {

        string arguments = "";
        if (size == -1)
        {
            arguments = $"{(!hwaccel.Equals("none") ? "-hwaccel " + hwaccel : "")} {(!startTime.Equals("") ? startTime : "")} {(!duration.Equals("") ? duration : "")}" +
                               $" -i \"{fileName}\" " + string.Join(" ", args) + 
                               $" \"{name + outputFormat}\"";
        }
        else
        {
            double bitrate = (size * 8192) / TimeSpan.Parse(duration.Substring(4,8)).TotalSeconds - 128;
            arguments = $"{(!hwaccel.Equals("none") ? "-hwaccel " + hwaccel : "")} {(!startTime.Equals("") ? startTime : "")} {(!duration.Equals("") ? duration : "")}" +
                        $" -i \"{fileName}\" " + string.Join(" ", args) + 
                        $" -b:a 48k -b:v {bitrate}k \"{name + outputFormat}\"";
        }
        Console.WriteLine("args: " + arguments);
        ProcessStartInfo ffmpeg = new ProcessStartInfo
        {
            CreateNoWindow = false,
            UseShellExecute = false,
            FileName = @"ffmpeg\ffmpeg.exe",
            Arguments = arguments,
            RedirectStandardOutput = true,
            WorkingDirectory =  filePath
        };
        
        using (Process process = new Process())
        {
            process.StartInfo = ffmpeg;
            process.Start();
            process.WaitForExit();
        }
        
        return true;
    }
    
    public bool ConvertFile(Preset preset)
    {
        
        string arguments = $"-hwaccel cuda {(!preset.FirstArg.Equals("") ? preset.FirstArg + " " : "")} -i \"{fileName}\" " + string.Join(" ", preset.Args)+
                           $" \"{Path.GetFileNameWithoutExtension(fileName) + preset.Format}\"";
        Console.WriteLine("args: " + arguments);
        ProcessStartInfo ffmpeg = new ProcessStartInfo
        {
            CreateNoWindow = false,
            UseShellExecute = false,
            FileName = @"ffmpeg\ffmpeg.exe",
            Arguments = arguments,
            RedirectStandardOutput = true,
            WorkingDirectory =  filePath
        };
        
        using (Process process = new Process())
        {
            process.StartInfo = ffmpeg;
            process.Start();
            process.WaitForExit();
        }
        
        return true;
    }
}