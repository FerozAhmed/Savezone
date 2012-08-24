using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using JsonFx.Json;
using Microsoft.Win32;
using SaveSync.Models;

namespace SaveSync.Controllers
{
    static class GamesController
    {

        public static List<SavegameInfo> GetDatabaseData()
        {
            var savegameInfos = new List<SavegameInfo>();

            var client = new WebClient();
            var response = "";

            try
            {
                response = client.DownloadString(new Uri("http://michieldemey.be/sgdb/games.php"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to the database.\nMake sure you are connected to the internet",
                                "Could not connect", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                throw;
            }

            var reader = new JsonReader();
            //var writer = new JsonWriter();

            var savegames = response.Trim().Split(';');
            Console.WriteLine(savegames);

            foreach(var gameJSON in savegames)
            {
                var game = new Game();
                var sgi = new SavegameInfo();

                var output = reader.Read<Dictionary<string, Object>>(gameJSON);

                if (output != null)
                {
                    game.Id = int.Parse(output["id"].ToString());
                    game.Name = output["GameName"].ToString();
                    sgi.Game = game;
                    sgi.SpecialPath = output["SpecialPath"].ToString().Replace("%","").Trim();

                    if (output["SpecialPath"].ToString().Equals("%REGISTRY%"))
                    {
                        //Console.WriteLine(output["GameName"].ToString() + @" - " + output["RegPath"].ToString());
                        sgi.Savegamelocation = output["RegPath"].ToString();
                    }
                    else if (output["SpecialPath"].ToString().Equals("%STEAM%"))
                    {
                        //Console.WriteLine(output["GameName"].ToString() + @" - STEAM Game");
                        var rk = Registry.CurrentUser;

                        var sl = rk.OpenSubKey(@"SOFTWARE\Valve\Steam");
                        sgi.Savegamelocation = sl.GetValue("SteamPath") + "\\" + output["Path"].ToString();
                        if(Directory.Exists(sgi.Savegamelocation))
                        {
                            Console.WriteLine(@"GamesController>>> Steam path: " + sgi.Savegamelocation);
                        }
                    }
                    else if (output["SpecialPath"].ToString().Equals("%STEAM_CLOUD%"))
                    {
                        //Console.WriteLine(output["GameName"].ToString() + @" - STEAM Game");
                        var rk = Registry.CurrentUser;

                        var sl = rk.OpenSubKey(@"SOFTWARE\Valve\Steam");
                        sgi.Savegamelocation = sl.GetValue("SteamPath") + "\\userdata";
                        var userdataDir = new DirectoryInfo(sgi.Savegamelocation);
                        foreach (DirectoryInfo d in userdataDir.GetDirectories())
                        {
                            if (Directory.Exists(d.FullName))
                            {
                                sgi.Savegamelocation = d.FullName + "\\" + output["Path"];
                                if (Directory.Exists(sgi.Savegamelocation))
                                {
                                    Console.WriteLine(@"GamesController>>> Steam cloud path: " + sgi.Savegamelocation);   
                                }
                            }
                        }
                    }
                    else
                    {
                        sgi.Savegamelocation = getSpecialPath(output["SpecialPath"].ToString());
                        if (output["Path"] != null)
                        {
                            sgi.Savegamelocation += "\\" + output["Path"].ToString();
                        }
                        else
                        {
                            sgi.Savegamelocation = "";
                        }
                        //Console.WriteLine(output["GameName"].ToString());
                    }
                    //json = writer.Write(output["GameName"]);
                    //MainController.Savegames.Add(sgi);
                    savegameInfos.Add(sgi);
                }
            }

            return savegameInfos;
        }

        public static String getSpecialPath(String specialpath)
        {
            String path = "";
            switch (specialpath)
            {
                case "%APPDATA%":
                    path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
                case "%APPDATA_COMMON%":
                    break;
                case "%DOCUMENTS%":
                    path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    break;
                case "%PROG_FILES_86%":
                    path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    break;
                case "%SAVED_GAMES%":
                    path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\";
                    break;
                case "%SHARED_DOCUMENTS%":
                    path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                    break;
                case "%USER_PROFILE%":
                    path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    break;
            }
            return path;
        }
    }
}
