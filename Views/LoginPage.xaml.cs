namespace AppSync.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter username and password", "OK");
            return;
        }

        LoadingIndicator.IsVisible = true;

        try
        {
            if (username == "test-user" && password == "Appservices@123")
            {
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Error", "Invalid credentials", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Login failed", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }
}