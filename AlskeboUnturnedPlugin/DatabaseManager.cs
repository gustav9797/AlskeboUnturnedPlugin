using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned.Chat;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Steamworks;

namespace AlskeboUnturnedPlugin {
    public class DatabaseManager {

        public DatabaseManager() {
            new I18N.West.CP1250();
        }

        private MySqlConnection createConnection() {
            MySqlConnection connection = null;
            try {
                connection = new MySqlConnection(String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", "127.0.0.1", "unturned", "root", "13421342", "3306"));
            } catch (Exception ex) {
                Logger.LogException(ex);
            }
            return connection;
        }

        public bool playerExists(CSteamID id) {
            bool exists = false;
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM players WHERE steamid = '" + id.m_SteamID + "';";
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                exists = reader.HasRows;
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
            return exists;
        }

        public void insertPlayer(CSteamID id, string displayName, bool receivedVehicle) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO players (steamid, displayname, receivedvehicle) VALUES ('" + id.m_SteamID + "','" + displayName + "','" + receivedVehicle.ToString() + "');";
                connection.Open();
                command.ExecuteNonQueryAsync();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }

        public bool playerHasReceivedVehicle(CSteamID id) {
            bool has = false;
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT receivedvehicle FROM players WHERE steamid = '" + id.m_SteamID + "';";
                connection.Open();
                object result = command.ExecuteScalar();
                Boolean.TryParse(result.ToString(), out has);
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
            return has;
        }

        public void setPlayerReceivedVehicle(CSteamID id, bool receivedVehicle) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE players SET receivedvehicle = '" + receivedVehicle.ToString() + "' WHERE steamid = '" + id.m_SteamID + "';";
                connection.Open();
                command.ExecuteNonQueryAsync();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }
    }
}
