using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class PayCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "Pay"; }
        }

        public string Help {
            get { return "Pay another player."; }
        }

        public string Syntax {
            get { return "<player> <amount>"; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer sender = (UnturnedPlayer)caller;

            int amount;
            if (command.Length >= 2 && int.TryParse(command[1], out amount) && amount > 0) {
                UnturnedPlayer receiver = UnturnedPlayer.FromName(command[0]);
                if (receiver != null) {
                    if (EconomyManager.hasBalance(sender, amount)) {
                        EconomyManager.setBalance(sender, EconomyManager.getBalance(sender) - amount);
                        AlskeboUnturnedPlugin.databaseManager.logPlayerAsync(sender.CSteamID, PlayerLogType.PAY, receiver.CSteamID + "");
                        EconomyManager.addBalance(receiver, amount);
                        AlskeboUnturnedPlugin.databaseManager.logPlayerAsync(receiver.CSteamID, PlayerLogType.PAID, sender.CSteamID + "");
                        UnturnedChat.Say(sender, "You paid $" + amount + " to " + receiver.DisplayName + ".");
                        UnturnedChat.Say(receiver, "You received $" + amount + " from " + sender.DisplayName + ".");
                    } else
                        UnturnedChat.Say(sender, "You do not have that much money.");
                } else
                    UnturnedChat.Say(sender, "Could not find the specified player.");
            } else
                UnturnedChat.Say(sender, "Usage: /pay " + Syntax);

        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.pay");
                return list;
            }
        }
    }
}
