using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace De.Markellus.Update
{
    /// <summary>
    /// Enthält Infos über verfügbare Updates. Wird vom in der Config
    /// angegebenen Server geladen.
    /// </summary>
    public class UpdateInfo
    {
        private XDocument _doc;
        private string _programName;

        public List<UpdatePackage> Packages { get; private set; }

        internal UpdateInfo(string programName)
        {
            _programName = programName;
        }

        /// <summary>
        /// Läd die Update-Infos vom Server.
        /// </summary>
        public void Load()
        {
            Packages = new List<UpdatePackage>();
            byte[] data;
            try
            {
                data = new WebClient().DownloadData(string.Format(Config.RemoteUpdateInfoPath, _programName));
            }
            catch (Exception ex)
            {
                throw new WebException("The info server is unreachable. Please check the network connection.", ex);
            }
            

            _doc = XDocument.Parse(Encoding.UTF8.GetString(data));
            if (_doc.Root.Name != "appinfo") { throw new XmlException("Invalid XML-File: appinfo-root not found"); }

            //Pakete auslesen
            foreach (XElement el in _doc.Root.Elements().Where(i => i.Name.LocalName == "package"))
            {
                Packages.Add(new UpdatePackage()
                {
                    Version = new Version(el.Elements().First(i => i.Name.LocalName == "version").Value),
                    DownloadLink = el.Elements().First(i => i.Name.LocalName == "download").Value,
                    PostLaunch = el.Elements().First(i => i.Name.LocalName == "launchAfter").Value,
                    Arguments = String.Format(el.Elements().First(i => i.Name.LocalName == "launchArguments").Value, Path.GetFullPath("./").Trim('\\')),
                });
            }
        }

        /// <summary>
        /// Gibt das neueste/aktuellste Update-Paket zurück.
        /// </summary>
        public UpdatePackage GetNewest()
        {
            return Packages.First(p => p.Version == (Packages.Max(p2 => p2.Version)));
        }
    }
}
