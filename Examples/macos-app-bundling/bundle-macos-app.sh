#!/bin/bash
set -e

APP_NAME="Examples"
#TARGET_RID="osx-arm64"
#FRAMEWORK="net8.0"
#CONFIG="Release"
ICNS_NAME="examples.icns"

# Require PUBLISH_ROOT as the first argument
if [ -z "$1" ]; then
  echo "Error: You must provide the publish root as the first argument."
  echo "Usage: $0 <publish-root-path>"
  exit 1
fi

PUBLISH_ROOT="$1"

BUNDLE_DIR="${PUBLISH_ROOT}/${APP_NAME}.app"
MACOS_DIR="${BUNDLE_DIR}/Contents/MacOS"
RESOURCES_DIR="${BUNDLE_DIR}/Contents/Resources"

echo "Using publish root: $PUBLISH_ROOT"
echo "Creating .app bundle structure in: $BUNDLE_DIR"
mkdir -p "$MACOS_DIR"
mkdir -p "$RESOURCES_DIR"

echo "Copying executable and libraries..."
cp "${PUBLISH_ROOT}/${APP_NAME}" "$MACOS_DIR/"
cp "${PUBLISH_ROOT}/"*.dll "$MACOS_DIR/" 2>/dev/null || true
cp "${PUBLISH_ROOT}/"*.dylib "$MACOS_DIR/" 2>/dev/null || true
cp "${PUBLISH_ROOT}/"*.json "$MACOS_DIR/" 2>/dev/null || true
cp "${PUBLISH_ROOT}/"*.pdb "$MACOS_DIR/" 2>/dev/null || true

#echo "Copying Resources Folder..."
#if [ -d "${PUBLISH_ROOT}/Resources" ]; then
#  cp -R "${PUBLISH_ROOT}/Resources"/* "$RESOURCES_DIR/" 2>/dev/null || true
#fi

if [ -d "${PUBLISH_ROOT}/Resources" ]; then
  echo "Copying Resource folder to bundle resources..."
  cp -R "${PUBLISH_ROOT}/Resources" "$RESOURCES_DIR/"
else
  echo "Resource folder not found, skipping copy."
fi

if [ -f "${PUBLISH_ROOT}/packedExampleResources.shaperes" ]; then
  echo "Copying packedExampleResources.shaperes to bundle resources..."
  cp "${PUBLISH_ROOT}/packedExampleResources.shaperes" "$RESOURCES_DIR/"
else
  echo "packedExampleResources.shaperes not found, skipping copy."
fi

# Copy the .icns file from the output directory to the Resources directory in the bundle
if [ -f "${PUBLISH_ROOT}/${ICNS_NAME}" ]; then
  echo "Copying ${ICNS_NAME} icon file from output directory..."
  cp "${PUBLISH_ROOT}/${ICNS_NAME}" "$RESOURCES_DIR/"
else
  echo "WARNING: ${ICNS_NAME} not found in output directory, skipping icon copy."
fi

echo "Copying Info.plist..."
cp "${PUBLISH_ROOT}/Info.plist" "${BUNDLE_DIR}/Contents/"

echo "Making executable..."
chmod +x "${MACOS_DIR}/${APP_NAME}"

echo ".app bundle created at: $BUNDLE_DIR"