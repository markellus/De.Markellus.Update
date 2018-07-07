using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace De.Markellus.Update
{
    public static class Utils
    {
        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, true);
                }
            }
        }

        public static void DeleteDirectory(string dir, bool deleteDirItself = false)
        {
            if (!Directory.Exists(dir))
            {
                throw new DirectoryNotFoundException($"Can not delete {dir}: Object does not exist");
            }
            foreach (string file in Directory.GetFiles(dir))
            {
                File.Delete(file);
            }

            foreach (string subDir in Directory.GetDirectories(dir))
            {
                DeleteDirectory(subDir, true);
            }

            if (deleteDirItself)
            {
                Thread.Sleep(50); //Explorer Bugfix
                Directory.Delete(dir);
            }
        }

        public static void DispatcherInvoke(Action action)
        {
            Dispatcher dispatchObject = Application.Current?.Dispatcher;
            if (dispatchObject == null || dispatchObject.CheckAccess())
            {
                action();
            }
            else
            {
                dispatchObject.Invoke(action);
            }
        }
    }
}
