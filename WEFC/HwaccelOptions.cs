using System.Collections.ObjectModel;

namespace WEFC;

public class HwaccelOptions : ObservableCollection<string>
{
    public HwaccelOptions()
    {
        Add("qsv");
        Add("cuda");
        Add("none");
    }
}