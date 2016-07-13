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
            AlskeboUnturnedPlugin.databaseManager.logPlayerAsync(player.CSteamID, PlayerLogType.BALANCE_SET, "NEW:" + balance);
        }

        public static void addBalance(UnturnedPlayer player, int amount) {
            setBalance(player, getBalance(player) + amount);
        }

        public static bool hasBalance(UnturnedPlayer player, int amount) {
            int balance = getBalance(player);
            if (amount <= balance)
                return true;
            return false;
        }
    }
}
