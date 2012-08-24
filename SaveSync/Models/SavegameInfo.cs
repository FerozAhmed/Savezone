using System;
using System.Collections.Generic;
using System.IO;
using SaveSync.Controllers;

namespace SaveSync.Models
{
    class SavegameInfo
    {
        private Game _game;
        private String _savegamelocation, _pc;
        private Boolean _manuallyAdded, _locationExists;
        private String _specialPath;

        public SavegameInfo()
        {
        }

        public SavegameInfo(Game game, string savegamelocation, string pc, String specialPath, Boolean manuallyadded = false)
        {
            _game = game;
            _savegamelocation = savegamelocation;
            _pc = pc;
            _specialPath = specialPath;
        }

        public Game Game
        {
            get { return _game; }
            set { _game = value; }
        }

        public string Savegamelocation
        {
            get { return _savegamelocation; }
            set { _savegamelocation = value; }
        }

        public bool LocationExists
        {
            get { return Directory.Exists(Savegamelocation); }
        }

        public string Pc
        {
            get { return _pc; }
            set { _pc = value; }
        }

        public bool ManuallyAdded
        {
            get { return _manuallyAdded; }
            set { _manuallyAdded = value; }
        }

        public string SpecialPath
        {
            get { return _specialPath; }
            set { _specialPath = value; }
        }

        public static List<SavegameInfo> GetSavegames()
        {
            return GamesController.GetDatabaseData();
        }

        public override string ToString()
        {
            return Game.Name + " / Installed " + Game.IsInstalled + " - " + Savegamelocation ;
        }
    }
}
