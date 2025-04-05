#!/bin/bash

VERSION="0.0.1"

BUILDS_DIR="./builds/$VERSION"

echo "make sure to run as sudo, and chmod +x BuildAllPlatforms.sh, and brew install zip"
rm -rf $BUILDS_DIR
mkdir -p $BUILDS_DIR

cd $BUILDS_DIR

build_for_platform() {
    local platform=$1
    local platformFolderName=$2
    local configName=$3
    
    local finalFolderName="tofu3d_${VERSION}_${platformFolderName}"

    echo "Building for $platform..."
    
    rm -rf $finalFolderName
    mkdir -p $finalFolderName
    
    
    dotnet publish "../../../Tofu3D.csproj" -r "$platform" -c "$configName" --self-contained --output "$finalFolderName" --verbosity m
# -p:PublishSingleFile=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true

    echo "Creating zip archive for $platform..."
    
    local archiveName="${finalFolderName}.zip"
    zip -r $archiveName $finalFolderName
}

echo "Building "
build_for_platform "win-x64" "win_x64" "Windows Release"
#build_for_platform "win-arm64" "win_arm64" "Windows Release"
#build_for_platform "linux-x64" "linux_x64" "Linux Release"
#build_for_platform "linux-arm64" "linux_arm64" "Linux Release"
#
cd ../..
./BuildMacUniversalBinaries.sh "$BUILDS_DIR" "$VERSION"

echo "Builds completed successfully."
open $BUILDS_DIR
