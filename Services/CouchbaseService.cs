using AppSync.Models;
using Couchbase.Lite;
using Couchbase.Lite.Query;

namespace AppSync.Services
{
    public class CouchbaseService
    {
        public static CouchbaseService Instance { get; } = new CouchbaseService();

        public Database Db { get; private set; } = null!;
        private Collection _profileCollection = null!;
        private INavigation? _navigation;

        public void Initialize(Database sharedDb, INavigation? navigation = null)
        {
            _navigation = navigation;
            Db = sharedDb;
            EnsureCollectionExists("profiles", "employees");
            _profileCollection = Db?.GetCollection("profiles", "employees");
        }

        private async void ShowErrorToUser(string title, string message)
        {
            if (_navigation != null && Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        }

        private void EnsureCollectionExists(string collectionName, string scopeName)
        {
            try
            {
                Db.CreateCollection(collectionName, scopeName);
            }
            catch (CouchbaseLiteException ex)
            {
                // Collection already exists or creation failed
                Console.WriteLine($"Collection creation: {ex.Message}");
            }
        }

        public List<Profile> GetAllProfiles()
        {
            var profiles = new List<Profile>();

            if (_profileCollection == null)
            {
                return profiles;
            }

            try
            {
                var query = QueryBuilder
                    .Select(
                        SelectResult.Property("id").As("id"),
                        SelectResult.Property("name"),
                        SelectResult.Property("title"),
                        SelectResult.Property("email"))
                    .From(DataSource.Collection(_profileCollection))
                    .OrderBy(Ordering.Property("id").Ascending());

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Query error: {ex.Message}");
            }

            return profiles;
        }

        public void DeleteProfile(string id)
        {
            if (_profileCollection == null) return;
            
            var doc = _profileCollection.GetDocument(id);
            if (doc != null)
            {
                _profileCollection.Delete(doc);
            }
        }

        public void SaveProfile(Profile profile)
        {
            if (string.IsNullOrEmpty(profile.Id)) return;

            var doc = new MutableDocument(profile.Id);
            
            doc.SetString("email", profile.Email);
            doc.SetString("id", profile.Id);
            doc.SetString("name", profile.Name);
            doc.SetString("title", profile.Title);
            
            _profileCollection.Save(doc);
        }
    }
}