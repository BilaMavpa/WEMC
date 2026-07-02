using System.Diagnostics;
using System.IO;

namespace WEFC;

public class FileInfo
{
    private static FileInfo? _instance = null;
    private static readonly Lock Lock = new Lock();
    
    private FileInfo()
    {
        
    }

    public static FileInfo Instance
    {
        get
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    _instance = new FileInfo();
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
        // Console.WriteLine("Path: " + FilePath);
        // Console.WriteLine("Name: " + FileName);

        // ConvertFile(".mp3", []);
    }

    public bool ConvertFile(string outputFormat, string[] args)
    {
        string arguments = $"-hwaccel cuda -i \"{fileName}\" " + string.Join(" ", args) + $"\"{Path.GetFileNameWithoutExtension(fileName) + outputFormat}\"";
        
        ProcessStartInfo ffmpeg = new ProcessStartInfo
        {
            CreateNoWindow = false,
            UseShellExecute = false,
            FileName = @"ffmpeg\ffmpeg.exe",
            Arguments = arguments,
            RedirectStandardOutput = true,
            WorkingDirectory =  filePath
        };
        
        using (var process = new Process())
        {
            process.StartInfo = ffmpeg;
            process.Start();
            process.WaitForExit();
        }

        // Console.WriteLine("Press any key to continue...");
        // Console.ReadKey();
        
        return true;
    }
}