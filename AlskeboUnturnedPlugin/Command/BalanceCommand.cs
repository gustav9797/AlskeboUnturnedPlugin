using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class BalanceCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "Balance"; }
        }

        public string Help {
            get { return "Shows your current balance."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            int current = AlskeboUnturnedPlugin.playerManager.getPlayerInt(player, "balance");
            UnturnedChat.Say(player, "Balance: $" + current);
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.balance");
                return list;
            }
        }
    }
}
