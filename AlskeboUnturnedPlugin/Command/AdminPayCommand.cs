using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class AdminPayCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "AdminPay"; }
        }

        public string Help {
            get { return "Admin pay."; }
        }

        public string Syntax {
            get { return "<player> <amount>"; }
        }

        public List<string> Aliases {
            get {
                List<string> a = new List<string>();
                a.Add("apay");
                return a;
            }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer sender = (UnturnedPlayer)caller;

            int amount;
            if (command.Length >= 2 && int.TryParse(command[1], out amount)) {
                UnturnedPlayer receiver = UnturnedPlayer.FromName(command[0]);
                if (receiver != null) {
                    EconomyManager.addBalance(receiver, amount);
                    UnturnedChat.Say(sender, "[Admin] You paid $" + amount + " to " + receiver.DisplayName + ".");
                    UnturnedChat.Say(receiver, "[Admin] You received $" + amount + " from " + sender.DisplayName + ".");
                } else
                    UnturnedChat.Say(sender, "Could not find the specified player.");
            } else
                UnturnedChat.Say(sender, "Usage: /adminpay " + Syntax);

        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.adminpay");
                return list;
            }
        }
    }
}
