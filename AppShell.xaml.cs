namespace AppSync;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Console.WriteLine("=== APPSHELL CONSTRUCTOR CALLED ===");
        
        // For iOS, services should be ready immediately
        // But let's double-check
        App.EnsureDatabaseInitialized();
        
        var couchbaseService = App.GetCouchbaseService();
        var syncService = App.GetSyncService();
        
        Console.WriteLine($"=== APPSHELL: CouchbaseService ready: {couchbaseService != null} ===");
        Console.WriteLine($"=== APPSHELL: SyncService ready: {syncService != null} ===");
    }
}