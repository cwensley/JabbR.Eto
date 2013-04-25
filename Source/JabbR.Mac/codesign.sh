#!/bin/bash

OUTPUT_PATH=$1
SIGN_IDENTITY=$2
INSTALLER_SIGN_IDENTITY=$3
PROVISIONING_PROFILE=3C5D2A55-19B4-4D36-9A7C-243AE09BC2F5
APP_PATH=$OUTPUT_PATH/JabbReto.app
INSTALLER_NAME=$OUTPUT_PATH/JabbR.Eto.Mac-1.0.pkg
ENTITLEMENTS=$OUTPUT_PATH/../../Entitlements.plist
# ENTITLEMENTS=$OUTPUT_PATH/JabbReto.xcent
export PROVISIONING_PROFILE

# codesign -v --force --sign "$SIGN_IDENTITY" "$APP_PATH/Contents/MonoBundle/libMonoPosixHelper.dylib"
codesign -v --force --sign "$SIGN_IDENTITY" "--resource-rules=$APP_PATH/Contents/ResourceRules.plist" --entitlements "$ENTITLEMENTS" "$APP_PATH"
if [ "$INSTALLER_SIGN_IDENTITY" != "" ]
then
productbuild --component "$APP_PATH" /Applications --sign "$INSTALLER_SIGN_IDENTITY" "$INSTALLER_NAME"
fi
