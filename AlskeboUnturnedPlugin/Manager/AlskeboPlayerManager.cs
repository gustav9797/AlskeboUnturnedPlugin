using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlskeboUnturnedPlugin {

    public enum PlayerLogType {
        CONNECT,
        DISCONNECT,
        CHAT,
        BUY_VEHICLE,
        GIVEN_VEHICLE,
        PAY,
        PAID,
        BALANCE_SET,
        DEPOSIT_MONEY,
        BUY_TICKET,
        FIRSTVEHICLE
    }

    public class AlskeboPlayerManager {
        Dictionary<CSteamID, Dictionary<String, object>> playerData = new Dictionary<CSteamID, Dictionary<string, object>>();

        public void onPlayerConnected(UnturnedPlayer player) {
            if (playerData.ContainsKey(player.CSteamID))
                playerData.Remove(player.CSteamID);
            playerData.Add(player.CSteamID, new Dictionary<string, object>());
        }

        public void onPlayerDisconnected(UnturnedPlayer player) {
            if (playerData.ContainsKey(player.CSteamID))
                playerData.Remove(player.CSteamID);
        }

        public void setPlayerData(UnturnedPlayer player, String key, object value) {
            key = key.ToLower();
            Dictionary<string, object> dict = playerData[player.CSteamID];
            if (dict == null)
                dict = new Dictionary<string, object>();
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
            playerData[player.CSteamID] = dict;
        }

        public object getPlayerData(UnturnedPlayer player, String key) {
            key = key.ToLower();
            Dictionary<string, object> dict = playerData[player.CSteamID];
            if (dict == null || !dict.ContainsKey(key))
                return null;
            return dict[key];
        }

        public bool getPlayerBoolean(UnturnedPlayer player, String key, bool defaultValue = false) {
            key = key.ToLower();
            Dictionary<string, object> dict = playerData[player.CSteamID];
            if (dict == null || !dict.ContainsKey(key))
                return defaultValue;
            return (bool)dict[key];
        }

        public int getPlayerInt(UnturnedPlayer player, String key) {
            key = key.ToLower();
            Dictionary<string, object> dict = playerData[player.CSteamID];
            if (dict == null || !dict.ContainsKey(key))
                return 0;
            return (int)dict[key];
        }
    }
}
