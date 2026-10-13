namespace WEMC;

public class ConversionArgs
{
    public string Name { get; set; }
    public string OutputFormat { get; set; }
    public string StartTime { get; set; }
    public string Duration { get; set; }
    public string Hwaccel { get; set; }
    public bool UseSize { get; set; }
    public float Size { get; set; }
    public string TrackOption { get; set; }
    public string[] Args { get; set; }
    
}