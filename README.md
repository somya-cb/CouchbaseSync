# AppSync - .NET MAUI App

This project is a **.NET MAUI** mobile application that demonstrates **Couchbase Lite** synchronization with **Couchbase Capella**. It allows users to view and manage employee profiles with real-time bidirectional synchronization to the backend.

## ✅ Platform Support & Testing

**This app is fully functional and has been tested on:**

- **iOS**: iOS 18.4/18.5 (iPhone/iPad simulators)
- **Android**: Android API 35 (Pixel emulators)

## 🚀 Features

- **Full CRUD Operations**: Create, Read, Update, Delete employee profiles
- **Real-time Sync**: Bidirectional synchronization with Couchbase Capella
- **Cross-platform UI**: Clean, professional interface for both iOS and Android
- **Session Authentication**: Login required on each app launch
- **Employee Management**: View, edit, and delete employee records

## Directory Structure

The project follows the typical structure of a **.NET MAUI** application, with the following key directories:

- **`Models`**: Contains the data models used in the application, such as the `Profile` model representing employee information.
- **`Platforms`**: Contains platform-specific code for Android, iOS, etc.
- **`Properties`**: Contains application-level properties, such as settings and configurations.
- **`Resources`**: Contains image assets, fonts, and other resources for the application.
- **`Services`**: Contains services such as `CouchbaseService` for database operations and `SyncService` for synchronization.
- **`Views`**: Contains UI components and pages, such as `ProfilePage`, where employee profiles are displayed.

## Key Files

- **`App.xaml`**: The entry point for the application, where application-wide resources are defined.
- **`App.xaml.cs`**: Handles the initialization of shared services like the `CouchbaseService` and `SyncService`.
- **`AppSync.csproj`**: The project file that contains references and settings for the .NET MAUI application.
- **`AppSync.sln`**: The solution file that contains the project and solution structure.
- **`MauiProgram.cs`**: The file where services and dependencies are configured for the app.
- **`Dataset.json`**: This is the dataset you can import onto Capella.
  
## 🔧 Setup & Configuration

### Couchbase Capella Setup

1. **Create Cluster & Bucket:**
   - Set up a Couchbase Capella cluster
   - Create bucket: `users`
   - Create scope: `employees` 
   - Create collection: `profiles`

2. **App Services Configuration:**
   - Create an App Services endpoint (e.g., test-endpoint)
   - Create a user for the endpoint (e.g., test-user)
   - In Access & Validation, use wildcard `!` to allow all access
   - Update the sync gateway URL in `SyncService.cs` with your endpoint

3. **Network Access:**
   - Allow IP address access in Capella (blocks all by default)
   - For testing: Can allow access from everywhere
   - ⚠️ **Not recommended for production** - use specific IP ranges

4. **Import Sample Data :**
   - Use the included `Dataset.json` to populate your Capella bucket with sample employee data

### App Configuration

**Login Credentials:** Use the same credentials as your App Services user

## 🚀 Running the App

### Prerequisites

### .NET Workload Installation (Required)
Before building or running the project, make sure you have the necessary .NET workloads installed:

```bash
dotnet workload install maui
dotnet workload install android
dotnet workload install ios
dotnet workload update
```

#### VS Code Extensions (Optional - for development)
If you plan to use this app as a base for building similar applications, install these VS Code extensions:

- **.NET MAUI** - Tools for building .NET MAUI apps  
- **C# Dev Kit** - Official C# extension with IntelliSense and debugging
- **C#** - Base language support for C#

### Development Environment Requirements

- .NET 8 SDK
- iOS Simulator (for iOS testing)
- Android Emulator (for Android testing)

### Steps to Run

1. **Clone the repository:**
```bash
git clone https://github.com/somya-cb/CouchbaseSync.git
cd CouchbaseSync
```

2. **Clean and restore dependencies:**
```bash
dotnet clean
dotnet restore
```

3. **Run on iOS:**
```bash
dotnet build -f net8.0-ios -t:Run
```

4. **Run on Android:**
```bash
# First ensure Android emulator is running
emulator -avd YOUR_EMULATOR_NAME &
# Verify emulator is connected
adb devices
# Build and run
dotnet build -f net8.0-android -t:Run
```

## 🧪 Testing the App

1. **Login** with your App Services credentials
2. **View employee profiles** (synced from Capella)
3. **Test real-time bidirectional sync:**
   - Edit an employee profile on device → Check Capella for instant sync
   - Delete an employee on device → Verify removal in Capella
   - Edit/delete a record in Capella → See real-time updates in app
   - Use refresh button to manually sync if needed
4. **Test CRUD operations:**
   - Create, Read, Update, Delete employee profiles
   - Confirm deletion dialogs work properly
5. **Test session management:**
   - Logout and login again to verify authentication flow

## 🛠️ Troubleshooting

- **Sync fails:** Verify your App Services endpoint URL in `SyncService.cs`
- **Connection issues:** Ensure network access is configured in Capella
- **App won't start:** Check that emulator/simulator has internet connectivity
- **Build errors:** Run `dotnet clean` and `dotnet restore` before building

## 📱 Screenshots

### iOS
<img src="https://github.com/somya-cb/CouchbaseSync/blob/main/screenshots/ios%20-%20Login.png" alt="iOS Login Page" width="400" height="600"> &nbsp;&nbsp;&nbsp;&nbsp; <img src="https://github.com/somya-cb/CouchbaseSync/blob/main/screenshots/ios-%20Profiles.png" alt="iOS Profile Page" width="400" height="600">

### Android
<img src="https://github.com/somya-cb/CouchbaseSync/blob/main/screenshots/android%20-%20Login.png" alt="Android Login Page" width="250" height="500"> &nbsp;&nbsp;&nbsp;&nbsp; <img src="https://github.com/somya-cb/CouchbaseSync/blob/main/screenshots/android%20-%20Profiles.png" alt="Android Profile Page" width="250" height="500">
