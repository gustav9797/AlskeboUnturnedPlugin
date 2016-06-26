using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlskeboUnturnedPlugin {
    public class AlskeboPlayerManager {
        Dictionary<CSteamID, Dictionary<String, bool>> playerData = new Dictionary<CSteamID, Dictionary<string, bool>>();

        public void onPlayerConnected(UnturnedPlayer player) {
            if (playerData.ContainsKey(player.CSteamID))
                playerData.Remove(player.CSteamID);
            playerData.Add(player.CSteamID, new Dictionary<string, bool>());
        }

        public void onPlayerDisconnected(UnturnedPlayer player) {
            if (playerData.ContainsKey(player.CSteamID))
                playerData.Remove(player.CSteamID);
        }

        public void setPlayerData(UnturnedPlayer player, String data, bool value) {
            Dictionary<string, bool> dict = playerData[player.CSteamID];
            if (dict == null)
                dict = new Dictionary<string, bool>();
            if (dict.ContainsKey(data))
                dict[data] = value;
            else
                dict.Add(data, value);
            playerData[player.CSteamID] = dict;
        }

        public bool getPlayerData(UnturnedPlayer player, String data) {
            Dictionary<string, bool> dict = playerData[player.CSteamID];
            if (dict == null || !dict.ContainsKey(data))
                return false;
            return dict[data];
        }
    }
}
