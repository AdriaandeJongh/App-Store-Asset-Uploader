#!/usr/bin/env bash

# get this directory
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

# load settings
. "$dir/1.Settings.config"

# make sure we have the latest version of the metadata for this app id
"$itmst_location" -m lookupMetadata -u "$username" -p "$password" -apple_id $app_id -destination "$dir/Assets/" -v informational -app_platform $app_platform

# run the dotnet app to modify the metadata and copy the assets into the package
"$dir/ITMSPLUS/ITMSPLUS" --mode modify_metadata --itmsp_file_path "$dir/Assets/$app_id.itmsp" --default_asset_locale "$default_locale" --targeted_version "$targeted_version"