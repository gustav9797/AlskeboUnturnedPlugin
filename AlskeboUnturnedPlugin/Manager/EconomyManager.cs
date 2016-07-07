using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlskeboUnturnedPlugin {
    public class EconomyManager {

        public static int getBalance(UnturnedPlayer player) {
            return AlskeboUnturnedPlugin.playerManager.getPlayerInt(player, "balance");
        }

        public static void setBalance(UnturnedPlayer player, int balance) {
            AlskeboUnturnedPlugin.playerManager.setPlayerData(player, "balance", balance);
            AlskeboUnturnedPlugin.databaseManager.setPlayerBalance(player.CSteamID, balance);
        }

        public static bool hasBalance(UnturnedPlayer player, int amount) {
            int balance = getBalance(player);
            if (amount <= balance)
                return true;
            return false;
        }
    }
}
