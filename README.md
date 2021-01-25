# FileChecker

An updated version of Bill Baer's [FileChecker](https://github.com/wbaer/FileChecker) application.

* .NET 5.0 Console application which should run on any platform
* Dropped WinForms for cross-platform compatibility
* Updated rules for 400 character _filename_ and 250GB file size maximums
* Added a rule when the path + filename is greater equal or greater than to 400 characters

## Usage

`FileChecker --path /srv/share [--legacy] [--append]`

* `--path` is the root folder where you want to start the scan
* `--legacy` adds additional checks for '#' and '%'. This is useful if the tenant admin has disallowed these characters via `Set-SPOTenant -SpecialCharactersStateInFileFolderNames`. See [Set-SPOTenant](https://docs.microsoft.com/powershell/module/sharepoint-online/set-spotenant).
* `--append` will not overwrite the CSV output but rather append to the existing CSV. It will create a new file if one does not exist.

## Requirements

* [.NET 5.0 Runtime](https://dotnet.microsoft.com/download/dotnet/5.0) (Desktop runtime is not required)

## Platform Support

* macOS (Intel)
* Windows 10 (x64)
* Will probably work on all .NET 5.0 supported platforms
