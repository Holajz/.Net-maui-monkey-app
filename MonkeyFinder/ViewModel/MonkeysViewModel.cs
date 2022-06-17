using MonkeyFinder.Services;
using System.Collections.Specialized;

namespace MonkeyFinder.ViewModel;


public partial class RangeEnabledObservableCollection<T> : ObservableCollection<T>
{
    public void InsertRange(IEnumerable<T> items)
    {
        this.CheckReentrancy();
        foreach (var item in items)
            this.Items.Add(item);
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
    }
}
public partial class MonkeysViewModel : BaseViewModel
{
    MonkeyService monkeyService;
    IConnectivity connectivity;
    IGeolocation geolocation;

    public RangeEnabledObservableCollection<Monkey> Monkeys { get;} = new RangeEnabledObservableCollection<Monkey>();
    public MonkeysViewModel(MonkeyService monkeyService, IConnectivity connectivity, IGeolocation geolocation)
    {
        Title = "Monkey Finder";
        this.monkeyService = monkeyService;
        this.connectivity = connectivity;
        this.geolocation = geolocation;
    }

    [ICommand]
    async Task GoToDetailsAsync(Monkey monkey)
    {
        if (monkey == null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(DetailsPage)}?id={monkey.Name}", true,
            new Dictionary<string, object>
            {
                {"Monkey", monkey}
            });
    }

    [ICommand]
    async Task GetClosestMonkey()
    {
        if (IsBusy || Monkeys.Count == 0)
            return;

        try
        {
            // Get cached location, else get real location.
            var location = await geolocation.GetLastKnownLocationAsync();
            if (location == null)
            {
                location = await geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(30)
                });
            }

            // Find closest monkey to us
            var first = Monkeys.OrderBy(m => location.CalculateDistance(
                new Location(m.Latitude, m.Longitude), DistanceUnits.Miles))
                .FirstOrDefault();

            await Shell.Current.DisplayAlert("", first.Name + " " +
                first.Location, "OK");

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to query location: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    [ObservableProperty]
    bool isRefreshing;

    [ICommand]
    async Task GetMonkeysAsync()
    {
        if (IsBusy)
            return;

        if (connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert("No connectivity!",
                $"Please check internet and try again.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var monkeys = await monkeyService.GetMonkeys();

            if (Monkeys.Count != 0)
                Monkeys.Clear();

            Monkeys.InsertRange(monkeys);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get monkeys: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }

    }
}
