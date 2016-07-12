﻿using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class AlskeboVehicleManager {
        private Dictionary<CSteamID, List<VehicleInfo>> playerOwnedVehicles = new Dictionary<CSteamID, List<VehicleInfo>>();
        private Dictionary<uint, VehicleInfo> vehicleOwners = new Dictionary<uint, VehicleInfo>();
        private Dictionary<uint, DestroyingVehicleInfo> vehiclesToBeDestroyed = new Dictionary<uint, DestroyingVehicleInfo>();
        private Dictionary<uint, DatabaseVehicle> lastSave = new Dictionary<uint, DatabaseVehicle>();
        private bool loadingVehicles = false;
        private bool removedDefaultVehicles = false;
        private static int vehicleDestroyMinutes = 5;
        private static int vehicleDestroyFuel = 10;
        public static Color vehicleManagerPrefix = Color.gray;

        List<ushort> vehicleDestroySounds = new List<ushort> { 19, 20, 35, 36, 37, 53, 61, 62, 63, 65, 69, 71, 72, 81, 89 };
        System.Random r = new System.Random();

        public AlskeboVehicleManager() {
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += destroyVehicles;
            timer.Start();

            System.Timers.Timer saveTimer = new System.Timers.Timer(5000);
            saveTimer.Elapsed += saveVehicles;
            saveTimer.Start();

            // Make sure to get server shutdown before vehicles are saved to file
            Provider.onServerShutdown += onServerShutdown;
            foreach (Delegate d in Provider.onServerShutdown.GetInvocationList()) {
                if (!d.Method.DeclaringType.ToString().Contains("Alskebo")) {
                    Provider.onServerShutdown -= (Provider.ServerShutdown)d;
                    Provider.onServerShutdown += (Provider.ServerShutdown)d;
                }
            }
            Provider.onServerShutdown += onLateServerShutdown;

            // onPrePreLevelLoaded run before vehicles are loaded
            Level.onPrePreLevelLoaded += onPrePreLevelLoaded;
            foreach (Delegate d in Level.onPrePreLevelLoaded.GetInvocationList()) {
                if (!d.Method.DeclaringType.ToString().Contains("Alskebo")) {
                    Level.onPrePreLevelLoaded -= (PrePreLevelLoaded)d;
                    Level.onPrePreLevelLoaded += (PrePreLevelLoaded)d;
                }
            }
            Level.onPrePreLevelLoaded += onLatePrePreLevelLoaded;
            Level.onLevelLoaded += onLevelLoaded;
        }

        public void onPluginLoaded() {

        }

        public void onPrePreLevelLoaded(int level) {
            Logger.Log("onPrePreLevelLoaded");
            Level.onPrePreLevelLoaded -= onPrePreLevelLoaded;
            String fileName = ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Level/" + Level.info.name + "/Vehicles.dat";
            if (File.Exists(fileName)) {
                File.Delete(fileName);
                Logger.Log("Deleted Vehicles.dat.");
            } else
                Logger.Log("Could not find Vehicles.dat.");
        }

        public void onLatePrePreLevelLoaded(int level) {
            Logger.Log("onLatePrePreLevelLoaded");
            loadingVehicles = true;
            Level.onLevelLoaded -= onLatePrePreLevelLoaded;

            Logger.Log("Receiving owned vehicles from database...");
            List<DatabaseVehicle> vehicles = AlskeboUnturnedPlugin.databaseManager.receiveOwnedVehicles();
            int naturalCount = 0;
            foreach (DatabaseVehicle v in vehicles) {
                if (v.ownerSteamId == 0)
                    naturalCount++;
            }

            int defaultVehicleCount = 24;
            removedDefaultVehicles = true;
            if (naturalCount >= defaultVehicleCount) {
                Logger.Log("Removing all default vehicles (" + VehicleManager.vehicles.Count + ")...");
            start:
                foreach (InteractableVehicle v in VehicleManager.vehicles) {
                    if (v != null) {
                        VehicleManager.Instance.tellVehicleDestroy(Provider.server, v.instanceID);
                        goto start;
                    }
                }
                CustomVehicleManager.customseq = 0U;
                VehicleManager.Vehicles = new List<InteractableVehicle>();
                CustomVehicleManager.custominstanceCount = 0U;
                CustomVehicleManager.customrespawnVehicleIndex = (ushort)0;
                BarricadeManager.clearPlants();
            } else {
                removedDefaultVehicles = false;
                Logger.Log("There are (" + naturalCount + "/" + defaultVehicleCount + ") natural vehicles, not removing default vehicles.");
            }
        }

        public void onLevelLoaded(int level) {
            Logger.Log("onLevelLoaded");

            Level.onLevelLoaded -= onLevelLoaded;
            Logger.Log("Receiving owned vehicles from database...");
            List<DatabaseVehicle> vehicles = AlskeboUnturnedPlugin.databaseManager.receiveOwnedVehicles();

            Logger.Log("Spawning stored vehicles...");
            foreach (DatabaseVehicle dbv in vehicles) {
                InteractableVehicle vehicle = CustomVehicleManager.customSpawnVehicle(dbv.type, new Vector3(dbv.x, dbv.y + 1.0f, dbv.z), new Quaternion(dbv.rx, dbv.ry, dbv.rz, dbv.rw));

                vehicle.tellFuel(dbv.fuel);
                VehicleManager.sendVehicleFuel(vehicle, dbv.fuel);

                vehicle.tellHealth(dbv.health);
                VehicleManager.sendVehicleHealth(vehicle, dbv.health);

                vehicle.tellLocked(new CSteamID(dbv.ownerSteamId), new CSteamID(dbv.groupSteamId), dbv.locked);

                storeOwnedVehicle(new CSteamID(dbv.ownerSteamId), new CSteamID(dbv.groupSteamId), vehicle, dbv.id);

                checkVehicleDestroy(vehicleOwners[vehicle.instanceID], vehicle);
            }

            if (!removedDefaultVehicles)
                saveVehicles(null, null);
            Logger.Log("Done.");
            loadingVehicles = false;
        }

        public void onPluginUnloaded() {
            Logger.Log("onPluginUnloaded");

        }

        public void onServerShutdown() {
            Logger.Log("onServerShutdown");

            saveVehicles(null, null, true);
            /*Logger.Log("Removing owned vehicles to prevent duplicates on next server start (" + vehicleOwners.Count + "/" + VehicleManager.Vehicles.Count + ")...");
            foreach (uint instanceId in vehicleOwners.Keys) {
                // Removes vehicle completely and destroys stuff built on it
                VehicleManager.Instance.tellVehicleDestroy(Provider.server, instanceId);
            }*/
        }

        public void onLateServerShutdown() {
            Logger.Log("onLateServerShutdown");

            String fileName = ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Level/" + Level.info.name + "/Vehicles.dat";
            if (File.Exists(fileName)) {
                File.Delete(fileName);
                Logger.Log("Deleted Vehicles.dat.");
            } else
                Logger.Log("Could not find Vehicles.dat.");
        }

        public void onPlayerConnected(UnturnedPlayer player) {
            Logger.Log("onPlayerConnected");

        }

        public void onPlayerDisconnected(UnturnedPlayer player) {
            Logger.Log("onPlayerDisconnected");

        }

        public void onPlayerEnterVehicle(UnturnedPlayer player, InteractableVehicle vehicle) {
            Logger.Log("onPlayerEnterVehicle");

            if (vehiclesToBeDestroyed.ContainsKey(vehicle.instanceID)) {
                vehiclesToBeDestroyed.Remove(vehicle.instanceID);
                UnturnedChat.Say(player, "Vehicle respawn process aborted.", vehicleManagerPrefix);
            }

            if (!vehicleOwners.ContainsKey(vehicle.instanceID)) {
                // Vehicle was spawned with /v or unturned respawned it
                long databaseId = storeOwnedVehicle(new CSteamID(0), new CSteamID(0), vehicle);
                Logger.Log("Stored naturally spawned vehicle with ID " + databaseId + ".");
            }

            CSteamID owner = getVehicleOwner(player.CurrentVehicle);
            String vehicleName = getVehicleTypeName(player.CurrentVehicle.id);
            if (owner.m_SteamID != 0) {
                VehicleInfo info = getOwnedVehicleInfo(player.CurrentVehicle.instanceID);
                String ownerName = (info != null ? info.ownerName : "Unknown player");

                if (player.CSteamID.Equals(owner))
                    UnturnedChat.Say(player, "Welcome back to your " + vehicleName + ", " + player.DisplayName + "!", vehicleManagerPrefix);
                else if (info != null && info.hasGroup && player.SteamGroupID.Equals(info.groupId)) {
                    UnturnedChat.Say(player, "Welcome back to " + ownerName + "'s " + vehicleName + ", " + player.DisplayName + "!", vehicleManagerPrefix);
                } else {
                    UnturnedChat.Say(player, "This " + vehicleName + " belongs to " + ownerName + ".", vehicleManagerPrefix);
                    UnturnedChat.Say(owner, "Your " + vehicle.asset.Name + " was stolen!");
                }
            } else
                UnturnedChat.Say(player, "This natural " + vehicleName + " will despawn when inactive with low fuel.", vehicleManagerPrefix);

        }

        public void onPlayerExitVehicle(UnturnedPlayer player, InteractableVehicle vehicle) {
            Logger.Log("onPlayerExitVehicle");

            if (vehicle != null) {
                VehicleInfo info = getOwnedVehicleInfo(vehicle.instanceID);
                if (info != null) {
                    if (checkVehicleDestroy(info, vehicle))
                        UnturnedChat.Say(player, "Be careful! This vehicle will despawn in exactly " + vehicleDestroyMinutes + " minutes. Abort the process by fueling it and/or entering it.", vehicleManagerPrefix);

                    if (!lastSave.ContainsKey(vehicle.instanceID) || !isSimilar(vehicle, lastSave[vehicle.instanceID])) {
                        DatabaseVehicle dbv = DatabaseVehicle.fromInteractableVehicle(info.databaseId, info.ownerId.m_SteamID, info.groupId.m_SteamID, vehicle);
                        AlskeboUnturnedPlugin.databaseManager.updateVehicle(dbv);
                        AlskeboUnturnedPlugin.databaseManager.setVehicleLastActivity(dbv.id);
                        if (lastSave.ContainsKey(vehicle.instanceID))
                            lastSave[vehicle.instanceID] = dbv;
                        else
                            lastSave.Add(vehicle.instanceID, dbv);
                    }
                }
            }
        }

        private void destroyVehicles(object sender, ElapsedEventArgs e) {
            List<uint> toRemove = new List<uint>();
            foreach (KeyValuePair<uint, DestroyingVehicleInfo> pair in vehiclesToBeDestroyed) {
                if (!pair.Value.vehicle.isDead && (DateTime.Now - pair.Value.lastActivity).Minutes >= vehicleDestroyMinutes) {
                    Logger.Log("destroying");

                    InteractableVehicle vehicle = pair.Value.vehicle;
                    if (pair.Value.timesHonked >= 10) {
                        pair.Value.vehicle.askDamage(ushort.MaxValue, false);
                        toRemove.Add(pair.Value.vehicle.instanceID);
                    } else if (!pair.Value.lastHonked) {
                        CustomVehicleManager.sendVehicleHeadlights(vehicle);
                        EffectManager.sendEffect(vehicleDestroySounds[r.Next(vehicleDestroySounds.Count - 1)], 200, vehicle.transform.position);
                        pair.Value.lastHonked = true;
                        ++pair.Value.timesHonked;
                    } else {
                        pair.Value.lastHonked = false;
                    }

                } else if (pair.Value.vehicle.isDead) {
                    toRemove.Add(pair.Value.vehicle.instanceID);
                }
            }
            foreach (uint r in toRemove) {
                vehiclesToBeDestroyed.Remove(r);
            }
        }

        private bool checkVehicleDestroy(VehicleInfo info, InteractableVehicle vehicle) {
            Logger.Log("checkVehicleDestroy");

            if (info.isNatural && vehicle.isEmpty && getFuelPercentage(vehicle) <= vehicleDestroyFuel) {
                if (!vehiclesToBeDestroyed.ContainsKey(vehicle.instanceID)) {
                    DestroyingVehicleInfo destroyingInfo = new DestroyingVehicleInfo();
                    destroyingInfo.vehicle = vehicle;
                    destroyingInfo.lastActivity = DateTime.Now;
                    vehiclesToBeDestroyed.Add(vehicle.instanceID, destroyingInfo);
                }
                return true;
            }
            return false;
        }

        private void saveVehicles(object sender, ElapsedEventArgs e) {
            Logger.Log("saveVehicles");

            saveVehicles(sender, e, false);
        }

        private void saveVehicles(object sender, ElapsedEventArgs e, bool overrid3 = false) {
            Logger.Log("saveVehicles 2");

            foreach (InteractableVehicle vehicle in VehicleManager.Vehicles) {
                if (!vehicleOwners.ContainsKey(vehicle.instanceID) && !vehicle.isExploded && !vehicle.isDrowned && !vehicle.isDead) {
                    // Vehicle was spawned with /v or unturned respawned it
                    long databaseId = storeOwnedVehicle(new CSteamID(0), new CSteamID(0), vehicle);
                    Logger.Log("Stored naturally spawned vehicle with ID " + databaseId + " and instanceID " + vehicle.instanceID + ".");
                }
            }

            foreach (KeyValuePair<uint, VehicleInfo> pair in vehicleOwners) {
                InteractableVehicle vehicle = VehicleManager.getVehicle(pair.Key);

                if (vehicle == null || vehicle.isExploded || vehicle.isDrowned) {
                    deleteOwnedVehicle(pair.Value.databaseId, pair.Value.instanceId);
                    Logger.Log("Deleted exploded/drowned/dead vehicle with ID " + pair.Value.databaseId + " and instanceID " + pair.Value.instanceId + (vehicle == null ? " (null)" : "") + ".");
                } else if (!lastSave.ContainsKey(pair.Key) || !isSimilar(vehicle, lastSave[pair.Key]) || overrid3) {
                    DatabaseVehicle dbv = DatabaseVehicle.fromInteractableVehicle(pair.Value.databaseId, pair.Value.ownerId.m_SteamID, pair.Value.groupId.m_SteamID, vehicle);
                    AlskeboUnturnedPlugin.databaseManager.updateVehicle(dbv);
                    if (lastSave.ContainsKey(vehicle.instanceID))
                        lastSave[vehicle.instanceID] = dbv;
                    else
                        lastSave.Add(vehicle.instanceID, dbv);
                }
            }
        }

        private bool isSimilar(InteractableVehicle first, DatabaseVehicle second) {
            if (first.transform.position.x == second.x && first.transform.position.y == second.y && first.transform.position.z == second.z
                && first.transform.rotation.x == second.rx && first.transform.rotation.y == second.ry && first.transform.rotation.z == second.rz && first.transform.rotation.w == second.rw) {
                if (first.fuel == second.fuel && first.health == second.health && first.isLocked == second.locked && first.lockedGroup.m_SteamID == second.groupSteamId)
                    return true;
            }
            return false;
        }

        public InteractableVehicle givePlayerOwnedVehicle(UnturnedPlayer player, ushort carId) {
            Logger.Log("givePlayerOwnedVehicle");

            InteractableVehicle vehicle = giveVehicle(player, carId);
            if (vehicle == null) {
                UnturnedChat.Say(player, "Could not give you a vehicle.", vehicleManagerPrefix);
                return null;
            }

            storeOwnedVehicle(player.CSteamID, player.SteamGroupID, vehicle, -1);
            return vehicle;
        }

        public CSteamID getVehicleOwner(InteractableVehicle vehicle) {
            Logger.Log("getVehicleOwner");

            if (vehicleOwners.ContainsKey(vehicle.instanceID))
                return vehicleOwners[vehicle.instanceID].ownerId;
            return new CSteamID(0);
        }

        public CSteamID getVehicleGroup(InteractableVehicle vehicle) {
            Logger.Log("getVehicleGroup");

            if (vehicleOwners.ContainsKey(vehicle.instanceID))
                return vehicleOwners[vehicle.instanceID].groupId;
            return new CSteamID(0);
        }

        public List<VehicleInfo> getOwnedVehicles(CSteamID id) {
            Logger.Log("getOwnedVehicles");

            if (playerOwnedVehicles.ContainsKey(id))
                return playerOwnedVehicles[id];
            return new List<VehicleInfo>();
        }

        public List<VehicleInfo> getAllVehicles(CSteamID ownerId, CSteamID groupId) {
            Logger.Log("getAllVehicles");

            List<VehicleInfo> output = new List<VehicleInfo>();
            foreach (VehicleInfo info in vehicleOwners.Values) {
                if (ownerId != null && !info.isNatural && info.ownerId.m_SteamID == ownerId.m_SteamID)
                    output.Add(info);
                if (groupId != null && info.hasGroup && info.groupId.m_SteamID == groupId.m_SteamID)
                    output.Add(info);
            }
            return output;
        }

        public VehicleInfo getOwnedVehicleInfo(uint instanceId) {
            Logger.Log("getOwnedVehicleInfo");

            if (vehicleOwners.ContainsKey(instanceId))
                return vehicleOwners[instanceId];
            return null;
        }

        private long storeOwnedVehicle(CSteamID ownerId, CSteamID groupId, InteractableVehicle vehicle, long databaseId = -1) {
            Logger.Log("storeOwnedVehicle");

            vehicle.onLockUpdated += new VehicleLockUpdated(delegate () { onLockUpdated(vehicle); });

            if (databaseId == -1)
                databaseId = AlskeboUnturnedPlugin.databaseManager.insertOwnedVehicle(ownerId, groupId, vehicle);
            if (!playerOwnedVehicles.ContainsKey(ownerId))
                playerOwnedVehicles.Add(ownerId, new List<VehicleInfo>());

            List<VehicleInfo> list = playerOwnedVehicles[ownerId];
            VehicleInfo info = new VehicleInfo();
            info.instanceId = vehicle.instanceID;
            info.databaseId = databaseId;
            info.ownerId = ownerId;
            info.groupId = groupId;
            String ownerNameTemp = AlskeboUnturnedPlugin.databaseManager.getPlayerDisplayName(ownerId);
            info.ownerName = (ownerNameTemp != null ? ownerNameTemp : "Unknown player");
            if (ownerId.m_SteamID == 0)
                info.ownerName = "Nature";
            info.isLocked = vehicle.isLocked;
            list.Add(info);
            playerOwnedVehicles[ownerId] = list;
            vehicleOwners.Add(vehicle.instanceID, info);
            return databaseId;
        }

        public void deleteOwnedVehicle(long databaseId, uint instanceId) {
            Logger.Log("deleteOwnedVehicle");

            AlskeboUnturnedPlugin.databaseManager.deleteVehicle(databaseId);

            if (vehicleOwners.ContainsKey(instanceId)) {
                VehicleInfo info = vehicleOwners[instanceId];
                if (playerOwnedVehicles.ContainsKey(info.ownerId)) {
                    List<VehicleInfo> infos = playerOwnedVehicles[info.ownerId];
                    if (!infos.Remove(info))
                        Logger.LogWarning("Could not remove from playerOwnedVehicles");
                    else
                        playerOwnedVehicles[info.ownerId] = infos;
                }
                if (!vehicleOwners.Remove(instanceId)) {
                    Logger.LogWarning("Could not remove from vehicleOwners");
                }
            }

            if (vehiclesToBeDestroyed.ContainsKey(instanceId))
                vehiclesToBeDestroyed.Remove(instanceId);

            if (lastSave.ContainsKey(instanceId))
                lastSave.Remove(instanceId);
        }

        public String getVehicleTypeName(ushort id) {
            Logger.Log("getVehicleTypeName");

            Asset a = SDG.Unturned.Assets.find(EAssetType.VEHICLE, id);
            if (a != null)
                return ((VehicleAsset)a).Name;
            return "Unknown vehicle";
        }

        private static InteractableVehicle giveVehicle(UnturnedPlayer player, ushort id) {
            Logger.Log("giveVehicle");

            Vector3 point = player.Player.transform.position + player.Player.transform.forward * 6f;
            RaycastHit hitInfo;
            Physics.Raycast(point + Vector3.up * 16f, Vector3.down, out hitInfo, 32f, RayMasks.BLOCK_VEHICLE);
            if ((UnityEngine.Object)hitInfo.collider != (UnityEngine.Object)null)
                point.y = hitInfo.point.y + 16f;
            InteractableVehicle vehicle = CustomVehicleManager.customSpawnVehicle(id, point, player.Player.transform.rotation);
            return vehicle;
        }

        private void onLockUpdated(InteractableVehicle vehicle) {
            Logger.Log("onLockUpdated");

            if (loadingVehicles)
                return;

            if (vehicle != null && vehicleOwners.ContainsKey(vehicle.instanceID)) {
                VehicleInfo info = vehicleOwners[vehicle.instanceID];
                // If the states are correct, nothing should be done
                if (info.isLocked == vehicle.isLocked) {
                    return;
                } else if (vehicle.passengers != null && vehicle.passengers.Length > 0) {
                    Passenger driver = vehicle.passengers[0];
                    if (driver != null && driver.player != null) {
                        CSteamID driverId = driver.player.playerID.CSteamID;
                        CSteamID driverGroupId = driver.player.playerID.SteamGroupID;

                        if (driverId.Equals(info.ownerId)) {
                            storeVehicleLocked(vehicle);
                            UnturnedChat.Say(driverId, "This vehicle is now " + (vehicle.isLocked ? "locked" : "unlocked") + ".", vehicleManagerPrefix);
                        } else if (info.hasGroup && driverGroupId.Equals(info.groupId)) {
                            storeVehicleLocked(vehicle);
                            UnturnedChat.Say(driverId, "This vehicle is now " + (vehicle.isLocked ? "locked" : "unlocked") + ".", vehicleManagerPrefix);
                        } else {
                            CustomVehicleManager.sendForceVehicleLock(vehicle, info.ownerId, info.groupId, info.isLocked);
                            UnturnedChat.Say(driverId, "You do not have the keys for this vehicle.", vehicleManagerPrefix);
                        }
                        return;
                    }
                }

                storeVehicleLocked(vehicle);
                UnturnedChat.Say("A " + vehicle.asset.Name + " was lockpicked!");
            }
        }

        private void storeVehicleLocked(InteractableVehicle vehicle) {
            Logger.Log("storeVehicleLocked");

            storeVehicleLocked(vehicle, vehicle.isLocked);
        }

        private void storeVehicleLocked(InteractableVehicle vehicle, bool locked) {
            Logger.Log("storeVehicleLocked 2");

            if (vehicleOwners.ContainsKey(vehicle.instanceID)) {
                VehicleInfo info = vehicleOwners[vehicle.instanceID];
                info.isLocked = locked;
                vehicleOwners[vehicle.instanceID] = info;

            } else
                Logger.LogWarning("Could not store lock state");
        }

        public void setOwnedVehicleLocked(InteractableVehicle vehicle, bool locked) {
            Logger.Log("setOwnedVehicleLocked");

            if (vehicleOwners.ContainsKey(vehicle.instanceID)) {
                VehicleInfo info = vehicleOwners[vehicle.instanceID];
                storeVehicleLocked(vehicle, locked);
                vehicle.tellLocked(info.ownerId, info.groupId, locked);
                CustomVehicleManager.sendForceVehicleLock(vehicle, info.ownerId, info.groupId, locked);
            } else
                Logger.LogWarning("Could not lock non-owned vehicle");
        }

        public int getFuelPercentage(InteractableVehicle vehicle) {
            Logger.Log("getFuelPercentage");

            return (int)Math.Floor(((float)vehicle.fuel / (float)vehicle.asset.fuel) * 100);
        }
    }
}
