using Couchbase.Lite;
using Couchbase.Lite.Sync;

namespace AppSync.Services
{
    public class DocumentPushResultEventArgs : EventArgs
    {
        public string DocumentId { get; init; } = string.Empty;
        public Exception? Error { get; init; }
        public bool Success => Error == null;
    }

    public class SyncService
    {
        public event EventHandler<DocumentPushResultEventArgs>? DocumentPushCompleted;

        private readonly Database _database;
        private readonly Replicator _replicator;
        private readonly ListenerToken _statusListenerToken;
        private readonly ListenerToken _docListenerToken;
        public Replicator Replicator => _replicator;

        public SyncService(Database sharedDb, string username, string password)
        {
            _database = sharedDb;

            var collection = _database.GetCollection("profiles", "employees");
            if (collection == null)
            {
                throw new InvalidOperationException("Collection 'profiles' in scope 'employees' does not exist.");
            }

            var syncGatewayUrl = new Uri("wss://9k3gg8a0v8ikcgn.apps.cloud.couchbase.com:4984/test-endpoint");
            var target = new URLEndpoint(syncGatewayUrl);

            var collectionConfig = new CollectionConfiguration(collection);

            var config = new ReplicatorConfiguration(
                new[] { collectionConfig },
                target
            )
            {
                ReplicatorType = ReplicatorType.PushAndPull,
                Continuous = true,
                Authenticator = new BasicAuthenticator(username, password)
            };

            _replicator = new Replicator(config);
            _statusListenerToken = _replicator.AddChangeListener(OnReplicatorStatusChanged);
            _docListenerToken = _replicator.AddDocumentReplicationListener(OnDocumentReplication);

            _replicator.Start();
        }

        public async Task StopAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var stopToken = _replicator.AddChangeListener((_, args) =>
            {
                if (args.Status.Activity == ReplicatorActivityLevel.Stopped)
                    tcs.TrySetResult(true);
            });

            _replicator.Stop();
            await Task.WhenAny(tcs.Task, Task.Delay(5000));

            _replicator.RemoveChangeListener(stopToken);
            _replicator.RemoveChangeListener(_statusListenerToken);
            _replicator.RemoveChangeListener(_docListenerToken);
            _replicator.Dispose();
        }

        private void OnReplicatorStatusChanged(object? sender, ReplicatorStatusChangedEventArgs e)
        {
            var status = e.Status;
            Console.WriteLine($"[Sync] Status: {status.Activity}, Completed: {status.Progress.Completed}, Total: {status.Progress.Total}");

            if (status.Error != null)
            {
                Console.WriteLine($"[Sync] Error: {status.Error}");
            }
        }

        private void OnDocumentReplication(object? sender, DocumentReplicationEventArgs e)
        {
            var direction = e.IsPush ? "Push" : "Pull";
            Console.WriteLine($"[Sync] {direction} - {e.Documents.Count} documents");

            foreach (var doc in e.Documents)
            {
                if (doc.Error != null)
                {
                    Console.WriteLine($"[Sync] Error syncing doc {doc.Id}: {doc.Error}");
                }

                if (e.IsPush)
                {
                    DocumentPushCompleted?.Invoke(this, new DocumentPushResultEventArgs
                    {
                        DocumentId = doc.Id,
                        Error = doc.Error
                    });
                }
            }
        }

        public Collection GetCollection() => _database.GetCollection("profiles", "employees")
                                              ?? throw new InvalidOperationException("Collection not found.");

        public Database GetDatabase() => _database;
    }
}