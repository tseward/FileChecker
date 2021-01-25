#!/bin/zsh
dotnet clean --configuration Release
dotnet publish -r osx-x64 --self-contained false -c Release -f net5.0 -p:PublishSingleFile=true
dotnet publish -r linux-x64 --self-contained false -c Release -f net5.0 -p:PublishSingleFile=true
dotnet publish -r win-x64 --self-contained false -c Release -f net5.0 -p:PublishSingleFile=true
