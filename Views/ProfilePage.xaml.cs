namespace AppSync.Views;

public partial class ProfilePage : ContentPage
{
    private List<AppSync.Models.Profile> _allProfiles = new();

    public ProfilePage()
    {
        InitializeComponent();
        SetupSyncListener();
        LoadProfiles();
    }

    private void SetupSyncListener()
    {
        try
        {
            var syncService = App.GetSyncService();
            if (syncService != null)
            {
                syncService.Replicator.AddDocumentReplicationListener(OnDocumentSyncChanged);
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Sync setup error: {ex.Message}";
        }
    }

    private void OnDocumentSyncChanged(object? sender, Couchbase.Lite.Sync.DocumentReplicationEventArgs e)
    {
        if (!e.IsPush && e.Documents.Count > 0)
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Task.Delay(200);
                LoadProfiles();
            });
        }
    }

    private async void LoadProfiles()
    {
        try
        {
            StatusLabel.Text = "Loading profiles...";
            
            var couchbaseService = App.GetCouchbaseService();
            if (couchbaseService != null)
            {
                _allProfiles = await Task.Run(() => couchbaseService.GetAllProfiles());
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusLabel.Text = $"Found {_allProfiles.Count} profiles";
                    DisplayProfiles();
                });
            }
            else
            {
                StatusLabel.Text = "Database not ready";
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private void DisplayProfiles()
    {
        ProfilesContainer.Children.Clear();
        
        foreach (var profile in _allProfiles)
        {
            var profileFrame = new Frame
            {
                BackgroundColor = Colors.White,
                Padding = 8,
                Margin = new Thickness(0, 2),
                CornerRadius = 5,
                HasShadow = false
            };

            var mainGrid = new Grid
            {
                ColumnDefinitions = 
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions = 
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            // Profile info (left side)
            var nameLabel = new Label 
            { 
                Text = $"{profile.Name} ({profile.Id})", 
                FontAttributes = FontAttributes.Bold,
                FontSize = 14
            };
            
            var titleLabel = new Label 
            { 
                Text = profile.Title, 
                FontSize = 12,
                TextColor = Colors.Gray
            };
            
            var emailLabel = new Label 
            { 
                Text = profile.Email, 
                FontSize = 11,
                TextColor = Colors.DarkGray
            };

            Grid.SetColumn(nameLabel, 0);
            Grid.SetRow(nameLabel, 0);
            
            Grid.SetColumn(titleLabel, 0);
            Grid.SetRow(titleLabel, 1);
            
            Grid.SetColumn(emailLabel, 0);
            Grid.SetRow(emailLabel, 2);

            // Buttons (right side)
            var buttonsStack = new StackLayout 
            { 
                Orientation = StackOrientation.Horizontal,
                Spacing = 5,
                VerticalOptions = LayoutOptions.Center
            };

            // Edit button
            var editFrame = new Frame
            {
                BackgroundColor = Colors.Orange,
                Padding = new Thickness(8, 4),
                CornerRadius = 3,
                HasShadow = false
            };
            
            editFrame.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => EditProfile(profile))
            });
            
            editFrame.Content = new Label 
            { 
                Text = "Edit", 
                TextColor = Colors.White, 
                FontSize = 10,
                HorizontalOptions = LayoutOptions.Center
            };

            // Delete button
            var deleteFrame = new Frame
            {
                BackgroundColor = Colors.Red,
                Padding = new Thickness(8, 4),
                CornerRadius = 3,
                HasShadow = false
            };
            
            deleteFrame.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await DeleteProfileWithConfirmation(profile))
            });
            
            deleteFrame.Content = new Label 
            { 
                Text = "Delete", 
                TextColor = Colors.White, 
                FontSize = 10,
                HorizontalOptions = LayoutOptions.Center
            };

            buttonsStack.Children.Add(editFrame);
            buttonsStack.Children.Add(deleteFrame);

            Grid.SetColumn(buttonsStack, 1);
            Grid.SetRow(buttonsStack, 0);
            Grid.SetRowSpan(buttonsStack, 3);

            mainGrid.Children.Add(nameLabel);
            mainGrid.Children.Add(titleLabel);
            mainGrid.Children.Add(emailLabel);
            mainGrid.Children.Add(buttonsStack);
            
            profileFrame.Content = mainGrid;
            ProfilesContainer.Children.Add(profileFrame);
        }
    }

    private void EditProfile(AppSync.Models.Profile profile)
    {
        Application.Current.MainPage = new Views.EditPage(profile);
    }

    private async Task DeleteProfileWithConfirmation(AppSync.Models.Profile profile)
    {
        try
        {
            bool confirmed = await DisplayAlert(
                "Delete Profile", 
                $"Are you sure you want to delete {profile.Name}?", 
                "Yes", 
                "No"
            );

            if (confirmed)
            {
                var couchbaseService = App.GetCouchbaseService();
                if (couchbaseService != null)
                {
                    await Task.Run(() => couchbaseService.DeleteProfile(profile.Id));
                    LoadProfiles();
                    StatusLabel.Text = $"Deleted {profile.Name}";
                }
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Delete error: {ex.Message}";
        }
    }

    private void OnRefreshTapped(object sender, EventArgs e)
    {
        LoadProfiles();
    }

    private void OnLogoutTapped(object sender, EventArgs e)
    {
        Application.Current.MainPage = new Views.LoginPage();
    }
}
