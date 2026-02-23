namespace AppSync.Views;

public partial class EditPage : ContentPage
{
    private readonly AppSync.Models.Profile _profile;

    public EditPage(AppSync.Models.Profile profile)
    {
        InitializeComponent();
        _profile = profile;
        LoadProfileData();
    }

    private void LoadProfileData()
    {
        IdLabel.Text = _profile.Id;
        NameEntry.Text = _profile.Name;
        TitleEntry.Text = _profile.Title;
        EmailEntry.Text = _profile.Email;
    }

    private async void OnSaveTapped(object sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Saving...";

            var updatedProfile = new AppSync.Models.Profile
            {
                Id = _profile.Id,
                Name = NameEntry.Text?.Trim() ?? "",
                Title = TitleEntry.Text?.Trim() ?? "",
                Email = EmailEntry.Text?.Trim() ?? ""
            };

            var couchbaseService = App.GetCouchbaseService();
            var syncService = App.GetSyncService();

            if (couchbaseService == null)
            {
                StatusLabel.Text = "Database service not available";
                return;
            }

            await Task.Run(() => couchbaseService.SaveProfile(updatedProfile));
            StatusLabel.Text = "Saved locally, syncing to Capella...";

            if (syncService != null)
            {
                var tcs = new TaskCompletionSource<string>();
                EventHandler<AppSync.Services.DocumentPushResultEventArgs>? handler = null;
                handler = (s, args) =>
                {
                    if (args.DocumentId == updatedProfile.Id)
                    {
                        tcs.TrySetResult(args.Success
                            ? "Synced to Capella!"
                            : $"Sync failed: {args.Error?.Message}");
                    }
                };

                syncService.DocumentPushCompleted += handler;
                var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
                syncService.DocumentPushCompleted -= handler;

                StatusLabel.Text = completed == tcs.Task
                    ? tcs.Task.Result
                    : "Saved locally (sync pending...)";
            }
            else
            {
                StatusLabel.Text = "Saved locally!";
            }

            await Task.Delay(1500);
            Application.Current.MainPage = new Views.ProfilePage();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Save error: {ex.Message}";
        }
    }

    private void OnCancelTapped(object sender, EventArgs e)
    {
        // Go back without saving
        Application.Current.MainPage = new Views.ProfilePage();
    }
}
