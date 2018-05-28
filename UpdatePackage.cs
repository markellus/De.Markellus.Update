using System;

namespace De.Markellus.Update
{
    /// <summary>
    /// Enthält Angaben über ein einzelnes, zur Verfügung stehendes Update-Paket.
    /// </summary>
    public struct UpdatePackage
    {
        public Version Version;

        public string DownloadLink;

        public string PostLaunch;

        public string Arguments;
    }
}
