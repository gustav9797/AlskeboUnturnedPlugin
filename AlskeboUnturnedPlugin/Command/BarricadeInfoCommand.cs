using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class BarricadeInfoCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "BarricadeInfo"; }
        }

        public string Help {
            get { return "Shows barricade info."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get {
                List<string> a = new List<string>();
                a.Add("BInfo");
                return a;
            }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            bool current = AlskeboUnturnedPlugin.playerManager.getPlayerBoolean(player, "barricadeinfo");
            AlskeboUnturnedPlugin.playerManager.setPlayerData(player, "barricadeinfo", !current);
            UnturnedChat.Say(caller, "Showing barricade info: " + ((!current).ToString().ToLower()));
            if (!current) {
                UnturnedChat.Say(caller, "Punch near your barricades to show their health.");
                AlskeboUnturnedPlugin.playerManager.setPlayerData(player, "structureinfo", false);
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.barricadeinfo");
                return list;
            }
        }
    }
}
