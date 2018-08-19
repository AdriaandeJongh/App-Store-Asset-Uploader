using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;

namespace ITMSPLUS
{
    class Program
    {
        private static string assetsPath = string.Empty;
        private static string itmspFilePath = string.Empty;

        private static string defaultAssetsLocale = string.Empty;
        private static string targetedVersion = string.Empty;

        private static XmlDocument metaDataDocument = null;
        private static XmlElement locales = null;

        private static List<string> displayTargets = new List<string>();

        static void Main(string[] args)
        {
            string mode = string.Empty;

            for (int i = 0; i < args.Length; i=i+2)
            {
                switch (args[i])
                {
                    case "--mode":
                        mode = args[i + 1];
                        break;
                    case "--itmsp_file_path":
                        itmspFilePath = args[i + 1];
                        break;
                    case "--default_asset_locale":
                        defaultAssetsLocale = args[i + 1];
                        break;
                    case "--targeted_version":
                        targetedVersion = args[i + 1];
                        break;
                }
            }

            //make sure all parameters are set
            if(mode == string.Empty || itmspFilePath == string.Empty || defaultAssetsLocale == string.Empty || targetedVersion == string.Empty)
            {
                Console.WriteLine("ITMSPLUS needs to run with all commands:");
                Console.WriteLine("--mode [create_directories | modify_metadata]");
                Console.WriteLine("--itmsp_file_path \"<path>\"");
                Console.WriteLine("--default_asset_locale \"<locale>\"");
                Console.WriteLine("--targeted_version \"<version>\"");
                return;
            }

            //put all assets in the same directory as the itmsp file
            assetsPath = Directory.GetParent(itmspFilePath) + "/";

            //do all the setup
            ReadMetadata();
            DetermineLocales();
            DetermineDisplayTargets();
            RemoveRedundantMetadata();
            ClearITMSPPackage();

            switch(mode)
            {
                case "create_directories":

                    Console.WriteLine("Running ITMSPLUS mode: " + mode);
                    CreateDirectories();

                    break;
                case "modify_metadata":

                    Console.WriteLine("Running ITMSPLUS mode: " + mode);
                    ModifyMetadata();

                    break;
                default:
                    Console.WriteLine("Mode for ITMSPLUS should be set to create_directories or modify_metadata but was: " + mode);
                    break;
            }

            Console.WriteLine("Finished running ITMSPLUS.");

        }

        static void ReadMetadata()
        {
            //just read the XML

            string metaDataPath = itmspFilePath + "/metadata.xml";
            metaDataDocument = new XmlDocument();
            metaDataDocument.Load(metaDataPath);
        }

        static void DetermineLocales()
        {
            //select the locales within the right version and remove the old version(s)

            XmlElement versions = metaDataDocument.DocumentElement["software"]["software_metadata"]["versions"];

            List<int> childrenToRemove = new List<int>();

            for (int i = 0; i < versions.ChildNodes.Count; i++)
            {
                if (versions.ChildNodes.Item(i).Attributes["string"].Value == targetedVersion)
                {
                    locales = versions.ChildNodes.Item(i)["locales"];
                }
                else
                {
                    childrenToRemove.Add(i);
                }
            }

            foreach (var item in childrenToRemove)
            {
                Console.WriteLine("Removing version " + versions.ChildNodes.Item(item).Attributes["string"].Value + " from metadata...");
                versions.RemoveChild(versions.ChildNodes.Item(item));
            }
        }

        static void DetermineDisplayTargets()
        {
            //determine which display targets there are (to use for naming files and directories)

            for (int i = 0; i < locales.ChildNodes.Count; i++)
            {
                XmlElement locale = locales.ChildNodes.Item(i) as XmlElement;

                XmlElement appPreviews = locale["app_previews"];

                if (appPreviews != null)
                {
                    for (int y = 0; y < appPreviews.ChildNodes.Count; y++)
                    {
                        string displayTarget = appPreviews.ChildNodes.Item(y).Attributes["display_target"].Value;

                        if (!displayTargets.Contains(displayTarget))
                            displayTargets.Add(displayTarget);
                    }
                }

                XmlElement softwareScreenshots = locale["software_screenshots"];

                if (softwareScreenshots != null)
                {
                    for (int y = 0; y < softwareScreenshots.ChildNodes.Count; y++)
                    {
                        string displayTarget = softwareScreenshots.ChildNodes.Item(y).Attributes["display_target"].Value;

                        if (!displayTargets.Contains(displayTarget))
                            displayTargets.Add(displayTarget);
                    }
                }

            }
        }

