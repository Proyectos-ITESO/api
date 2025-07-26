#!/bin/bash

# MicroJack PRO - Version Update Script
# Updates version in both API and Updater projects

if [ -z "$1" ]; then
    echo "Usage: $0 <version>"
    echo "Example: $0 1.1.0"
    exit 1
fi

NEW_VERSION="$1"
NEW_ASSEMBLY_VERSION="${NEW_VERSION}.0"

echo "=== Updating MicroJack PRO to version $NEW_VERSION ==="

# Update API project
echo "Updating MicroJack.API.csproj..."
sed -i "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/g" MicroJack.API.csproj
sed -i "s/<AssemblyVersion>.*<\/AssemblyVersion>/<AssemblyVersion>$NEW_ASSEMBLY_VERSION<\/AssemblyVersion>/g" MicroJack.API.csproj
sed -i "s/<FileVersion>.*<\/FileVersion>/<FileVersion>$NEW_ASSEMBLY_VERSION<\/FileVersion>/g" MicroJack.API.csproj
sed -i "s/<InformationalVersion>.*<\/InformationalVersion>/<InformationalVersion>$NEW_VERSION<\/InformationalVersion>/g" MicroJack.API.csproj

# Update Updater project
echo "Updating MicroJack.Updater.csproj..."
sed -i "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/g" ../MicroJack.Updater/MicroJack.Updater.csproj
sed -i "s/<AssemblyVersion>.*<\/AssemblyVersion>/<AssemblyVersion>$NEW_ASSEMBLY_VERSION<\/AssemblyVersion>/g" ../MicroJack.Updater/MicroJack.Updater.csproj
sed -i "s/<FileVersion>.*<\/FileVersion>/<FileVersion>$NEW_ASSEMBLY_VERSION<\/FileVersion>/g" ../MicroJack.Updater/MicroJack.Updater.csproj
sed -i "s/<InformationalVersion>.*<\/InformationalVersion>/<InformationalVersion>$NEW_VERSION<\/InformationalVersion>/g" ../MicroJack.Updater/MicroJack.Updater.csproj

# Update release script
echo "Updating release script..."
sed -i "s/VERSION=\".*\"/VERSION=\"$NEW_VERSION\"/g" create_first_release.sh

echo "✅ Version updated to $NEW_VERSION in both projects!"
echo
echo "🚀 Next steps:"
echo "   1. Run: ./create_first_release.sh"
echo "   2. Upload the generated ZIP to your server"
echo "   3. Update license server with new version and hash"
echo