using Couchbase.Lite;
using AppSync.Services;

namespace AppSync;

public partial class App : Application
{
    public static Database? SharedDb { get; private set; }
    private static bool _databaseInitialized = false;
    private static CouchbaseService? _couchbaseService;
    private static SyncService? _syncService;

    public App()
    {
        InitializeComponent();
        Console.WriteLine("=== APP CONSTRUCTOR CALLED ===");

#if IOS
        Console.WriteLine("=== iOS: INITIALIZING DATABASE ===");
        InitializeDatabase();
#endif
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Console.WriteLine("=== CREATE WINDOW CALLED ===");
        
        // Always show login page - no persistence
        Console.WriteLine("=== SHOWING LOGIN PAGE ===");
        return new Window(new Views.LoginPage());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        Console.WriteLine("=== APP ONSTART CALLED ===");

#if ANDROID
        await Task.Delay(100);
        if (!_databaseInitialized)
        {
            InitializeDatabase();
        }
#endif
    }

    private void InitializeDatabase()
    {
        try
        {
            Console.WriteLine("=== INITIALIZE DATABASE CALLED ===");

            var config = new DatabaseConfiguration();
            Console.WriteLine($"=== DATABASE PATH: {config.Directory} ===");

            SharedDb = new Database("appsync-db", config);
            Console.WriteLine("=== DATABASE CREATED ===");

            _couchbaseService = CouchbaseService.Instance;
            _couchbaseService.Initialize(SharedDb);
            Console.WriteLine("=== COUCHBASE SERVICE INITIALIZED ===");

            _syncService = new SyncService(SharedDb);
            Console.WriteLine("=== SYNC SERVICE CREATED ===");

            _databaseInitialized = true;
            Console.WriteLine("=== DATABASE AND SERVICES INITIALIZED SUCCESSFULLY ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== DATABASE INIT FAILED: {ex.Message} ===");
            Console.WriteLine($"=== STACK TRACE: {ex.StackTrace} ===");
        }
    }

    public static void EnsureDatabaseInitialized()
    {
        if (!_databaseInitialized && Current is App app)
        {
            app.InitializeDatabase();
        }
    }

    public static CouchbaseService? GetCouchbaseService()
    {
        EnsureDatabaseInitialized();
        return _couchbaseService;
    }

    public static SyncService? GetSyncService()
    {
        EnsureDatabaseInitialized();
        return _syncService;
    }
}