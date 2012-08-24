namespace SaveSync.Models
{
    class ProgressInfo
    {
        private ProgressTypeEnum.ProgressType _progressType;
        private int _currentIndex;
        private int _total;
        private string _gamename;

        public ProgressTypeEnum.ProgressType ProgressType
        {
            get { return _progressType; }
            set { _progressType = value; }
        }

        public int CurrentIndex
        {
            get { return _currentIndex; }
            set { _currentIndex = value; }
        }

        public int Total
        {
            get { return _total; }
            set { _total = value; }
        }

        public string Gamename
        {
            get { return _gamename; }
            set { _gamename = value; }
        }

        public ProgressInfo(ProgressTypeEnum.ProgressType progressType, int currentIndex, int total)
        {
            _progressType = progressType;
            _currentIndex = currentIndex;
            _total = total;
        }

        public ProgressInfo(ProgressTypeEnum.ProgressType progressType, int currentIndex, int total, string gamename)
        {
            _progressType = progressType;
            _currentIndex = currentIndex;
            _total = total;
            _gamename = gamename;
        }
    }
}
