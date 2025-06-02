#!/bin/bash
echo "If you have not already, run:"
echo "chmod +x $0"

echo "Terminal command:"
echo "./iconset2icns.sh ./yourapp.iconset"

ICONSET_PATH="$1"

# Remove trailing slash if present
ICONSET_PATH="${ICONSET_PATH%/}"

if [[ ! -d "$ICONSET_PATH" ]]; then
  echo "Error: '$ICONSET_PATH' is not a directory."
  exit 1
fi

if [[ "$ICONSET_PATH" != *.iconset ]]; then
  echo "Error: '$ICONSET_PATH' does not have a .iconset extension."
  exit 1
fi

# Get the script's directory (where the output will be placed)
SCRIPT_DIR="$(cd "$(dirname "$0")"; pwd)"

# Output filename (strip .iconset, add .icns) in the script directory
ICNS_BASENAME="$(basename "${ICONSET_PATH%.iconset}")"
ICNS_PATH="${SCRIPT_DIR}/${ICNS_BASENAME}.icns"

echo "Converting '$ICONSET_PATH' to '$ICNS_PATH'..."
iconutil -c icns "$ICONSET_PATH" -o "$ICNS_PATH"

if [ $? -eq 0 ]; then
  echo "Success! Created: $ICNS_PATH"
else
  echo "iconutil failed."
fi

read -n 1 -s -r -p "Press any key to exit..."