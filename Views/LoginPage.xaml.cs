namespace AppSync.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ((Label)((Frame)sender).Content).Text = "Enter credentials";
            return;
        }

        if (username == "test-user" && password == "Appservices@123")
        {
            // Navigate to ProfilePage (not TestPage!)
            Application.Current.MainPage = new Views.ProfilePage();
        }
        else
        {
            ((Label)((Frame)sender).Content).Text = "Invalid login";
        }
    }
}
