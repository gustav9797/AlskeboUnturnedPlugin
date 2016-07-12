using Rocket.API;
using Rocket.Core.Assets;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class DebugCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "debugtest"; }
        }

        public string Help {
            get { return ""; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            Lottery.win(player);
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.debug");
                return list;
            }
        }
    }
}
