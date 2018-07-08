namespace De.Markellus.Update
{
    internal static class Config
    {
#if DEBUG
        public static string RemoteUpdatePath = "http://127.0.0.1/";
#else
        public static string RemoteUpdatePath     = "https://markellus.de/AppCenter/UpdateService/{0}/";
#endif
        public static string RemoteUpdateInfoPath = RemoteUpdatePath + "Version.xml";
    }
}
