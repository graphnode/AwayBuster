# AwayBuster

AwayBuster is a simple Windows utility designed to prevent your system from going idle. It runs as a single executable and lives quietly in your system tray.

- **Single Portable Executable:** Easy deployment with one file and operates solely from the system tray.
- **Status Indication:** The tray icon tints red to indicate whether the utility is enabled or disabled.
- **Windows Only:** Works only on windows, uses winforms and requires .NET 8.

## How to Build

This is a simple C# project that requires .NET 8.

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/awaybuster.git
   cd awaybuster
   ```

2. Restore dependencies and build the project:
   ```bash
   dotnet build -c Release
   ```
   
3. Publish as a single executable:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
   ```
   
## License

This project is licensed under the MIT License.
