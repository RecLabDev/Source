#!/bin/bash

SCRIPT_DIR="$(dirname $(realpath $0))"
cd $SCRIPT_DIR

CRATE_DIR="$(realpath '../packages/JsRuntime')"

UNITY_PROJECT_DIR="$(realpath '../runtimes/Unity')"
UNITY_PLUGIN_DIR="$UNITY_PROJECT_DIR/Assets/Plugins/JsRuntime"

BUILD_TARGET="${1:-release}"

# TODO: Adjust the library name according to your naming convention and target platform.
# For Windows: SOURCE_LIB_NAME="jsruntime.dll"
# For Linux: SOURCE_LIB_NAME="libjsruntime.so"
# For macOS: SOURCE_LIB_NAME="libjsruntime.dylib"
SOURCE_LIB_NAME="js_runtime.dll"
SOURCE_LIB_PATH="$CRATE_DIR/target/$BUILD_TARGET/$SOURCE_LIB_NAME"

SOURCE_PDB_NAME="js_runtime.pdb"
SOURCE_PDB_PATH="$CRATE_DIR/target/$BUILD_TARGET/$SOURCE_PDB_NAME"

# TODO: Build this per platform (as above).
TARGET_LIB_NAME="JsRuntime.dll"
TARGET_LIB_PATH="$UNITY_PLUGIN_DIR/$TARGET_LIB_NAME"

TARGET_PDB_NAME="JsRuntime.pdb"
TARGET_PDB_PATH="$UNITY_PLUGIN_DIR/$TARGET_PDB_NAME"

#--
echo "Building Rust crate ($BUILD_TARGET; $CRATE_DIR)"
cd "$CRATE_DIR"
cargo build --no-default-features --features ffi # TODO: In emergencies, break glass: --features lite
if [ $? -ne 0 ];
then
    echo "Cargo Build failed, exiting script."
    exit 1
fi

if [ -f "$SOURCE_LIB_PATH" ];
then
    mkdir -p "$UNITY_PLUGIN_DIR"
    cp "$SOURCE_LIB_PATH" "$TARGET_LIB_PATH"
    echo "Library moved successfully to Unity Plugins folder."
    echo " -> Source Lib: $SOURCE_LIB_PATH"
    echo " -> Target Lib: $UNITY_PLUGIN_DIR/$TARGET_LIB_NAME"
else
    echo "Library not found, check the build configuration and path."
    echo "Expected path: $SOURCE_LIB_PATH"
    exit 1
fi

#--
if [ "$BUILD_TARGET" == "debug" ] && [ -f "$SOURCE_PDB_PATH" ];\
then
    cp "$SOURCE_PDB_PATH" "$TARGET_PDB_PATH"
    echo "PDB file moved successfully to Unity Plugins folder."
    echo " -> Source PDB: $SOURCE_PDB_PATH"
    echo " -> Target PDB: $TARGET_PDB_PATH"
fi

#--
echo "All good! <3"