        static void RemoveRedundantMetadata()
        {
            //remove metadata for game center and in-app purchases as that will 
            // drastically increase processing time (or so I've read...)
            //
            //note: this doesn't actually remove the game center achievements or 
            // in-app purchases from the store. it simply means we won't 
            // be updating them using this tool.

            XmlElement game_center = metaDataDocument.DocumentElement["software"]["software_metadata"]["game_center"];
            if (game_center != null)
                metaDataDocument.DocumentElement["software"]["software_metadata"].RemoveChild(game_center);
            
            XmlElement in_app_purchases = metaDataDocument.DocumentElement["software"]["software_metadata"]["in_app_purchases"];
            if (in_app_purchases != null)
                metaDataDocument.DocumentElement["software"]["software_metadata"].RemoveChild(in_app_purchases);
        }

        static void ClearITMSPPackage()
        {
            // remove any previously copied assets

            foreach (string file in Directory.GetFiles(itmspFilePath + "/", "*.png").Where(item => item.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase)))
                File.Delete(file);

            foreach (string file in Directory.GetFiles(itmspFilePath + "/", "*.mp4").Where(item => item.EndsWith(".mp4", StringComparison.CurrentCultureIgnoreCase)))
                File.Delete(file);
        }

        static void CreateDirectories()
        {
            //create folders for different sizes

            for (int dt = 0; dt < displayTargets.Count; dt++)
            {
                string displayTargetDirectory = assetsPath + displayTargets[dt] + "/";

                if (!Directory.Exists(displayTargetDirectory))
                {
                    Console.WriteLine("Creating new asset folder for displayTarget: " + displayTargets[dt]);
                    Directory.CreateDirectory(displayTargetDirectory);
                }

                //create folders for each locale
                for (int i = 0; i < locales.ChildNodes.Count; i++)
                {
                    XmlElement locale = locales.ChildNodes.Item(i) as XmlElement;
                    string countryCode = locale.Attributes["name"].Value;
                    string localePath = displayTargetDirectory + countryCode;

                    if (!Directory.Exists(localePath))
                    {
                        Console.WriteLine("Creating new asset folder for " + countryCode);
                        Directory.CreateDirectory(localePath);
                    }
                }

                string defaultAppPreviewSettingsPath = displayTargetDirectory + defaultAssetsLocale + "/AppPreview-settings.xml";

                if(!File.Exists(defaultAppPreviewSettingsPath))
                {
                    Console.WriteLine("Creating new AppPreview-settings.xml for default locale of " + displayTargets[dt]);
                    new System.Xml.Linq.XDocument(
                        new System.Xml.Linq.XElement("apppreview_settings",
                                                     new System.Xml.Linq.XElement("preview_image_time1", "00:00:00:00"),
                                                     new System.Xml.Linq.XElement("preview_image_time2", "00:00:00:00"),
                                                     new System.Xml.Linq.XElement("preview_image_time3", "00:00:00:00")
                        )
                    )
                    .Save(defaultAppPreviewSettingsPath);
                }
            }
        }

        public class AssetData
        {
            public string fileExtension = string.Empty;
            public string locale = string.Empty;
            public string size = string.Empty;
            public string file_name = string.Empty;
            public string checksum = string.Empty;
            public string display_target = string.Empty;
            public string position = string.Empty;
            public string preview_image_time = string.Empty;
        }

        static void ModifyMetadata()
        {
            //scan which files there are in the created directories, see which
            // assets are meant for this metadata file, and modify the metadata
            // pointing at those files

            string[] allFiles = GetFilesInDirectory(assetsPath);
            List<AssetData> allAssets = new List<AssetData>();

            //loop through all files and create AssetDatas for all files we'll need
            foreach (var file in allFiles)
            {
                if (file.Contains(itmspFilePath))
                    continue; //ignore all files inside the itsmp package
                
                string fileName = Path.GetFileName(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string fileExtension = Path.GetExtension(fileName);
                string fileLocale = Directory.GetParent(file).Name;
                string fileDisplayTarget = Directory.GetParent(Directory.GetParent(file).FullName).Name;

                if (!displayTargets.Contains(fileDisplayTarget))
                    continue; //ignore all files not meant for this app

                if (fileExtension != ".png" && fileExtension != ".mp4")
                    continue; //ignore files that aren't assets

                AssetData ad = new AssetData();
                ad.locale = fileLocale;
                ad.fileExtension = fileExtension;
                ad.file_name = fileDisplayTarget + "-" + fileLocale + "-" + fileName;
                ad.position = fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 1, 1);
                ad.display_target = fileDisplayTarget;
                ad.size = GetFileSize(file);
                ad.checksum = GetMD5(file);

                if (fileExtension == ".mp4")
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(Directory.GetParent(file).FullName + "/AppPreview-settings.xml");
                    ad.preview_image_time = doc.DocumentElement["preview_image_time" + ad.position].InnerText;
                }

                allAssets.Add(ad);


                File.Copy(file, itmspFilePath + "/" + ad.file_name, true);
            }

            //get the assets from the default locale
            List<AssetData> defaultAssets = new List<AssetData>();

            foreach (var ad in allAssets)
            {
                if (ad.locale == defaultAssetsLocale)
                    defaultAssets.Add(ad);
            }

