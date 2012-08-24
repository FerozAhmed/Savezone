using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
using SaveSync.Models;

namespace SaveSync.Controllers
{
    class MainController
    {
        //Credentials
        private String _username;
        private String _password;

        // Creating the cloudstorage object
        readonly CloudStorage _storage = new CloudStorage();
        private static List<SavegameInfo> _savegames = new List<SavegameInfo>();

        private static BackgroundWorker _worker;

        public MainController()
        {
            TryConnect();
        }

        public static List<SavegameInfo> Savegames
        {
            get { return SavegameInfo.GetSavegames(); }
            set { _savegames = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public MainController(string username, string password)
        {
            _username = username;
            _password = password;

            TryConnect();
        }

        private void TryConnect()
        {
            if (!_storage.IsOpened)
            {
                switch (GlobalController.Provider)
                {
                    case GlobalController.ProviderEnum.DropBox:
                        ConnectDropBox();
                        break;
                    case GlobalController.ProviderEnum.BoxNet:
                        ConnectBoxNet(Username, Password);
                        break;
                }
            }
        }

        private void ConnectDropBox()
        {
            // get the configuration for dropbox
            var dropBoxConfig = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox) as DropBoxConfiguration;

            // declare an access token
            ICloudStorageAccessToken accessToken = null;
            // load a valid security token from file
            using (var fs = File.Open("auth/token.xml", FileMode.Open, FileAccess.Read, FileShare.None))
            {
                accessToken = _storage.DeserializeSecurityToken(fs);
            }

            // open the connection
            /*try
            {*/
                _storage.Open(dropBoxConfig, accessToken);
            //}
            /*catch (Exception)
            {
                throw new Exception("Could not connect to Dropbox.\nCheck if you username and password are correct.\nInfo: Dropbox authentication is stored in 'auth/token.xml'. Delete the file te re-authenticate.");
            }*/

            if (_storage.IsOpened)
            {
                // get a specific directory in the cloud storage, e.g. /Public
                var publicFolder = _storage.GetRoot();

                // enumerate all child (folder and files)
                if (publicFolder == null) throw new Exception("Could not get the root folder.");
                foreach (var fof in publicFolder)
                {
                    // check if we have a directory
                    Boolean bIsDirectory = fof is ICloudDirectoryEntry;
                    // output the info
                    Console.WriteLine("{0}: {1}", bIsDirectory ? "DIR" : "FIL", fof.Name);
                }
            }
        }

        private void ConnectBoxNet(String username, String password)
        {
            // get the configuration for dropbox
            var boxnetConfig = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.BoxNet);

            // Use GenericNetworkCredentials class for Box.Net.
            var cred = new GenericNetworkCredentials
                           {Password = password, UserName = username};

            // open the connection
            try
            {
                _storage.Open(boxnetConfig, cred);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not connect to Box.\nCheck if you username and password are correct.");
            }

            if (_storage.IsOpened)
            {
                //Try to create the app folder
                _storage.CreateFolder("Savegames");

                // get a specific directory in the cloud storage, e.g. /Public
                var publicFolder = _storage.GetRoot();

                if (publicFolder == null) throw new Exception("Could not get the root folder.");
                // enumerate all child (folder and files)
                foreach (var fof in publicFolder)
                {
                    // check if we have a directory
                    var bIsDirectory = fof is ICloudDirectoryEntry;
                    // output the info
                    Console.WriteLine(@"{0}: {1}", bIsDirectory ? "DIR" : "FIL", fof.Name);
                }
            }
        }

