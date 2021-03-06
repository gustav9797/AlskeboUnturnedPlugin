﻿using Rocket.API;
using Rocket.Core.Assets;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class BackupCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Both; }
        }

        public string Name {
            get { return "Backup"; }
        }

        public string Help {
            get { return "Create a backup."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedChat.Say(caller, "Creating a backup...");
            BackupManager.doBackup(new System.Action(delegate () {
                UnturnedChat.Say(caller, "Done.");
            }));
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.backup");
                return list;
            }
        }
    }
}
