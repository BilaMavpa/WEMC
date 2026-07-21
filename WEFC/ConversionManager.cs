using System.Diagnostics;
using System.IO;

namespace WEFC;

public class ConversionManager
{
    private static ConversionManager? _instance = null;
    private static readonly Lock Lock = new Lock();
    
    private ConversionManager()
    {
        
    }

    public static ConversionManager Instance
    {
        get
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    _instance = new ConversionManager();
                }
                return _instance;
            }
        }
    }
    
    private string filePath { get; set; }
    private string fileName { get; set; }

    private readonly string _ffmpegPath = Environment.GetEnvironmentVariable("PATH")
        ?.Split(Path.PathSeparator)
        .Select(dir => Path.Combine(dir, "ffmpeg.exe"))
        .FirstOrDefault(File.Exists);
    
    public string FilePath => filePath;
    public string FileName => fileName;

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
        filePath = fullFilePath[..lastPosition];
        fileName = fullFilePath[lastPosition..];

    }

    public bool ConvertFile(ConversionArgs ca)
    {
        
        string arguments = $"{(!ca.Hwaccel.Equals("none") ? "-hwaccel " + ca.Hwaccel + " " : "")}" +
            $"{(!ca.StartTime.Equals("") ? $"-ss \"{ca.StartTime}\" " : "")}" +
            $"{(!ca.Duration.Equals("") ? $"-t \"{ca.Duration}\" " : "")}" +
            $"-i \"{FileName}\" " + string.Join(" ", ca.Args) +
            $"{(ca.UseSize ? " -b:a 48k -b:v " + 
            ((ca.Size * 8192) / TimeSpan.Parse(ca.Duration).TotalSeconds - 128) + "k " : "")}" +
            $" \"{ca.Name + ca.OutputFormat}\"";
        
        Console.WriteLine("args: " + arguments);
        
        ProcessStartInfo ffmpeg = new ProcessStartInfo
        {
            CreateNoWindow = false,
            UseShellExecute = false,
            FileName = _ffmpegPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            WorkingDirectory =  FilePath
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
        
        string arguments = $"-hwaccel cuda {(!preset.FirstArg.Equals("") ? preset.FirstArg + " " : "")} -i \"{FileName}\" " + string.Join(" ", preset.Args)+
                           $" \"{Path.GetFileNameWithoutExtension(FileName) + preset.Format}\"";
        Console.WriteLine("args: " + arguments);
        
        ProcessStartInfo ffmpeg = new ProcessStartInfo
        {
            CreateNoWindow = false,
            UseShellExecute = false,
            FileName = _ffmpegPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            WorkingDirectory =  FilePath
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