#!/bin/bash

# Default values
VERSION=${VERSION:-"0.4.34"}
TARGET_ARCH=${TARGET_ARCH:-"x86_64"}
OS_TYPE=${OS_TYPE:-"pc-windows-msvc"}
INSTALL_DIR=${INSTALL_DIR:-"./.bin"}  # Default installation directory

# Create the installation directory if it doesn't exist
mkdir -p $INSTALL_DIR

# Base URL for GitHub releases
BASE_URL="https://github.com/rust-lang/mdBook/releases/download"

# Form the complete download URL
DOWNLOAD_URL="$BASE_URL/v$VERSION/mdbook-v$VERSION-$TARGET_ARCH-$OS_TYPE.zip"

# Download and unzip
wget $DOWNLOAD_URL
unzip "mdbook-v$VERSION-$TARGET_ARCH-$OS_TYPE.zip"
rm "mdbook-v$VERSION-$TARGET_ARCH-$OS_TYPE.zip"

# Move to the specified directory
mv mdbook "$INSTALL_DIR/"

echo "mdBook version $VERSION for $TARGET_ARCH has been downloaded and installed in $INSTALL_DIR."
