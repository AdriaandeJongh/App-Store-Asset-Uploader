#!/usr/bin/env bash

# get this directory
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

# load settings
. "$dir/1.Settings.config"

"$itmst_location" -m verify -u "$username" -p "$password" -apple_id $app_id -f "$dir/Assets/$app_id.itmsp" -v informational