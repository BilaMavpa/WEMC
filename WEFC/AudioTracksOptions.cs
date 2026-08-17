using System.Collections.ObjectModel;

namespace WEFC;

public class AudioTracksOptions : ObservableCollection<string>
{
    public AudioTracksOptions()
    {
        Add("Keep same");
        Add("Only Keep 1");
        Add("Merge 1 + 2");
        Add("Merge all");
    }
}