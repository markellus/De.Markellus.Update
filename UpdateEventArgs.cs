using System;

namespace De.Markellus.Update
{
    public enum UpdateStatus
    {
        Idle,
        /// <summary>
        /// Es werden Informationen über vorhandene Updates geladen.
        /// </summary>
        LoadInfo,
        /// <summary>
        /// Das Laden der Informationen ist abgeschlossen.
        /// </summary>
        LoadInfoFinished,
        /// <summary>
        /// Es wird ein Update-Paket heruntergeladen.
        /// </summary>
        DownloadingPackage,
        /// <summary>
        /// Das Herunterladen eines Update-Pakets ist abgeschlossen.
        /// </summary>
        DownloadingPackageFinished,
        /// <summary>
        /// Ein Update_Paket wird entpackt
        /// </summary>
        Unpacking,
        /// <summary>
        /// Das Entpacken eines Update-Pakets ist abgeschlossen.
        /// </summary>
        UnpackingFinished,
        /// <summary>
        /// Wir befinden uns in der Instanz eines Update-Pakets.
        /// Das Update sollte jetzt installiert werden.
        /// </summary>
        UpdateSessionStarted,
        /// <summary>
        /// Ein Update wird installiert.
        /// </summary>
        Installing,
        /// <summary>
        /// Der Update-Server ist offline.
        /// </summary>
        ErrorServerDown,
        /// <summary>
        /// Der Update-Server sendet korrupte Info-Daten.
        /// </summary>
        ErrorInvalidInfo,
        /// <summary>
        /// Der Download des Updates ist fehlgeschlagen.
        /// </summary>
        ErrorDownloadFailed,
        /// <summary>
        /// Der Download des Updates wurde unerwartet unterbrochen.
        /// </summary>
        ErrorDownloadInterrupted,
        /// <summary>
        /// Das Entpacken des Update-Paketes ist fehlgeschlagen.
        /// </summary>
        ErrorUnpackingFailed,
        /// <summary>
        /// Das Installieren des Updates ist fehlgeschlagen.
        /// </summary>
        ErrorInstallFailed,
    }

    public class UpdateEventArgs : EventArgs
    {
        public UpdateStatus Status { get; private set; }

        public int ProgressPercentage { get; private set; }

        public UpdateEventArgs(UpdateStatus status, int percentage = -1)
        {         
            this.Status = status;
            this.ProgressPercentage = percentage;
        }
    }
}
