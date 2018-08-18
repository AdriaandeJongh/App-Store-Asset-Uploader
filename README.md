# App-Store-Uploader
A tool to upload screenshots and app previews using Apple's Transporter CLI.

Instructions are a work-in-progress!

## Requirements

* macOS.
* The latest version of XCode with command line tools installed.
* An (editable) app with one screenshow and one app preview in at least one locale.
  
## Usage

1. Clone / download this repository.
2. Duplicate `1.Settings-template.config` and rename the duplicate to `1.Settings.config`.
3. Edit `1.Settings.config` and replace all variables there to match your own details.

   - `itmst_location` refers to the location of the Transporter command line interface. It's already prefilled with its default location, but if you downloaded it separately, edit it there.
4. Run `2.SetupDirectories.command`.
5. Add all your assets to the newly created /Assets/ folder, placing all screenshots and app previews with the right sizes in the right folders in each locale.

   - See the size reference below for what screenshot and app preview sizes belong in which folders (aka 'display targets').
   - Name screenshots in the format of `<position_in_app_store>.png`, eg. `1.png`, `2.png`, etc.
   - Name app previews in the format of `AppPreview-<position_in_app_store>.mp4`, eg. `AppPreview-1.mp4`, `AppPreview-2.mp4`, etc.
   - The position number of the screenshots is unrelated to the position numbers of the app previews: start both with 1.
   - The locale you set as the default locale in `1.Settings.config` should have all screenshots and app previews you want to show, as for every locale any missing screenshots or app previews are taken from there.
   - TODO: example images here
6. Run `3.ApplyMetadata.command`.
7. Run `4.VerifyMetadata.command`.
8. Run `5.Upload.command`. Your assets will shortly be processed by the App Store! :)
9. Run `6.GetStatus.command` to get the status of your latest upload(s). 

## Size reference per display target

All sizes below are in landscape, but can also be submitted as portrait.

Display Target | Screenshots | App Previews
--- | --- | ---
iOS-5.5-in | `2208 x 1242` | `1920 × 1080`
iOS-5.8-in | `2436 x 1125` | `1920 × 886`.
iOS-iPad-Pro | `2732 × 2048` | `1600 × 1200`.
appletvos | `1920 × 1080` | `1920 × 1080`.
Mac | `1440 × 900` | -

