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

#if IOS
        InitializeDatabase();
#endif
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Always show login page on app launch
        return new Window(new Views.LoginPage());
    }

    protected override async void OnStart()
    {
        base.OnStart();

#if ANDROID
        // Android needs slight delay for context to be ready
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
            var config = new DatabaseConfiguration();
            SharedDb = new Database("appsync-db", config);

            _couchbaseService = CouchbaseService.Instance;
            _couchbaseService.Initialize(SharedDb);

            _syncService = new SyncService(SharedDb);

            _databaseInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.Message}");
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