#!/bin/sh
set -e
dotnet publish gitrm.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o dist -v d
mv -f dist/gitrm ~/.local/bin
