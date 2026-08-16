using System.Collections.ObjectModel;

namespace WEFC;

public class AudioTracksOptions : ObservableCollection<string>
{
    public AudioTracksOptions()
    {
        Add("1 + 2");
        Add("All");
    }
}