using Couchbase.Lite;
using Couchbase.Lite.Sync;
using System;
using System.Diagnostics;

namespace AppSync.Services
{
    public class SyncService
    {
        private readonly Database _database;
        private readonly Replicator _replicator;
        public Replicator Replicator => _replicator;


        public SyncService(Database sharedDb)
        {
            _database = sharedDb;

            var collection = _database.GetCollection("profiles", "employees");
            if (collection == null)
            {
                throw new InvalidOperationException("Collection 'profiles' in scope 'employees' does not exist.");
            }

            var syncGatewayUrl = new Uri("wss://gp0d0chcso9jjubq.apps.cloud.couchbase.com:4984/test-endpoint");
            var target = new URLEndpoint(syncGatewayUrl);

            var config = new ReplicatorConfiguration(target)
            {
                ReplicatorType = ReplicatorType.PushAndPull,
                Continuous = true,
                Authenticator = new BasicAuthenticator("test-user", "Appservices@123")
            };

            config.AddCollection(collection);

            _replicator = new Replicator(config);
            _replicator.AddChangeListener(OnReplicatorStatusChanged);
            _replicator.AddDocumentReplicationListener(OnDocumentReplication);

            Debug.WriteLine("Starting replicator...");
            _replicator.Start();
        }

private void OnReplicatorStatusChanged(object? sender, ReplicatorStatusChangedEventArgs e)
{
    var status = e.Status;
    Console.WriteLine($"[Sync] Status: {status.Activity}, Completed: {status.Progress.Completed}, Total: {status.Progress.Total}");

    // Add this debug
    if (status.Activity == ReplicatorActivityLevel.Idle && status.Progress.Completed == status.Progress.Total)
    {
        Console.WriteLine($"[Sync] INITIAL SYNC COMPLETE - Total documents: {status.Progress.Total}");
    }

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
        if (doc.Error == null)
        {
            Console.WriteLine($"[Sync] Synced document ID: {doc.Id}");
            
            // Debug: If pulling, show document content
            if (!e.IsPush && doc.Id != null)
            {
                var localDoc = _database.GetDefaultCollection()?.GetDocument(doc.Id);
                if (localDoc != null)
                {
                    Console.WriteLine($"[Sync] Document content - id property: {localDoc.GetString("id")}, name: {localDoc.GetString("name")}");
                }
            }
        }
        else
        {
            Console.WriteLine($"[Sync] Error syncing doc {doc.Id}: {doc.Error}");
        }
    }
}

        public Collection GetCollection() => _database.GetCollection("profiles", "employees")
                                              ?? throw new InvalidOperationException("Collection not found.");

        public Database GetDatabase() => _database;
    }
}
