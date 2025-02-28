#!/bin/sh

# Publish for multiple architectures
dotnet publish -r win-x64 -c Release --self-contained true
dotnet publish -r win-arm64 -c Release --self-contained true

# Use native shell commands
wix build installer/WixInstaller.wxs -arch x64 -arch arm64