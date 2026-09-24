using System.Collections.ObjectModel;

namespace WEMC;

public class HwaccelOptions : ObservableCollection<string>
{
    public HwaccelOptions()
    {
        Add("qsv");
        Add("cuda");
        Add("none");
    }
}