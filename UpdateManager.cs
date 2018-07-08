using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace De.Markellus.Update
{
    public class UpdateManager
    {
        private readonly string _programName = Assembly.GetEntryAssembly().GetName().Name;
        private readonly string _tmpPath;
        private string _argsPath;
        private UpdatePackage _selectedPackage;

        public event EventHandler<UpdateEventArgs> UpdateStateChanged;

        public UpdateInfo UpdateInfo { get; private set; }

        public UpdateManager(string programName = null)
        {
            if(programName != null)
            {
                _programName = programName;
            }
            _tmpPath = Path.Combine(Path.GetTempPath(), _programName, "Update");
        }

        public void LoadUpdateInfo()
        {
            new Thread(() =>
            {
                FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.LoadInfo));

                bool isUpdateSession = false;

                string[] args = Environment.GetCommandLineArgs();

                try
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        switch (args[i])
                        {
                            case "/update":
                                isUpdateSession = true;
                                break;
                            case "/path":
                                _argsPath = args[i + 1];
                                break;
                        }
                    }

                    if (isUpdateSession)
                    {
                        FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.UpdateSessionStarted, -1));
                    }
                    else
                    {
                        UpdateInfo = new UpdateInfo(_programName);
                        UpdateInfo.Load();
                        FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.LoadInfoFinished, 100));
                    }
                }
                catch (WebException ex)
                {
                    FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.ErrorServerDown, -1));
                }
                catch(XmlException ex)
                {
                    FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.ErrorInvalidInfo, -1));
                }
            }).Start();
        }

        public void DownloadUpdate(UpdatePackage package)
        {
            _selectedPackage = package;

            new Thread(() =>
            {
                FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.DownloadingPackage, 0));
                WebClient client = new WebClient();
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                client.DownloadDataCompleted += Client_DownloadDataCompleted;
                try
                {
                    client.DownloadDataAsync(new Uri(package.DownloadLink));
                }
                catch (Exception ex)
                {
                    FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.ErrorDownloadFailed, -1));
                }
            }).Start();
        }

        private void Unpack(byte[] data)
        {
            new Thread(() =>
            {
                try
                {
                    FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.Unpacking, 0));

                    if (Directory.Exists(_tmpPath))
                    {
                        Directory.Delete(_tmpPath, true);
                    }

                    Directory.CreateDirectory(_tmpPath);

                    using (MemoryStream stream = new MemoryStream())
                    {
                        stream.Write(data, 0, data.Length);
                        using (ZipArchive archive = new ZipArchive(stream))
                        {
                            int counter = 0;
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (!entry.FullName.EndsWith("/"))
                                {
                                    FileInfo fi = new FileInfo(Path.Combine(_tmpPath, entry.FullName));
                                    if (!Directory.Exists(fi.DirectoryName))
                                    {
                                        Directory.CreateDirectory(fi.DirectoryName);
                                    }
                                    entry.ExtractToFile(fi.FullName);
                                }
                                FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.Unpacking,
                                    (int)(100.0/archive.Entries.Count*++counter)));
                            }
                        }
                    }

                    FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.UnpackingFinished, 100));
                }
                catch (Exception ex)
                {
                    FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.ErrorUnpackingFailed, -1));
                }
            }).Start();
        }

        public void LaunchUpdateSession()
        {
            Process updater = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(_tmpPath, _selectedPackage.PostLaunch),
                    Arguments = _selectedPackage.Arguments,
                    Verb = "runas"
                }
            };
            updater.Start();
            Environment.Exit(0);
        }

        public void InstallUpdate()
        {
            new Thread(() =>
            {
                try
                {
                    FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.Installing, -1));

                    int pCount = 2;

                    while (pCount > 1)
                    {
                        Process[] p = Process.GetProcessesByName("Newsreader");
                        pCount = p.Length;
                        Process self = Process.GetCurrentProcess();

                        foreach (Process other in p.Where(t => t.Id != self.Id))
                        {
                            other.Kill();
                        }
                    }

                    Utils.DeleteDirectory(_argsPath);

                    Utils.CopyDirectory(_tmpPath, _argsPath, true);
#if DEBUG
                    Process.Start(Path.Combine(_argsPath, AppDomain.CurrentDomain.FriendlyName).Replace(".vshost", ""));
#else
                    Process.Start(Path.Combine(_argsPath, AppDomain.CurrentDomain.FriendlyName));
#endif
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.ErrorInstallFailed, -1));
                }
            }).Start();
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.DownloadingPackage, e.ProgressPercentage));
            }
            catch (Exception ex)
            {
                FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.ErrorDownloadInterrupted, -1));
            }
            
        }

        private void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.DownloadingPackageFinished, 100));
                Unpack(e.Result);
            }
            catch (Exception ex)
            {
                FireUpdateStateChanged(new UpdateEventArgs(UpdateStatus.ErrorDownloadFailed, -1));
            }
            
        }    

        private void FireUpdateStateChanged(UpdateEventArgs e)
        {
            Utils.DispatcherInvoke(() =>
            {
                EventHandler<UpdateEventArgs> local;

                lock (this)
                {
                    local = UpdateStateChanged;
                }
                local?.Invoke(this, e);
            });
        }
    }
}
