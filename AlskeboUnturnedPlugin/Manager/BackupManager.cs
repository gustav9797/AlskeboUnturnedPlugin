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
using System.Diagnostics;
using SDG.Unturned;

namespace AlskeboUnturnedPlugin {
    public class BackupManager {
        private System.Timers.Timer timer;

        public BackupManager() {
            timer = new System.Timers.Timer(1741111);
            timer.Elapsed += backup;
            timer.Start();
        }

        ~BackupManager() {
            timer.Stop();
        }

        private void backup(object sender, ElapsedEventArgs e) {
            if (Provider.clients.Count > 0) {
                Logger.Log("Creating a backup...");
                doBackup(new System.Action(delegate () {
                    Logger.Log("Done.");
                }));
            }
        }

        public static void doBackup(System.Action onFinished) {
            new Thread(delegate () {
                String levelFolder = ".." + Path.DirectorySeparatorChar + "Level";
                String playersFolder = ".." + Path.DirectorySeparatorChar + "Players";
                String backupFolder = "Backups";
                String backupFile = backupFolder + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".zip";

                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                String tempFolder = backupFolder + Path.DirectorySeparatorChar + "temp";
                if (!Directory.Exists(tempFolder))
                    Directory.CreateDirectory(tempFolder);

                String tempFile = tempFolder + Path.DirectorySeparatorChar + "mysqldump.sql";
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                var info = new ProcessStartInfo();
                info.FileName = "mysqldump";
                info.Arguments = AlskeboUnturnedPlugin.Config.database + @" -h " +
                AlskeboUnturnedPlugin.Config.databaseIP + " -u " +
                AlskeboUnturnedPlugin.Config.username + " -p" +
                AlskeboUnturnedPlugin.Config.password + " --result-file=" + tempFile;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;
                var p = Process.Start(info);
                p.ErrorDataReceived += new DataReceivedEventHandler(delegate (object s, DataReceivedEventArgs e) {
                    Logger.Log(e.Data);
                });
                p.StandardOutput.ReadToEnd();
                p.BeginErrorReadLine();
                p.WaitForExit();

                using (ZipFile zip = new ZipFile()) {
                    zip.AddDirectory(levelFolder, "Level");
                    zip.AddDirectory(playersFolder, "Players");

                    if (File.Exists(tempFile)) {
                        zip.AddFile(tempFile, "");
                    } else
                        Logger.LogWarning("Could not find mysqldump file for backup.");

                    zip.Save(backupFile);
                }

                TaskDispatcher.QueueOnMainThread(onFinished);
            }).Start();
        }
    }
}
