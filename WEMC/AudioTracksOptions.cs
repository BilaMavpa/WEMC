using System.Collections.ObjectModel;

namespace WEMC;

public class AudioTracksOptions : ObservableCollection<string>
{
    public AudioTracksOptions()
    {
        Add("Keep same");
        Add("Only Keep 1");
        Add("Merge 1 + 2");
        Add("Merge all");
        Add("None");
    }
}