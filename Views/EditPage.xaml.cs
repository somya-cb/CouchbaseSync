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
            
            // Update profile with new values
            var updatedProfile = new AppSync.Models.Profile
            {
                Id = _profile.Id, // Keep the same ID
                Name = NameEntry.Text?.Trim() ?? "",
                Title = TitleEntry.Text?.Trim() ?? "",
                Email = EmailEntry.Text?.Trim() ?? ""
            };

            var couchbaseService = App.GetCouchbaseService();
            if (couchbaseService != null)
            {
                await Task.Run(() => couchbaseService.SaveProfile(updatedProfile));
                StatusLabel.Text = "Saved successfully!";
                
                // Wait a moment then go back
                await Task.Delay(1000);
                Application.Current.MainPage = new Views.ProfilePage();
            }
            else
            {
                StatusLabel.Text = "Database service not available";
            }
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
