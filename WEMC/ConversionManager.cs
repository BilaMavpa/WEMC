using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace WEMC;

public class ConversionManager
{
    private static ConversionManager? _instance = null;
    private static readonly Lock Lock = new Lock();
    public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
    
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
    
    private readonly string _ffprobePath = Environment.GetEnvironmentVariable("PATH")
        ?.Split(Path.PathSeparator)
        .Select(dir => Path.Combine(dir, "ffprobe.exe"))
        .FirstOrDefault(File.Exists);
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

    public bool ConvertFile(ConversionArgs ca, IProgress<string> progress,CancellationToken cancellationToken)
    {
        Console.WriteLine(_ffmpegPath);
        string arguments = $"{(!ca.Hwaccel.Equals("none") ? "-hwaccel " + ca.Hwaccel + " " : "")}" +
            $"{(!ca.StartTime.Equals("") ? $"-ss \"{ca.StartTime}\" " : "")}" +
            $"{(!ca.Duration.Equals("") ? $"-t \"{ca.Duration}\" " : "")}" +
            $"-i \"{FileName}\" " + string.Join(" ", ca.Args) +
            " -b:a 48k " +
            $"{(ca.UseSize ? " -b:v " +
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
            RedirectStandardError = true,
            RedirectStandardInput = true,
            WorkingDirectory =  FilePath
        };
        
        using (Process process = new Process())
        {
            process.StartInfo = ffmpeg;
            process.Start();
            while (!process.StandardError.EndOfStream)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException e)
                {
                    process.Kill();
                    throw;
                }
                
                string line = process.StandardError.ReadLine();
                if (line.Contains("Error"))
                {
                    process.Kill();
                    throw new Exception(line);
                }
                if (line.Contains("frame="))
                {
                    progress.Report(line);
                }
                else
                {
                    Console.WriteLine(line);
                }
            }
            process.WaitForExit();
        }
        
        return true;
    }
    
    public bool ConvertFile(Preset preset)
    {
        string arguments;
        
        if (preset.Name.Equals("merge all audio"))
        {
            arguments = $"-i \"{FileName}\" " + GetArgsForMergingAllAudio() + " -c:v copy \"converted " + fileName + "\"";
        }
        else
        {
            arguments = $"{(!preset.Hwaccel.Equals("") ? "-hwaccel " + preset.Hwaccel : "")}" +
                               $"{(!preset.FirstArg.Equals("") ? preset.FirstArg + " " : "")} -i \"{FileName}\" " +
                               string.Join(" ", preset.Args) +
                               $" \"converted {Path.GetFileNameWithoutExtension(FileName) + preset.Format}\"";
        }
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

    private int GetAudioTrackAmount()
    {
        string tracks;
        ProcessStartInfo ffprobe = new ProcessStartInfo
        {
            CreateNoWindow = false,
            UseShellExecute = false,
            FileName = _ffprobePath,
            Arguments = $"-v error -select_streams a -show_entries stream=index -of csv=p=0 \"{FileName}\"",
            RedirectStandardOutput = true,
            WorkingDirectory = FilePath
        };
            
        using (Process process = new Process())
        {
            process.StartInfo = ffprobe;
            process.Start();
            tracks = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }
        return tracks.Count(char.IsDigit);
    }

    public string GetArgsForMergingAllAudio()
    {
        int amount = GetAudioTrackAmount();
                    
        if (amount == 0)
        {
            throw new Exception("no audio present");
        }

        string args = "-filter_complex \"";
        for (int i = 1; i < amount+1; i++)
        {
            args += $"[0:{i}]";
        }
                    
        args += $"amerge=inputs={amount}[a]\" -ac 2 -map 0:v -map \"[a]\"";
        return args;
    }

    public string GetArgsForKeepingAllAudio()
    {
        int amount = GetAudioTrackAmount();
        
        if (amount == 0)
        {
            throw new Exception("no audio present");
        }
        
        string args = "-map 0:0";
        for (int i = 1; i < amount+1; i++)
        {
            args += $" -map 0:{i}";
        }
        return args;
    }
    
}