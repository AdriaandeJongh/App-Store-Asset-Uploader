# App-Store-Asset-Uploader
A tool to upload screenshots and app previews using Apple's Transporter CLI.

Instructions are a work-in-progress!

## Requirements

* macOS.
* Apple's Transporter app from the Mac App Store.

## Setup

1. Confirm that you have an editable version of your app (a version with status 'Prepare for Submission') on [App Store Connect](https://appstoreconnect.apple.com/) with every language you want to upload added to that version.
2. Confirm that you uploaded at least 1 app preview and 1 screenshot for every size (aka 'display target') to your default locale (eg. en-US) on App Store Connect. (I know the whole purpose of this tool is to avoid using the web interface to upload assets but this is the last time I swear! All jokes aside, having at least 1 type of asset uploaded for each display target will build the metadata structure we need to create directories later on...)
3. Clone / download this repository.
4. Duplicate `1.Settings-template.config` and rename the duplicate to `1.Settings.config`.
5. Edit `1.Settings.config` and replace all variables there to match your own details.

   - `password` must be an app-specific password generated on appleid.apple.com.
   - `targeted_version` should point at the version of your app on App Store Connect that you want to modify the app previews and screenshots for.
   - `itmst_location` refers to the location of the Transporter command line interface. It's already prefilled with its default location, but if you downloaded it separately, edit it there.
   - Because the iOS and tvOS app share the same appId but the macOS app does not, App-Store-Asset-Uploader can only download and upload one type of app (iOS, tvOS, macOS) at a time. The settings template includes examples for tvOS and macOS apps, being commented out at the bottom of the template. Be sure to only have one set of the variables `app_id` and `app_platform` enabled when running through the steps. If you need to upload assets to different platforms, you should repeat the Usage steps below (steps 1 through 5) for every platform by setting a different `app_id` and `app_platform`.

## Usage

1. Run `2.SetupDirectories.command`. This will create an `/Assets/` directory, download an `.itmsp` package to it (which contains a `metadata.xml`), and create directories for every locale for every display target.

   - Note: when switching between platforms (as explained in Setup), there is no need to remove any assets from the `/Assets/` directory as only relevant assets for each platform will be processed.
   
      <img width="616" alt="Screenshot of the created directories." src="https://user-images.githubusercontent.com/5611323/44308416-b4a63680-a3b5-11e8-9572-94503bb8708d.png">
2. Add all your assets to the newly created `/Assets/` directory, placing all screenshots and app previews with the right sizes in the right folders in each locale.
      
      <img width="820" alt="Example of a fully filled-in locale." src="https://user-images.githubusercontent.com/5611323/44308364-a3a8f580-a3b4-11e8-9dc8-6dee42f359ce.png">

   - See [the size reference below](#size-reference-per-display-target) for what screenshot and app preview sizes belong in which folders.
   - Name screenshots in the format of `<position_in_app_store>.png`, eg. `1.png`, `2.png`, etc, and app previews in the format of `AppPreview-<position_in_app_store>.mp4`, eg. `AppPreview-1.mp4`, `AppPreview-2.mp4`, etc.
   - The position number of the screenshots is unrelated to the position numbers of the app previews: start both with 1.
   - The locale you set as the default locale in `1.Settings.config` should have all screenshots and app previews in all display targets. For other locales, you can skip adding a screenshot or app preview when it's the same as the asset of the default locale. If you skip an asset in a not-default locale, the asset of the default locale will be put in the metadata for that non-default locale. As an example: if the default locale set in your `1.Settings.config` file is en-US (English) and only the screenshots `1.png` and `5.png` need to be localised to de-DE (German), the English locale folder will have all screenshots and app previews while the German locale folder will only have the screenshots `1.png` and `5.png`.
   
      <img width="820" alt="Example of a locale relying on assets from the default locale." src="https://user-images.githubusercontent.com/5611323/44308378-f1bdf900-a3b4-11e8-95d9-65d54192bbff.png">

   - Every locale with app previews should have an `AppPreview-settings.xml`. A template is automatically created in the default locale. Edit each `preview_image_time#` in `AppPreview-settings.xml` to indicate which frame should be the poster frame of the app preview. Funnily enough, the format of this time indication for this at-most-30-second-video is `hours:minutes:seconds:frames`.
3. Run `3.ApplyMetadata.command`. This will scan the assets directory for all .png's and .mp4's, copy the relevant files to the .itmsp file, and update the metadata.xml to point at the files inside the .itmsp package.
4. Run `4.VerifyMetadata.command`. This will connect with the App Store and verify that the metadata.xml was filled in correctly and all referenced assets are uploadable. Watch the output of the Terminal window to see if the verification succeeded or failed, and resolve any errors if needed.
5. Run `5.Upload.command`. Your assets will be uploaded and processed by the App Store, but may take up to 24 hours (!!) to be fully processed and show up in App Store Connect. In the meantime, you may run `6.GetStatus.command` to get the status of your latest upload(s).

## Size reference per display target

Official support documents [here for screenshots](https://help.apple.com/app-store-connect/#/devd274dd925), and [here for App Previews](https://help.apple.com/app-store-connect/#/dev4e413fcb8). For quick reference, most common sizes below.

Display Target | Screenshots | App Previews
--- | --- | ---
iOS-5.5-in | `2208 x 1242` | `1920 × 1080`
iOS-6.5-in | `2688 x 1242` | `1920 × 886`
iOS-iPad-Pro | `2732 × 2048` | `1600 × 1200`
iOS-iPad-Pro-2018 | `2732 × 2048` | `1600 × 1200`
appletvos | `1920 × 1080` | `1920 × 1080`
Mac | `1440 × 900` | `1920 x 1080`
