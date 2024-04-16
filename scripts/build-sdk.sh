#!/bin/bash

SCRIPT_DIR="$(dirname $(realpath $0))"
cd $SCRIPT_DIR

THETA_SDK_DIR="$(realpath '../sdk/DotNet')"

UNITY_PROJECT_DIR="$(realpath '../runtimes/Unity')"
UNITY_PLUGIN_DIR="$UNITY_PROJECT_DIR/Assets/Plugins/Aby"

BUILD_TARGET="${1:-Release}"

LIB_NAME="AbySDK.dll"

echo "Building SDK ($BUILD_TARGET; $THETA_SDK_DIR)"
cd "$THETA_SDK_DIR"
dotnet build -c $BUILD_TARGET

if [ $? -ne 0 ]; then
    echo "DotNet Build failed, exiting script."
    exit 1
fi

LIB_BUILD_PATH="$THETA_SDK_DIR/bin/$BUILD_TARGET/netstandard2.1/$LIB_NAME"
if [ -f "$LIB_BUILD_PATH" ]; then
    cp "$LIB_BUILD_PATH" "$UNITY_PLUGIN_DIR"
    echo "Library moved to Unity Plugins folder."
    echo "Source DLL: $LIB_BUILD_PATH"
    echo "Target DLL: $UNITY_PLUGIN_DIR/$LIB_NAME"
else
    echo "DLL not found, check the build configuration and path."
    echo "Expected path: $LIB_BUILD_PATH"
    exit 1
fi

# TODO: Run Unity integration tests ..
# if [[ " $* " =~ " --bootstrap " ]];
# then
#     UNITY_TEST_PATH="Theta.Unity.Editor.Aby.Tests.JsRuntime.CanMount"
#     echo "Running Unity SDK Setup Scripts ($UNITY_TEST_PATH)"
#     Unity -quit -batchmode -logfile -projectPath $UNITY_PROJECT_DIR -executeMethod $UNITY_TEST_PATH
#     EXIT_CODE=$?

#     if [ $EXIT_CODE -ne 0 ]; then
#         echo "Unity tests failed with code $EXIT_CODE .."
#         exit 1
#     fi
# fi

echo "All good! <3"