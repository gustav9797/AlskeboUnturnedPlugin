using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlskeboUnturnedPlugin {
    public class StructureInfoCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Both; }
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
            if (caller is UnturnedPlayer) {
                UnturnedPlayer player = (UnturnedPlayer)caller;
                bool current = AlskeboUnturnedPlugin.playerManager.getPlayerData(player, "structureinfo");
                AlskeboUnturnedPlugin.playerManager.setPlayerData(player, "structureinfo", !current);
                UnturnedChat.Say(caller, "Showing structure info: " + ((!current).ToString().ToLower()));
            } else {
                UnturnedChat.Say(caller, "You must be in-game to execute this command.");
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("alskebo.structureinfo");
                return list;
            }
        }
    }
}
