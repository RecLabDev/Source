#!/bin/bash

SCRIPT_DIR="$(dirname $(realpath $0))"
cd $SCRIPT_DIR

CRATE_DIR="$(realpath '../packages/Aby')"

UNITY_PROJECT_DIR="$(realpath '../runtimes/Unity')"
UNITY_PLUGIN_DIR="$UNITY_PROJECT_DIR/Assets/Plugins/Aby"

BUILD_TARGET="${1:-release}"

# TODO: Build this per platform (as above).
# For Windows: SOURCE_LIB_NAME="jsruntime.dll"
# For Linux: SOURCE_LIB_NAME="libjsruntime.so"
# For macOS: SOURCE_LIB_NAME="libjsruntime.dylib"
# SOURCE_LIB_NAME="aby.dll"
SOURCE_LIB_PATH="$CRATE_DIR/target/$BUILD_TARGET/aby.dll"
TARGET_LIB_PATH="$UNITY_PLUGIN_DIR/AbyRuntime.dll"

SOURCE_GEN_PATH="$CRATE_DIR/gen/Unity/AbyRuntime.g.cs"
TARGET_GEN_PATH="$UNITY_PLUGIN_DIR/AbyRuntime.g.cs"

SOURCE_PDB_PATH="$CRATE_DIR/target/$BUILD_TARGET/aby.pdb"
TARGET_PDB_PATH="$UNITY_PLUGIN_DIR/AbyRuntime.pdb"

#--
echo "Building Rust crate ($BUILD_TARGET; $CRATE_DIR)"
cd "$CRATE_DIR"
cargo build --no-default-features --features ffi,unity
if [ $? -ne 0 ];
then
    echo "Cargo Build failed, exiting script."
    exit 1
fi

if [ -f "$SOURCE_LIB_PATH" ];
then
    mkdir -p "$UNITY_PLUGIN_DIR"
    cp "$SOURCE_LIB_PATH" "$TARGET_LIB_PATH"
    cp "$SOURCE_GEN_PATH" "$TARGET_GEN_PATH"
    echo "Library:"
    echo " -> Source: $SOURCE_LIB_PATH"
    echo " -> Target: $TARGET_LIB_PATH"
    echo "Source (Gen):"
    echo " -> Source: $SOURCE_GEN_PATH"
    echo " -> Target: $TARGET_GEN_PATH"
else
    echo "Library not found, check the build configuration and path."
    echo "Expected path: $SOURCE_LIB_PATH"
    exit 1
fi

#--
if [ "$BUILD_TARGET" == "debug" ] && [ -f "$SOURCE_PDB_PATH" ];\
then
    cp "$SOURCE_PDB_PATH" "$TARGET_PDB_PATH"
    echo "Program Database (PDB):"
    echo " -> Source: $SOURCE_PDB_PATH"
    echo " -> Target: $TARGET_PDB_PATH"
fi

#--
echo "All good! <3"
