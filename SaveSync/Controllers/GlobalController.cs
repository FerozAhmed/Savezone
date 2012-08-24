namespace SaveSync.Controllers
{
    static class GlobalController
    {
        public enum ProviderEnum
        {
            DropBox, BoxNet
        }

        private static ProviderEnum _provider;

        public static ProviderEnum Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }
    }
}