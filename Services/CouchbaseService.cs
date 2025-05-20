using System.Diagnostics;
using AppSync.Models;
using Couchbase.Lite;
using Couchbase.Lite.Query;

namespace AppSync.Services
{
   public class CouchbaseService
{
    public static CouchbaseService Instance { get; } = new CouchbaseService();

    public Database Db { get; private set; }
    private Collection _profileCollection;

    public void Initialize(Database sharedDb)
    {
        Db = sharedDb;
        EnsureCollectionExists("profiles", "employees");
        _profileCollection = Db.GetCollection("profiles", "employees");  // Cache collection
        Debug.WriteLine("CouchbaseService initialized with shared DB.");
    }

    private void EnsureCollectionExists(string collectionName, string scopeName)
    {
        try
        {
            Db.CreateCollection(collectionName, scopeName);
            Debug.WriteLine($"Collection '{scopeName}.{collectionName}' created or already exists.");
        }
        catch (CouchbaseLiteException ex)
        {
            Debug.WriteLine($"Collection creation skipped: {ex.Message}");
        }
    }

    public List<Profile> GetAllProfiles()
    {
        var profiles = new List<Profile>();

        if (_profileCollection == null)
        {
            Debug.WriteLine("Collection not found.");
            return profiles;
        }

        try
        {
            var query = QueryBuilder
                .Select(
                    SelectResult.Expression(Meta.ID).As("id"),
                    SelectResult.Property("name"),
                    SelectResult.Property("title"),
                    SelectResult.Property("email"))
                .From(DataSource.Collection(_profileCollection));

            var resultSet = query.Execute();
            foreach (var row in resultSet)
            {
                profiles.Add(new Profile
                {
                    Id = row.GetString("id") ?? string.Empty,
                    Name = row.GetString("name") ?? string.Empty,
                    Title = row.GetString("title") ?? string.Empty,
                    Email = row.GetString("email") ?? string.Empty
                });
            }

            Debug.WriteLine($"Retrieved {profiles.Count} profile(s).");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Query error: {ex.Message}");
        }

        return profiles;
    }

    public void DeleteProfile(string id)
    {
        var doc = _profileCollection?.GetDocument(id);
        if (doc != null)
        {
            _profileCollection.Delete(doc);
            Debug.WriteLine($"Deleted document with ID: {id}");
        }
        else
        {
            Debug.WriteLine($"Document with ID {id} not found.");
        }
    }

    public void SaveProfile(Profile profile)
    {
        var doc = new MutableDocument(profile.Id);
        doc.SetString("name", profile.Name);
        doc.SetString("title", profile.Title);
        doc.SetString("email", profile.Email);
        _profileCollection.Save(doc);
        Debug.WriteLine($"Saved/updated document with ID: {profile.Id}");
    }
}

}
