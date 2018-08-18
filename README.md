# App-Store-Uploader
A tool to upload screenshots and app previews using Apple's Transporter CLI.

Instructions are a work-in-progress!

## Requirements

* macOS.
* The latest version of XCode with command line tools installed.
* An (editable) app with one screenshow and one app preview in at least one locale.
  
## Usage

1. Clone this repository to your own computer.
2. Duplicate `1.Settings-template.config` and rename the duplicate to `1.Settings.config`.
3. Edit `1.Settings.config` and replace all variables there to match your own details.

   - `itmst_location` refers to the location of the Transporter command line interface. It's already prefilled with its default location, but if you downloaded it separately, edit it there.
4. Run `2.SetupDirectories.command`.
5. Add all your assets to the newly created /Assets/ folder, placing all the right sizes and locales in the right folders.
