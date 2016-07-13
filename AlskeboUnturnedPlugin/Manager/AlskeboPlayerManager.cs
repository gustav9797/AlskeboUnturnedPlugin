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
        PAY,
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

        public void setPlayerData(UnturnedPlayer player, String data, object value) {
            Dictionary<string, object> dict = playerData[player.CSteamID];
            if (dict == null)
                dict = new Dictionary<string, object>();
            if (dict.ContainsKey(data))
                dict[data] = value;
            else
                dict.Add(data, value);
            playerData[player.CSteamID] = dict;
        }

        public object getPlayerData(UnturnedPlayer player, String data) {
            Dictionary<string, object> dict = playerData[player.CSteamID];
            if (dict == null || !dict.ContainsKey(data))
                return null;
            return dict[data];
        }

        public bool getPlayerBoolean(UnturnedPlayer player, String data) {
            Dictionary<string, object> dict = playerData[player.CSteamID];
            if (dict == null || !dict.ContainsKey(data))
                return false;
            return (bool)dict[data];
        }

        public int getPlayerInt(UnturnedPlayer player, String data) {
            Dictionary<string, object> dict = playerData[player.CSteamID];
            if (dict == null || !dict.ContainsKey(data))
                return 0;
            return (int)dict[data];
        }
    }
}
