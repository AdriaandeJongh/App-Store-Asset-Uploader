#!/usr/bin/env bash

# get this directory
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

# load settings
. $dir/1.Settings.config

"$itmst_location" -m statusAll -u "$username" -p "$password" -apple_id $app_id -v informational