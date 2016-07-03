using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class StructureInfoCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "StructureInfo"; }
        }

        public string Help {
            get { return "Shows structure info."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            bool current = AlskeboUnturnedPlugin.playerManager.getPlayerData(player, "structureinfo");
            AlskeboUnturnedPlugin.playerManager.setPlayerData(player, "structureinfo", !current);
            UnturnedChat.Say(caller, "Showing structure info: " + ((!current).ToString().ToLower()));
            if (!current) {
                UnturnedChat.Say(caller, "Punch near your structures to show their health.");
                AlskeboUnturnedPlugin.playerManager.setPlayerData(player, "barricadeinfo", false);
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.structureinfo");
                return list;
            }
        }
    }
}