            //now go through every locale and make sure every asset in the 
            // default locale is there OR the localised asset.
            for (int i = 0; i < locales.ChildNodes.Count; i++)
            {
                XmlElement locale = locales.ChildNodes.Item(i) as XmlElement;
                string countryCode = locale.Attributes["name"].Value;

                XmlElement appPreviews = locale["app_previews"];
                XmlElement screenshots = locale["software_screenshots"];

                if (appPreviews == null)
                {
                    appPreviews = metaDataDocument.CreateElement("app_previews", metaDataDocument.DocumentElement.NamespaceURI);
                    locale.AppendChild(appPreviews);
                }
                else
                {
                    appPreviews.RemoveAll();
                }


                if (screenshots == null)
                {
                    screenshots = metaDataDocument.CreateElement("software_screenshots", metaDataDocument.DocumentElement.NamespaceURI);
                    locale.AppendChild(screenshots);
                }
                else
                {
                    screenshots.RemoveAll();
                }

                foreach (AssetData defaultAsset in defaultAssets)
                {
                    AssetData assetData = null;

                    foreach (AssetData la in allAssets)
                    {
                        if (la.locale == countryCode &&
                            la.position == defaultAsset.position &&
                            la.display_target == defaultAsset.display_target &&
                            la.fileExtension == defaultAsset.fileExtension)
                        {
                            assetData = la;
                            break;
                        }
                    }

                    if (assetData == null)
                    {
                        assetData = defaultAsset;
                    }

                    if (assetData.fileExtension == ".mp4")
                    {
                        XmlElement appPreview = metaDataDocument.CreateElement("app_preview", metaDataDocument.DocumentElement.NamespaceURI);
                        appPreview.SetAttribute("display_target", assetData.display_target);
                        appPreview.SetAttribute("position", assetData.position);

                        XmlElement dataFile = metaDataDocument.CreateElement("data_file", metaDataDocument.DocumentElement.NamespaceURI);
                        dataFile.SetAttribute("role", "source");

                        XmlElement size = metaDataDocument.CreateElement("size", metaDataDocument.DocumentElement.NamespaceURI);
                        size.InnerText = assetData.size;
                        dataFile.AppendChild(size);

                        XmlElement fileName = metaDataDocument.CreateElement("file_name", metaDataDocument.DocumentElement.NamespaceURI);
                        fileName.InnerText = assetData.file_name;
                        dataFile.AppendChild(fileName);

                        XmlElement checksum = metaDataDocument.CreateElement("checksum", metaDataDocument.DocumentElement.NamespaceURI);
                        checksum.InnerText = assetData.checksum;
                        dataFile.AppendChild(checksum);

                        appPreview.AppendChild(dataFile);

                        XmlElement previewImageTime = metaDataDocument.CreateElement("preview_image_time", metaDataDocument.DocumentElement.NamespaceURI);
                        previewImageTime.SetAttribute("format", "30/1:1/nonDrop");
                        previewImageTime.InnerText = assetData.preview_image_time;

                        appPreview.AppendChild(previewImageTime);

                        appPreviews.AppendChild(appPreview);
                    }
                    else if(assetData.fileExtension == ".png")
                    {
                        XmlElement screenshot = metaDataDocument.CreateElement("software_screenshot", metaDataDocument.DocumentElement.NamespaceURI);
                        screenshot.SetAttribute("display_target", assetData.display_target);
                        screenshot.SetAttribute("position", assetData.position);

                        XmlElement size = metaDataDocument.CreateElement("size", metaDataDocument.DocumentElement.NamespaceURI);
                        size.InnerText = assetData.size;
                        screenshot.AppendChild(size);

                        XmlElement fileName = metaDataDocument.CreateElement("file_name", metaDataDocument.DocumentElement.NamespaceURI);
                        fileName.InnerText = assetData.file_name;
                        screenshot.AppendChild(fileName);

                        XmlElement checksum = metaDataDocument.CreateElement("checksum", metaDataDocument.DocumentElement.NamespaceURI);
                        checksum.SetAttribute("type", "md5");
                        checksum.InnerText = assetData.checksum;
                        screenshot.AppendChild(checksum);

                        screenshots.AppendChild(screenshot);
                    }
                }

                metaDataDocument.Save(itmspFilePath + "/metadata.xml");
            }
        }

        static string[] GetFilesInDirectory(string targetDirectory)
        {
            List<string> files = new List<string>();
            SearchFilesInDirectory(targetDirectory, files);
            return files.ToArray();
        }

        static void SearchFilesInDirectory(string targetDirectory, List<string> fileList)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                fileList.Add(fileName);
            
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                SearchFilesInDirectory(subdirectory, fileList);
        }

        static string GetMD5(string filepath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filepath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
                }
            }
        }

        static string GetFileSize(string filepath)
        {
            return new FileInfo(filepath).Length.ToString();
        }
    }
}
