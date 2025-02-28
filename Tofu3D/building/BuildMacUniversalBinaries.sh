#!/bin/bash


BUILDS_DIR=$1
cd $BUILDS_DIR
VERSION=$2
PROJECT_NAME="Tofu3D"
DOTNETBUILDDIR="../../../bin/MacOS Debug/net9.0"
FINAL_OUTPUT_DIR="tofu3d_${VERSION}_macos_universal"

# libraries to exclude from lipo-since they are already x64 and arm64
EXCLUDE_FROM_LIPO=("raarararaoaoa")

is_excluded() {
    local lib_name=$1
    for excluded in "${EXCLUDE_FROM_LIPO[@]}"; do
        if [[ "$excluded" == "$lib_name" ]]; then
            return 0 # Match found: excluded
        fi
    done
    return 1 # Not excluded
}

build_for_architecture() {
    local arch=$1
    echo "Building for $arch..."
    dotnet publish "../../../Tofu3D.csproj" -r osx-$arch -c "MacOS Debug" --self-contained
#     --verbosity m
}

# Build for both architectures
build_for_architecture "x64"
build_for_architecture "arm64"

# Create the final output directory
mkdir -p "$FINAL_OUTPUT_DIR"
mkdir -p "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/MacOS"
mkdir -p "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/Frameworks"

# Create universal binary
echo "Creating universal binary..."
lipo -create \
    "$DOTNETBUILDDIR/osx-x64/publish/$PROJECT_NAME" \
    "$DOTNETBUILDDIR/osx-arm64/publish/$PROJECT_NAME" \
    -output "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/MacOS/$PROJECT_NAME"

echo "Universal binary created successfully"

# Handle .dylib files
echo "Creating universal .dylib libraries..."
cp "$DOTNETBUILDDIR/osx-arm64/publish/runtimes/osx-arm64/native/cimgui.dylib" "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/Frameworks/cimgui.dylib"
    
for dylib in "$DOTNETBUILDDIR/osx-arm64/publish/"*.dylib; do
    base_name=$(basename "$dylib") # Extract the file name (e.g., libSkiaSharp.dylib)

    # Check if the library is excluded from lipo
    if is_excluded "$base_name"; then
        echo "Skipping lipo for $base_name (copying directly)..."
        cp "$DOTNETBUILDDIR/osx-x64/publish/$base_name" "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/Frameworks/$base_name"
    else
        echo "Creating universal $base_name..."
        lipo -create \
            "$DOTNETBUILDDIR/osx-x64/publish/$base_name" \
            "$DOTNETBUILDDIR/osx-arm64/publish/$base_name" \
            -output "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/Frameworks/$base_name"
    fi
done

# Ensure .dylib files are referenced correctly during runtime
echo "Setting rpath for the universal binary..."
install_name_tool -add_rpath "@executable_path/../Frameworks" "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/MacOS/$PROJECT_NAME"

# Ensure Info.plist and other resources are included (optional, based on app)
echo "Packaging app resources..."
mkdir -p "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/Resources"
cp ../../Info.plist "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/"
cp ../../tofu_icon.icns "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/Resources/"
cp -R ../../../EditorResources "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/Resources/"

cp "$DOTNETBUILDDIR/osx-arm64/Tofu3D.dll" "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/Resources/EditorResources"
# Sign the app (optional for local testing)
echo "Signing the app bundle..."
codesign --force --deep --sign - "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app"

#cd "$FINAL_OUTPUT_DIR"
#zip -r "../tofu3d_${VERSION}_macos_universal.zip" "$PROJECT_NAME.app"
#cd -

echo "Final universal app bundle created at: $FINAL_OUTPUT_DIR/$PROJECT_NAME.app"

#open "$FINAL_OUTPUT_DIR"

open "$FINAL_OUTPUT_DIR/$PROJECT_NAME.app/Contents/MacOS/Tofu3D"
