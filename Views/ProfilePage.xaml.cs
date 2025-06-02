using AppSync.Models;
using AppSync.Services;
using System.Collections.ObjectModel;
using Couchbase.Lite.Sync;
using Couchbase.Lite;


namespace AppSync.Views;

public partial class ProfilePage : ContentPage
{
    public ObservableCollection<Profile> Profiles { get; set; } = new();
    public Command<Profile> EditCommand { get; }
    public Command<Profile> DeleteCommand { get; }
    private bool _isLoadingProfiles = false;

  public ProfilePage()
{
    try
    {
        Console.WriteLine("=== PROFILEPAGE CONSTRUCTOR START ===");
        
        InitializeComponent();
        Console.WriteLine("=== PROFILEPAGE: InitializeComponent DONE ===");
        
        BindingContext = this;
        Console.WriteLine("=== PROFILEPAGE: BindingContext SET ===");

        EditCommand = new Command<Profile>(EditProfile);
        DeleteCommand = new Command<Profile>(DeleteProfile);
        Console.WriteLine("=== PROFILEPAGE: Commands CREATED ===");

        LoadProfiles();
        Console.WriteLine("=== PROFILEPAGE: LoadProfiles CALLED ===");
        
        SetupRealTimeSyncListener();
        Console.WriteLine("=== PROFILEPAGE: SetupRealTimeSyncListener CALLED ===");
        
        Console.WriteLine("=== PROFILEPAGE CONSTRUCTOR COMPLETE ===");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== PROFILEPAGE CONSTRUCTOR ERROR: {ex.Message} ===");
        Console.WriteLine($"=== STACK TRACE: {ex.StackTrace} ===");
        throw;
    }
}

    private void SetupRealTimeSyncListener()
    {
        try
        {
            var syncService = App.GetSyncService();
            if (syncService != null)
            {
                syncService.Replicator.AddDocumentReplicationListener(OnDocumentSyncChanged);
                Console.WriteLine("=== REAL-TIME SYNC LISTENER ADDED SAFELY ===");
            }
            else
            {
                Console.WriteLine("=== SYNC SERVICE NOT AVAILABLE YET ===");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR ADDING SYNC LISTENER: {ex.Message} ===");
        }
    }

    private void OnDocumentSyncChanged(object? sender, DocumentReplicationEventArgs e)
    {
        // Only refresh UI for pull operations (Capella → Device)
        if (!e.IsPush && e.Documents.Count > 0)
        {
            // Log what we're receiving
            foreach (var doc in e.Documents)
            {
                Console.WriteLine($"=== SYNC PULL: Document ID: '{doc.Id}' ===");
            }

            // Use MainThread.InvokeOnMainThreadAsync for safer threading
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    Console.WriteLine($"=== REAL-TIME: {e.Documents.Count} DOCUMENTS PULLED FROM CAPELLA ===");
                    
                    // Small delay to ensure database is updated
                    await Task.Delay(200);
                    
                    // Only refresh if not already loading
                    if (!_isLoadingProfiles)
                    {
                        LoadProfiles();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"=== ERROR IN SYNC LISTENER: {ex.Message} ===");
                }
            });
        }
        else if (e.IsPush)
        {
            // Log push operations but don't refresh UI
            Console.WriteLine($"=== SYNC PUSH: {e.Documents.Count} documents to Capella ===");
            foreach (var doc in e.Documents)
            {
                if (doc.Error == null)
                {
                    Console.WriteLine($"=== SYNC PUSH SUCCESS: {doc.Id} ===");
                }
                else
                {
                    Console.WriteLine($"=== SYNC PUSH ERROR: {doc.Id} - {doc.Error} ===");
                }
            }
        }
    }

    private async void LoadProfiles()
{
    if (_isLoadingProfiles) return;
    
    try
    {
        _isLoadingProfiles = true;
        var couchbaseService = App.GetCouchbaseService();
        if (couchbaseService != null)
        {
            var docs = await Task.Run(() => couchbaseService.GetAllProfiles());
            
            // Update UI on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Profiles.Clear();
                foreach (var doc in docs)
                {
                    Profiles.Add(doc);
                }
                
                // Update the count label
                if (FindByName("CountLabel") is Label countLabel)
                {
                    countLabel.Text = $"{Profiles.Count} employees";
                }
                
                Console.WriteLine($"=== UI REFRESHED WITH {Profiles.Count} PROFILES ===");
            });
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== ERROR LOADING PROFILES: {ex.Message} ===");
    }
    finally
    {
        _isLoadingProfiles = false;
    }
}

    private async void EditProfile(Profile profile)
    {
        try
        {
            var editPage = new EditProfilePage(profile);
            await Navigation.PushAsync(editPage);
            Console.WriteLine($"=== NAVIGATING TO EDIT PAGE FOR: {profile.Id} ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR NAVIGATING TO EDIT: {ex.Message} ===");
            await DisplayAlert("Error", "Failed to open edit page", "OK");
        }
    }

    private async void DeleteProfile(Profile profile)
    {
        try
        {
            // Check if profile has a valid ID
            if (string.IsNullOrEmpty(profile.Id))
            {
                Console.WriteLine($"=== CANNOT DELETE PROFILE: EMPTY ID - NAME: {profile.Name} ===");
                await DisplayAlert("Error", $"Cannot delete {profile.Name}: Profile has no ID", "OK");
                return;
            }

            var confirm = await DisplayAlert("Delete", $"Delete {profile.Name} (ID: {profile.Id})?", "Yes", "No");
            if (confirm)
            {
                var couchbaseService = App.GetCouchbaseService();
                if (couchbaseService != null)
                {
                    Console.WriteLine($"=== DELETING PROFILE {profile.Id} FROM DEVICE ===");
                    
                    // Delete from database first
                    await Task.Run(() => couchbaseService.DeleteProfile(profile.Id));
                    
                    // Remove from UI after successful database deletion
                    Profiles.Remove(profile);
                    
                    Console.WriteLine($"=== DELETED PROFILE {profile.Id} SUCCESSFULLY ===");
                }
                else
                {
                    await DisplayAlert("Error", "Database service not available", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR DELETING PROFILE {profile.Id}: {ex.Message} ===");
            await DisplayAlert("Error", $"Failed to delete {profile.Name}", "OK");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh when returning to page
        LoadProfiles();
        
        // Update count label
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (FindByName("CountLabel") is Label countLabel)
            {
                countLabel.Text = $"{Profiles.Count} employees";
            }
        });
    }

private async void OnLogoutClicked(object sender, EventArgs e)
{
    var confirm = await DisplayAlert("Logout", "Are you sure?", "Yes", "No");
    if (confirm)
    {
        Preferences.Remove("IsLoggedIn");
        Preferences.Remove("Username");
        Application.Current.MainPage = new Views.LoginPage();
    }
}
}