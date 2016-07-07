using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class DepositMoneyCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "DepositMoney"; }
        }

        public string Help {
            get { return "Deposits your money items."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            //List<byte> pages = new List<byte>();
            //List<byte> indices = new List<byte>();

            Dictionary<ushort, int> valueItems = new Dictionary<ushort, int>();
            valueItems.Add(1051, 5);
            valueItems.Add(1052, 10);
            valueItems.Add(1053, 20);
            valueItems.Add(1054, 50);
            valueItems.Add(1055, 100);
            valueItems.Add(1056, 1);
            valueItems.Add(1057, 2);

            int totalValue = 0;
        start:
            foreach (Items items in player.Inventory.Items) {
                if (items != null) {
                    for (byte index = 0; index < items.getItemCount(); ++index) {
                        ItemJar itemJar = items.getItem(index);
                        if (itemJar != null && itemJar.Item != null && valueItems.ContainsKey(itemJar.Item.ItemID)) {
                            totalValue += valueItems[itemJar.Item.ItemID];
                            player.Inventory.removeItem(items.page, index);
                            goto start;
                        }
                    }
                }
            }

            if (totalValue > 0) {
                player.Inventory.sendStorage();
                int current = EconomyManager.getBalance(player);
                current += totalValue;
                EconomyManager.setBalance(player, current);
                UnturnedChat.Say(player, "$" + totalValue + " has been deposited, you now have $" + current + ".");
            } else {
                UnturnedChat.Say(player, "You do not have any money in your inventory.");
                UnturnedChat.Say(player, "You need to collect cash such as $5, $10, $50 bills but also loonies and toonies.");
            }

        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.depositmoney");
                return list;
            }
        }
    }
}
