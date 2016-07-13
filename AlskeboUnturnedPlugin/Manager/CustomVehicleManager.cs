﻿using Rocket.Core.Logging;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

using System.Reflection;
using System.Text;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class CustomVehicleManager {
        public static byte customSAVEDATA_VERSION { get { return VehicleManager.SAVEDATA_VERSION; } }
        public static VehicleManager customInstance { get { return VehicleManager.Instance; } }
        public static List<InteractableVehicle> customVehicles { get { return VehicleManager.vehicles; } }

        public static uint custominstanceCount {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("instanceCount", BindingFlags.NonPublic | BindingFlags.Static);
                uint value = (uint)info.GetValue(null);
                return value;
            }
            set {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("instanceCount", BindingFlags.NonPublic | BindingFlags.Static);
                info.SetValue(null, value);
            }
        }

        public static ushort customrespawnVehicleIndex {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("respawnVehicleIndex", BindingFlags.NonPublic | BindingFlags.Static);
                ushort value = (ushort)info.GetValue(null);
                return value;
            }
            set {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("respawnVehicleIndex", BindingFlags.NonPublic | BindingFlags.Static);
                info.SetValue(null, value);
            }
        }

        public static float customlastTick {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("lastTick", BindingFlags.NonPublic | BindingFlags.Static);
                float value = (float)info.GetValue(null);
                return value;
            }
        }

        public static uint customseq {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("seq", BindingFlags.NonPublic | BindingFlags.Instance);
                uint value = (uint)info.GetValue(VehicleManager.Instance);
                return value;
            }
            set {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("seq", BindingFlags.NonPublic | BindingFlags.Instance);
                info.SetValue(CustomVehicleManager.customInstance, value);
            }
        }

        public static InteractableVehicle customSpawnVehicle(ushort id, Vector3 point, Quaternion angle) {
            VehicleAsset vehicleAsset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
            if (vehicleAsset == null)
                return null;
            InteractableVehicle vehicle = customAddVehicle(id, point, angle, false, false, false, vehicleAsset.fuel, false, vehicleAsset.health, (CSteamID[])null, (byte[][])null, ++custominstanceCount);
            VehicleManager.Instance.channel.openWrite();
            VehicleManager.Instance.sendVehicle(VehicleManager.vehicles[VehicleManager.vehicles.Count - 1]);
            VehicleManager.Instance.channel.closeWrite("tellVehicle", ESteamCall.OTHERS, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
            BarricadeManager.askPlants(VehicleManager.vehicles[VehicleManager.vehicles.Count - 1].transform);
            return vehicle;
        }

        private static InteractableVehicle customAddVehicle(ushort id, Vector3 point, Quaternion angle, bool sirens, bool headlights, bool taillights, ushort fuel, bool isExploded, ushort health, CSteamID[] passengers, byte[][] turrets, uint instanceID) {
            VehicleAsset vehicleAsset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
            if (vehicleAsset != null) {
                Transform parent = !Dedicator.isDedicated || !((UnityEngine.Object)vehicleAsset.clip != (UnityEngine.Object)null) ? UnityEngine.Object.Instantiate<GameObject>(vehicleAsset.vehicle).transform : UnityEngine.Object.Instantiate<GameObject>(vehicleAsset.clip).transform;
                parent.name = id.ToString();
                parent.parent = LevelVehicles.models;
                parent.position = point;
                parent.rotation = angle;
                parent.GetComponent<Rigidbody>().useGravity = true;
                parent.GetComponent<Rigidbody>().isKinematic = false;
                InteractableVehicle interactableVehicle = parent.gameObject.AddComponent<InteractableVehicle>();
                interactableVehicle.instanceID = instanceID;
                interactableVehicle.id = id;
                interactableVehicle.fuel = fuel;
                interactableVehicle.isExploded = isExploded;
                interactableVehicle.health = health;
                interactableVehicle.init();
                interactableVehicle.tellSirens(sirens);
                interactableVehicle.tellHeadlights(headlights);
                interactableVehicle.tellTaillights(taillights);
                if (Provider.isServer) {
                    if (turrets != null) {
                        for (byte index = (byte)0; (int)index < interactableVehicle.turrets.Length; ++index)
                            interactableVehicle.turrets[(int)index].state = turrets[(int)index];
                    } else {
                        for (byte index = (byte)0; (int)index < interactableVehicle.turrets.Length; ++index) {
                            ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, vehicleAsset.turrets[(int)index].itemID);
                            interactableVehicle.turrets[(int)index].state = itemAsset == null ? (byte[])null : itemAsset.getState();
                        }
                    }
                }
                if (passengers != null) {
                    for (byte seat = (byte)0; (int)seat < passengers.Length; ++seat) {
                        if (passengers[(int)seat] != CSteamID.Nil)
                            interactableVehicle.addPlayer(seat, passengers[(int)seat]);
                    }
                }
                VehicleManager.vehicles.Add(interactableVehicle);
                BarricadeManager.waterPlant(parent);
                return interactableVehicle;
            } else {
                Logger.LogError("customAddVehicle: Could not find asset with id " + id);
                if (Provider.isServer)
                    return null;
                Provider.connectionFailureInfo = ESteamConnectionFailureInfo.VEHICLE;
                Provider.disconnect();
            }
            return null;
        }

        public static void sendForceVehicleLock(InteractableVehicle vehicle, CSteamID owner, CSteamID group, bool locked) {
            SteamChannel channel = VehicleManager.Instance.channel;
            object[] objArray = new object[4];
            objArray[0] = vehicle.instanceID;
            objArray[1] = owner;
            objArray[2] = group;
            objArray[3] = locked;
            channel.send("tellVehicleLock", (ESteamCall)1, (ESteamPacket)15, objArray);
        }

        public static void sendVehicleHeadlights(InteractableVehicle vehicle) {
            bool newHeadlights = !vehicle.headlightsOn;
            vehicle.tellHeadlights(newHeadlights);
            SteamChannel channel = VehicleManager.Instance.channel;
            string name = "tellVehicleHeadlights";
            object[] objArray = new object[2];
            objArray[0] = vehicle.instanceID;
            objArray[1] = (newHeadlights ? 1 : 0);
            channel.send(name, (ESteamCall)1, (ESteamPacket)15, objArray);
        }

        public static void sendVehicleHeadlights(InteractableVehicle vehicle, bool on) {
            vehicle.tellHeadlights(on);
            SteamChannel channel = VehicleManager.Instance.channel;
            string name = "tellVehicleHeadlights";
            object[] objArray = new object[2];
            objArray[0] = vehicle.instanceID;
            objArray[1] = (on ? 1 : 0);
            channel.send(name, (ESteamCall)1, (ESteamPacket)15, objArray);
        }
    }
}
