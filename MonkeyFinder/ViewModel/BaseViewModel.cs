namespace MonkeyFinder.ViewModel;

//public class BaseViewModel : INotifyPropertyChanged
//{
//    bool isBusy;
//    string title;

//    public bool IsBusy
//    {
//        get => isBusy;
//        set
//        {
//            if (isBusy == value)
//                return;

//            isBusy = value;
//            OnPropertyChanged();
//            OnPropertyChanged(nameof(IsNotBusy));
//        }
//    }
//    public string Title { get => title;

//        set
//        {
//            if (title == value)
//                return;
//            title = value;
//            OnPropertyChanged();
//        } 
//    }

//    public bool IsNotBusy => !IsBusy;

//    public event PropertyChangedEventHandler PropertyChanged;

//    public void OnPropertyChanged([CallerMemberName] string name = null)
//    {
//        // ?. == if not null (syntactic sugar) invoke it
//        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//    }
//}

public partial class BaseViewModel : ObservableObject
{
    public BaseViewModel()
    {

    }

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsNotBusy))]
    bool isBusy;

    [ObservableProperty]
    string title;

    public bool IsNotBusy => !IsBusy;
}
