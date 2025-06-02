using AppSync.Models;
using AppSync.Services;

namespace AppSync.Views;

public partial class EditProfilePage : ContentPage
{
    public Profile Profile { get; set; }
    
    public EditProfilePage(Profile profile)
    {
        InitializeComponent();
        
        // Create a copy of the profile to edit
        Profile = new Profile
        {
            Id = profile.Id,
            Name = profile.Name,
            Title = profile.Title,
            Email = profile.Email
        };
        
        BindingContext = this;
        Console.WriteLine($"=== EDITING PROFILE: {Profile.Id} - {Profile.Name} ===");
    }
    
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Error", "Name is required", "OK");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await DisplayAlert("Error", "Email is required", "OK");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(TitleEntry.Text))
            {
                await DisplayAlert("Error", "Job Title is required", "OK");
                return;
            }
            
            // Update profile with new values
            Profile.Name = NameEntry.Text.Trim();
            Profile.Title = TitleEntry.Text.Trim();
            Profile.Email = EmailEntry.Text.Trim();
            
            // Save to database
            var couchbaseService = App.GetCouchbaseService();
            if (couchbaseService != null)
            {
                couchbaseService.SaveProfile(Profile);
                Console.WriteLine($"=== SAVED PROFILE: {Profile.Id} - {Profile.Name} ===");
                
                await DisplayAlert("Success", $"Profile updated successfully!", "OK");
                await Navigation.PopAsync(); // Go back to profiles list
            }
            else
            {
                await DisplayAlert("Error", "Database service not available", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR SAVING PROFILE: {ex.Message} ===");
            await DisplayAlert("Error", "Failed to save profile. Please try again.", "OK");
        }
    }
    
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Cancel", "Discard changes?", "Yes", "No");
        if (result)
        {
            await Navigation.PopAsync(); // Go back without saving
        }
    }
}