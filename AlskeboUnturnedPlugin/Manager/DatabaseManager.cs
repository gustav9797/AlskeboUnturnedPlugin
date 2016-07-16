using System;
using System.Collections.Generic;

using System.Text;
using Rocket.Unturned.Chat;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Steamworks;
using SDG.Unturned;
using System.Threading;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class DatabaseVehicle {
        public long id;
        public ulong ownerSteamId;
        public ulong groupSteamId;
        public ushort type;
        public Vector3 position;
        public Quaternion rotation;
        public ushort fuel;
        public ushort health;
        public bool locked;
        public bool isNoob;

        public DatabaseVehicle(long id, ulong ownerSteamId, ulong groupSteamId, ushort type, Vector3 position, Quaternion rotation, ushort fuel, ushort health, bool locked, bool isNoob) {
            this.id = id;
            this.ownerSteamId = ownerSteamId;
            this.groupSteamId = groupSteamId;
            this.type = type;
            this.position = position;
            this.rotation = rotation;
            this.fuel = fuel;
            this.health = health;
            this.isNoob = isNoob;
        }

        public static DatabaseVehicle fromInteractableVehicle(long id, ulong ownerSteamId, ulong groupSteamId, bool isNoob, InteractableVehicle vehicle) {
            DatabaseVehicle output = new DatabaseVehicle(id, ownerSteamId, groupSteamId, vehicle.id, vehicle.transform.position, vehicle.transform.rotation, vehicle.fuel, vehicle.health, vehicle.isLocked, isNoob);
            return output;
        }
    }

    public class DatabasePlayer {
        public ulong steamId;
        public ulong groupSteamId;
        public string displayName;
        public string steamName;
        public string characterName;
        public bool receivedVehicle;
        public int balance;
        public DateTime firstJoin;
        public DateTime lastJoin;
    }

    public class DatabaseManager {
        private String databaseIP;
        private string database;
        private string username;
        private string password;

        public DatabaseManager(AlskeboConfiguration config) {
            new I18N.West.CP1250();
            databaseIP = config.databaseIP;
            database = config.database;
            username = config.username;
            password = config.password;
        }

        private MySqlConnection createConnection() {
            MySqlConnection connection = null;
            //51.255.174.193
            try {
                connection = new MySqlConnection(String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", databaseIP, database, username, password, "3306"));
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

        public void insertPlayer(CSteamID id, CSteamID groupId, string displayName, string steamName, string characterName, bool receivedVehicle) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO players (steamid, groupsteamid, displayname, steamname, charactername, receivedvehicle) VALUES (@1, @2, @3, @4, @5, @6);";
                command.Parameters.AddWithValue("@1", id.m_SteamID);
                command.Parameters.AddWithValue("@2", groupId.m_SteamID);
                command.Parameters.AddWithValue("@3", displayName);
                command.Parameters.AddWithValue("@4", steamName);
                command.Parameters.AddWithValue("@5", characterName);
                command.Parameters.AddWithValue("@6", receivedVehicle.ToString());
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }

        public DatabasePlayer receivePlayer(CSteamID id) {
            DatabasePlayer output = null;
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM players WHERE steamid = '" + id.m_SteamID + "';";
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows) {
                    if (reader.Read()) {
                        output = new DatabasePlayer();
                        output.steamId = reader.GetUInt64("steamid");
                        output.groupSteamId = reader.GetUInt64("groupsteamid");
                        output.displayName = reader.GetString("displayname");
                        output.steamName = reader.GetString("steamname");
                        output.characterName = reader.GetString("charactername");
                        output.receivedVehicle = reader.GetBoolean("receivedvehicle");
                        output.balance = reader.GetInt32("balance");
                        output.firstJoin = reader.GetDateTime("firstjoin");
                        output.lastJoin = reader.GetDateTime("lastjoin");
                    }
                }
                reader.Close();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
            return output;
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

        public void setPlayerBalance(CSteamID id, int balance) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE players SET balance = '" + balance + "' WHERE steamid = '" + id.m_SteamID + "';";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }

        public long insertOwnedVehicle(CSteamID id, CSteamID group, bool noob, InteractableVehicle vehicle) {
            long returnId = -1;
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO vehicles (ownersteamid, groupsteamid, type, x, y, z, rx, ry, rz, rw, fuel, health, locked, noob) VALUES (@1, @12, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @13, @14); SELECT last_insert_id();";
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
                command.Parameters.AddWithValue("@14", noob.ToString());
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
                        //TODO: read column names instead of index
                        DatabaseVehicle vehicle = new DatabaseVehicle(
                            reader.GetInt64("id"),
                            reader.GetUInt64("ownersteamid"),
                            reader.GetUInt64("groupsteamid"),
                            reader.GetUInt16("type"),
                            new Vector3(reader.GetFloat("x"), reader.GetFloat("y"), reader.GetFloat("z")),
                            new Quaternion(reader.GetFloat("rx"), reader.GetFloat("ry"), reader.GetFloat("rz"), reader.GetFloat("rw")),
                            reader.GetUInt16("fuel"),
                            reader.GetUInt16("health"),
                            reader.GetBoolean("locked"),
                            reader.GetBoolean("noob")
                            );
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
                command.CommandText = "UPDATE vehicles SET x = @1, y = @2, z = @3, rx = @4, ry = @5, rz = @6, rw = @7, fuel = @8, health = @9, groupsteamid = @11, locked = @12, noob = @13 WHERE id = @10";
                command.Parameters.AddWithValue("@1", vehicle.position.x);
                command.Parameters.AddWithValue("@2", vehicle.position.y);
                command.Parameters.AddWithValue("@3", vehicle.position.z);
                command.Parameters.AddWithValue("@4", vehicle.rotation.x);
                command.Parameters.AddWithValue("@5", vehicle.rotation.y);
                command.Parameters.AddWithValue("@6", vehicle.rotation.z);
                command.Parameters.AddWithValue("@7", vehicle.rotation.w);
                command.Parameters.AddWithValue("@8", vehicle.fuel);
                command.Parameters.AddWithValue("@9", vehicle.health);
                command.Parameters.AddWithValue("@10", vehicle.id);
                command.Parameters.AddWithValue("@11", vehicle.groupSteamId);
                command.Parameters.AddWithValue("@12", vehicle.locked.ToString());
                command.Parameters.AddWithValue("@13", vehicle.isNoob.ToString());
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

        public void logVehicleAsync(long databaseId, VehicleLogType logType, String data = "") {
            new Thread(delegate () {
                try {
                    MySqlConnection connection = createConnection();
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO vehiclelog (vehicleid, type, data) VALUES (@1, @2, @3);";
                    command.Parameters.AddWithValue("@1", databaseId);
                    command.Parameters.AddWithValue("@2", logType.ToString());
                    command.Parameters.AddWithValue("@3", data);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                } catch (Exception e) {
                    Logger.Log(e);
                }
            }).Start();
        }

        public void logPlayerAsync(CSteamID id, PlayerLogType logType, String data = "") {
            new Thread(delegate () {
                try {
                    MySqlConnection connection = createConnection();
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO playerlog (steamid, type, data) VALUES (@1, @2, @3);";
                    command.Parameters.AddWithValue("@1", id.m_SteamID);
                    command.Parameters.AddWithValue("@2", logType.ToString());
                    command.Parameters.AddWithValue("@3", data);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                } catch (Exception e) {
                    Logger.Log(e);
                }
            }).Start();
        }

        public void updatePlayer(CSteamID id, CSteamID groupId, string displayName, string steamName, string characterName) {
            try {
                MySqlConnection connection = createConnection();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE players SET groupsteamid = @1, displayname = @2, steamname = @3, charactername = @4 WHERE steamid = @5";
                command.Parameters.AddWithValue("@1", groupId.m_SteamID);
                command.Parameters.AddWithValue("@2", displayName);
                command.Parameters.AddWithValue("@3", steamName);
                command.Parameters.AddWithValue("@4", characterName);
                command.Parameters.AddWithValue("@5", id.m_SteamID);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            } catch (Exception e) {
                Logger.Log(e);
            }
        }
    }
}
