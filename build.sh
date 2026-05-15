#!/bin/bash
if ! command -v patchelf &> /dev/null; then
    echo "patchelf is not installed. Please install it using your package manager."
    exit 1
fi

BINARY_NAME="gitrm"
INSTALL_PATH="$HOME/.local/bin"

if [ ! -d ".venv" ]; then
    echo "Creating virtual environment..."
    python3 -m venv .venv
fi
source .venv/bin/activate

echo "Installing requirements..."

pip install --upgrade pip
pip install -r requirements.txt

BUILD_VERSION=$(date +'%Y.%m.%d-%H%M')
echo "Compiling gitrm version $BUILD_VERSION..."

VERSION=$BUILD_VERSION python3 -m nuitka --onefile --standalone --remove-output \
    --show-progress --show-scons --output-filename=$BINARY_NAME src/main.py

if [ -f "$BINARY_NAME.bin" ] || [ -f "$BINARY_NAME" ]; then
    echo "Moving binary to $INSTALL_PATH..."
    
    mkdir -p "$INSTALL_PATH"
    
    [ -f "$BINARY_NAME.bin" ] && mv "$BINARY_NAME.bin" "$INSTALL_PATH/$BINARY_NAME"
    [ -f "$BINARY_NAME" ] && mv "$BINARY_NAME" "$INSTALL_PATH/$BINARY_NAME"
    
    chmod +x "$INSTALL_PATH/$BINARY_NAME"
else
    echo "Error: Compilation failed!"
    exit 1
fi

echo "Done! Make sure $INSTALL_PATH is in your PATH."