using Rocket.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using Ionic.Zip;
using System.Threading;
using Rocket.Core.Utils;

namespace AlskeboUnturnedPlugin {
    public class BackupManager {
        private System.Timers.Timer timer;

        public BackupManager() {
            timer = new System.Timers.Timer(1800000);
            timer.Elapsed += backup;
            timer.Start();
        }

        ~BackupManager() {
            timer.Stop();
        }

        private void backup(object sender, ElapsedEventArgs e) {
            Logger.Log("Creating a backup...");
            doBackup(new Action(delegate () {
                Logger.Log("Done.");
            }));
        }

        public static void doBackup(Action onFinished) {
            new Thread(delegate () {
                String levelFolder = ".." + Path.DirectorySeparatorChar + "Level";
                String playersFolder = ".." + Path.DirectorySeparatorChar + "Players";
                String backupFolder = "Backups";
                String backupFile = backupFolder + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".zip";

                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                using (ZipFile zip = new ZipFile()) {
                    zip.AddDirectory(levelFolder, "Level");
                    zip.AddDirectory(playersFolder, "Players");
                    zip.Save(backupFile);
                }

                TaskDispatcher.QueueOnMainThread(onFinished);
            }).Start();
        }
    }
}
