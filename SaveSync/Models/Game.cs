using System;
using System.IO;

namespace SaveSync.Models
{
    class Game
    {
        private int _id;
        private String _name, _installpath;
        private Boolean _isInstalled;

        public Game()
        {
        }

        public Game(string name, string installpath)
        {
            _name = name;
            _installpath = installpath;
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Installpath
        {
            get { return _installpath; }
            set { _installpath = value; }
        }

        public bool IsInstalled
        {
            get { return Directory.Exists(Installpath); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
