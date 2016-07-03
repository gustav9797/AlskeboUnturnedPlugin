using System;
using System.Collections.Generic;

using System.Text;
using Rocket.Unturned.Chat;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Steamworks;
using SDG.Unturned;

namespace AlskeboUnturnedPlugin {
    public struct DatabaseVehicle {
        public long id;
        public ulong ownerSteamId;
        public ulong groupSteamId;
        public ushort type;
        public float x, y, z, rx, ry, rz, rw;
        public ushort fuel;
        public ushort health;
        public bool locked;

        public static DatabaseVehicle fromInteractableVehicle(long id, ulong ownerSteamId, ulong groupSteamId, InteractableVehicle vehicle) {
            DatabaseVehicle output = new DatabaseVehicle();
            output.id = id;
            output.ownerSteamId = ownerSteamId;
            output.groupSteamId = groupSteamId;
            output.type = vehicle.id;
            output.x = vehicle.transform.position.x;
            output.y = vehicle.transform.position.y;
            output.z = vehicle.transform.position.z;
            output.rx = vehicle.transform.rotation.x;
            output.ry = vehicle.transform.rotation.y;
            output.rz = vehicle.transform.rotation.z;
            output.rw = vehicle.transform.rotation.w;
            output.fuel = vehicle.fuel;
            output.health = vehicle.fuel;
            output.locked = vehicle.isLocked;
            return output;
        }
    }

    public class DatabaseManager {
        private String databaseIP;

        public DatabaseManager(AlskeboConfiguration config) {
            new I18N.West.CP1250();
            databaseIP = config.databaseIP;
        }

        private MySqlConnection createConnection() {
            MySqlConnection connection = null;
            //51.255.174.193
            try {
                connection = new MySqlConnection(String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", databaseIP, "unturned", "unturned", "13421342", "3306"));
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
                reader.Close();
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
                command.ExecuteNonQuery();
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
                command.ExecuteNonQuery();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }

        public long insertOwnedVehicle(CSteamID id, CSteamID group, InteractableVehicle vehicle) {
            long returnId = -1;
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO vehicles (ownersteamid, groupsteamid, type, x, y, z, rx, ry, rz, rw, fuel, health, locked) VALUES (@1, @12, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @13); SELECT last_insert_id();";
                command.Parameters.AddWithValue("@1", id.m_SteamID);
                command.Parameters.AddWithValue("@2", vehicle.id);
                command.Parameters.AddWithValue("@3", vehicle.transform.position.x);
                command.Parameters.AddWithValue("@4", vehicle.transform.position.y);
                command.Parameters.AddWithValue("@5", vehicle.transform.position.z);
                command.Parameters.AddWithValue("@6", vehicle.transform.rotation.x);
                command.Parameters.AddWithValue("@7", vehicle.transform.rotation.y);
                command.Parameters.AddWithValue("@8", vehicle.transform.rotation.z);
                command.Parameters.AddWithValue("@9", vehicle.transform.rotation.w);
                command.Parameters.AddWithValue("@10", vehicle.fuel);
                command.Parameters.AddWithValue("@11", vehicle.health);
                command.Parameters.AddWithValue("@12", group.m_SteamID);
                command.Parameters.AddWithValue("@13", vehicle.isLocked.ToString());
                connection.Open();
                returnId = Convert.ToInt32(command.ExecuteScalar());
                connection.Close();

            } catch (Exception e) {
                Logger.Log(e);
            }
            return returnId;
        }

        public List<DatabaseVehicle> receiveOwnedVehicles() {
            List<DatabaseVehicle> output = new List<DatabaseVehicle>();
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM vehicles";
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows) {
                    while (reader.Read()) {
                        DatabaseVehicle vehicle = new DatabaseVehicle();
                        vehicle.id = reader.GetInt64(0);
                        vehicle.ownerSteamId = reader.GetUInt64(1);
                        vehicle.groupSteamId = reader.GetUInt64(2);
                        vehicle.type = reader.GetUInt16(3);
                        vehicle.x = reader.GetFloat(4);
                        vehicle.y = reader.GetFloat(5);
                        vehicle.z = reader.GetFloat(6);
                        vehicle.rx = reader.GetFloat(7);
                        vehicle.ry = reader.GetFloat(8);
                        vehicle.rz = reader.GetFloat(9);
                        vehicle.rw = reader.GetFloat(10);
                        vehicle.fuel = reader.GetUInt16(11);
                        vehicle.health = reader.GetUInt16(12);
                        vehicle.locked = reader.GetBoolean(13);
                        output.Add(vehicle);
                    }
                }
                reader.Close();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
            return output;
        }

        public void updateVehicle(DatabaseVehicle vehicle) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE vehicles SET x = @1, y = @2, z = @3, rx = @4, ry = @5, rz = @6, rw = @7, fuel = @8, health = @9, groupsteamid = @11, locked = @12 WHERE id = @10";
                command.Parameters.AddWithValue("@1", vehicle.x);
                command.Parameters.AddWithValue("@2", vehicle.y);
                command.Parameters.AddWithValue("@3", vehicle.z);
                command.Parameters.AddWithValue("@4", vehicle.rx);
                command.Parameters.AddWithValue("@5", vehicle.ry);
                command.Parameters.AddWithValue("@6", vehicle.rz);
                command.Parameters.AddWithValue("@7", vehicle.rw);
                command.Parameters.AddWithValue("@8", vehicle.fuel);
                command.Parameters.AddWithValue("@9", vehicle.health);
                command.Parameters.AddWithValue("@10", vehicle.id);
                command.Parameters.AddWithValue("@11", vehicle.groupSteamId);
                command.Parameters.AddWithValue("@12", vehicle.locked.ToString());
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }

        public void deleteVehicle(long id) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM vehicles WHERE id = @1;";
                command.Parameters.AddWithValue("@1", id);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }

        public String getPlayerDisplayName(CSteamID id) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM players WHERE steamid = '" + id.m_SteamID + "';";
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                String name = null;
                if (reader.HasRows && reader.Read()) {
                    name = reader.GetString("displayname");
                }
                reader.Close();
                connection.Close();
                return name;
            } catch (Exception e) {
                Logger.Log(e);
            }
            return null;
        }

        public void setPlayerLastJoin(CSteamID id) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE players SET lastjoin = now() WHERE steamid = '" + id.m_SteamID + "';";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }

        public void setVehicleLastActivity(long id) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE vehicles SET lastactivity = now() WHERE id = '" + id + "';";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }
    }
}
