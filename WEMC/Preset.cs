namespace WEMC;

public class Preset
{
    public string Name { get; set; }
    public string Format { get; set; }
    public string FirstArg { get; set; }
    public string Hwaccel { get; set; }
    public List<string> Args { get; set; }
    public bool ShowProgress { get; set; }
    public bool KeepAudio { get; set; }
    public bool KeepBitrate { get; set; }
}