        public void Backup(List<SavegameInfo> savegames, BackgroundWorker worker)
        {
            _worker = worker;

            var gamesProcessed = 0;
            var numberOfGames = 0;

            foreach (SavegameInfo sg in savegames)
            {
                if (sg.LocationExists) numberOfGames++;
            }

            foreach (SavegameInfo sg in savegames)
            {
                var extraFolder = "Savezone/";

                worker.ReportProgress(Convert.ToInt32(((double)gamesProcessed / (double)numberOfGames) * 100), new ProgressInfo(ProgressTypeEnum.ProgressType.Games, gamesProcessed, numberOfGames, sg.Game.Name));

                var foldername = sg.SpecialPath + "/" + sg.Game.Id;

                if (sg.LocationExists)
                {
                    var backupPath = sg.Savegamelocation;

                    if (GlobalController.Provider == GlobalController.ProviderEnum.BoxNet)
                        _storage.CreateFolder("/" + extraFolder + sg.SpecialPath);
                    else
                        _storage.CreateFolder("/" + sg.SpecialPath);
                    //_dropBoxStorage.CreateFolder("/" + sg.SpecialPath + "/" + foldername);

                    var files = new List<string>(Directory.GetFiles(backupPath, "*.*", SearchOption.AllDirectories));
                    var currentFileIndex = 0;

                    //_dropBoxStorage.CreateFolder(foldername);

                    foreach (var file in files)
                    {
                        worker.ReportProgress(Convert.ToInt32((double)currentFileIndex / (double)files.Count * 100), new ProgressInfo(ProgressTypeEnum.ProgressType.Files, currentFileIndex, files.Count));

                        //Create virtual directory structure
                        string dirWithFile;
                        if (GlobalController.Provider == GlobalController.ProviderEnum.BoxNet)
                            dirWithFile = "/" + extraFolder + foldername + Path.GetFullPath(file).Replace(backupPath, "").Replace("\\","/");
                        else
                            dirWithFile = "/" + foldername + Path.GetFullPath(file).Replace(backupPath, "").Replace("\\","/");
                        
                        var dir = dirWithFile.Replace("/" + Path.GetFileName(dirWithFile), "");

                        //now we check to see if the files are modified
                        var lastWriteTime_local = File.GetLastWriteTime(file);
                        var lastWriteTime_remote = new DateTime();

                        //Try to get the last modified date from the remote server
                        try
                        {
                            lastWriteTime_remote = _storage.GetFile(dirWithFile, _storage.GetRoot()).Modified;
                        }
                        catch (Exception ex)
                        {
                            //throw ex;
                            Console.WriteLine(@"MainController>>> Exception (Trying to get the last write time): " + ex.Message);
                        }

                        if (lastWriteTime_remote <= lastWriteTime_local)
                        {
                            //Check existing folders and partially cache folder structure
                            var remoteFolders = new List<String>();
                            if (!remoteFolders.Distinct().Contains(dir))
                            {
                                try
                                {
                                    _storage.GetFolder(dir);
                                }
                                catch (Exception ex)
                                {
                                    //Folder not found, create and add to the cache
                                    _storage.CreateFolder(dir);
                                }
                                remoteFolders.Add(dir);
                            }

                            try
                            {
                                _storage.UploadFile(file, dir, UploadDownloadProgress);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            
                        }

                        //File.SetLastAccessTime(file, DateTime.Now);
                        currentFileIndex++;

                        worker.ReportProgress(Convert.ToInt32((double)currentFileIndex / (double)files.Count * 100), new ProgressInfo(ProgressTypeEnum.ProgressType.Files,currentFileIndex, files.Count));
                        worker.ReportProgress(0, new ProgressInfo(ProgressTypeEnum.ProgressType.Upload,0,100));
                    }

                    gamesProcessed++;
                    Console.WriteLine(@"MainController>>> " + sg.Game.Name + @" backed up. ("+ gamesProcessed + @" / " + numberOfGames + @")");

                    //Console.WriteLine(((double)gamesProcessed / (double)numberOfGames) *100);
                    worker.ReportProgress(Convert.ToInt32(((double)gamesProcessed / (double)numberOfGames) *100), new ProgressInfo(ProgressTypeEnum.ProgressType.Games,gamesProcessed,numberOfGames, sg.Game.Name));
                    worker.ReportProgress(0, new ProgressInfo(ProgressTypeEnum.ProgressType.Files,0,files.Count));
                }
            }
            _storage.Close();
            Console.WriteLine(@"MainController>>> Savegames backed up!");
        }

        static void UploadDownloadProgress(Object sender, FileDataTransferEventArgs e)
        {
            _worker.ReportProgress(e.PercentageProgress, new ProgressInfo(ProgressTypeEnum.ProgressType.Upload, e.PercentageProgress, 100));
            Console.WriteLine(e.PercentageProgress);
            e.Cancel = false;
        }
    }
}
