using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Rocket.API;
using System.Threading;

namespace AlskeboUnturnedPlugin {
    public class AlskeboUnturnedPlugin : RocketPlugin {
        class PlayerData {
            public bool isDriving = false;
            public InteractableVehicle vehicle = null;
        }

        private Dictionary<string, PlayerData> playerDataMap = new Dictionary<string, PlayerData>();
        private bool wasUnloaded = false;
        public static DatabaseManager databaseManager;
        public static AlskeboVehicleManager vehicleManager;
        public static AlskeboPlayerManager playerManager;
        public static Advertiser advertiser;
        public static System.Random r = new System.Random();

        public override void LoadPlugin() {
            base.LoadPlugin();
            databaseManager = new DatabaseManager();
            vehicleManager = new AlskeboVehicleManager();
            playerManager = new AlskeboPlayerManager();
            advertiser = new Advertiser();
            U.Events.OnPlayerConnected += onPlayerConnected;
            U.Events.OnPlayerDisconnected += onPlayerDisconnected;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateGesture += onPlayerUpdateGesture;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdatePosition += onPlayerUpdatePosition;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStance += onPlayerUpdateStance;
            if (wasUnloaded) {
                Logger.LogWarning("\tRe-sending onPlayerConnected calls");
                foreach (SteamPlayer player in Provider.clients) {
                    onPlayerConnected(UnturnedPlayer.FromSteamPlayer(player));
                }
            }
            wasUnloaded = false;
            Logger.LogWarning("\tAlskeboPlugin Loaded Sucessfully");
        }

        public override void UnloadPlugin(PluginState state = PluginState.Unloaded) {
            base.UnloadPlugin(state);
            U.Events.OnPlayerConnected -= onPlayerConnected;
            U.Events.OnPlayerDisconnected -= onPlayerDisconnected;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateGesture -= onPlayerUpdateGesture;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdatePosition -= onPlayerUpdatePosition;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStance -= onPlayerUpdateStance;
            wasUnloaded = true;
            Logger.LogWarning("\tAlskeboPlugin Unloaded");
        }

        private void onPlayerConnected(UnturnedPlayer player) {
            new Thread(delegate () {
                if (!databaseManager.playerExists(player.CSteamID))
                    databaseManager.insertPlayer(player.CSteamID, player.DisplayName, false);
            }).Start();
            playerManager.onPlayerConnected(player);
            UnturnedChat.Say(player.DisplayName + " has connected.");
        }

        private void onPlayerDisconnected(UnturnedPlayer player) {
            playerManager.onPlayerDisconnected(player);
            UnturnedChat.Say(player.DisplayName + " has disconnected.");
        }

        private void onPlayerUpdateGesture(UnturnedPlayer player, Rocket.Unturned.Events.UnturnedPlayerEvents.PlayerGesture gesture) {
            if (gesture == Rocket.Unturned.Events.UnturnedPlayerEvents.PlayerGesture.PunchLeft || gesture == Rocket.Unturned.Events.UnturnedPlayerEvents.PlayerGesture.PunchRight) {
                if (playerManager.getPlayerData(player, "structureinfo")) {
                    List<RegionCoordinate> regions = new List<RegionCoordinate>();
                    Regions.getRegionsInRadius(player.Position, 20, regions);
                    List<Transform> structures = new List<Transform>();
                    StructureManager.getStructuresInRadius(player.Position, 10, regions, structures);

                    float lowestDistance = float.MaxValue;
                    Transform lowestDistanceTransform = null;
                    foreach (Transform transform in structures) {
                        float dis = Vector3.Distance(transform.position, player.Position);
                        if (dis < lowestDistance) {
                            lowestDistance = dis;
                            lowestDistanceTransform = transform;
                        }
                    }

                    if (lowestDistanceTransform != null) {
                        byte x;
                        byte y;
                        ushort index;
                        StructureRegion region;
                        if (StructureManager.tryGetInfo(lowestDistanceTransform, out x, out y, out index, out region)) {
                            Structure structure = region.structures[(int)index].structure;
                            ItemStructureAsset itemStructureAsset = (ItemStructureAsset)Assets.find(EAssetType.ITEM, structure.id);
                            UnturnedChat.Say(player, "-");
                            UnturnedChat.Say(player, "Structure status of " + itemStructureAsset.itemName + ":");
                            UnturnedChat.Say(player, "Health: " + structure.health);
                            UnturnedChat.Say(player, "Is dead: " + structure.isDead);
                        }
                    } else
                        UnturnedChat.Say(player, "Could not find any close structures.");
                } else if (playerManager.getPlayerData(player, "barricadeinfo")) {
                    List<RegionCoordinate> regions = new List<RegionCoordinate>();
                    Regions.getRegionsInRadius(player.Position, 20, regions);
                    List<Transform> barricades = new List<Transform>();
                    BarricadeManager.getBarricadesInRadius(player.Position, 10, regions, barricades);

                    float lowestDistance = float.MaxValue;
                    Transform lowestDistanceTransform = null;
                    foreach (Transform transform in barricades) {
                        float dis = Vector3.Distance(transform.position, player.Position);
                        if (dis < lowestDistance) {
                            lowestDistance = dis;
                            lowestDistanceTransform = transform;
                        }
                    }

                    if (lowestDistanceTransform != null) {
                        byte x;
                        byte y;
                        ushort plant;
                        ushort index;
                        BarricadeRegion region;
                        if (BarricadeManager.tryGetInfo(lowestDistanceTransform, out x, out y, out plant, out index, out region)) {
                            Barricade barricade = region.barricades[(int)index].barricade;
                            ItemBarricadeAsset itemBarricadeAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, barricade.id);
                            UnturnedChat.Say(player, "-");
                            UnturnedChat.Say(player, "Barricade status of " + itemBarricadeAsset.itemName + ":");
                            UnturnedChat.Say(player, "Health: " + barricade.health);
                            UnturnedChat.Say(player, "Plant: " + plant);
                        }
                    } else
                        UnturnedChat.Say(player, "Could not find any close barricades.");
                }
            }
        }

        private void onPlayerUpdatePosition(UnturnedPlayer player, UnityEngine.Vector3 position) {
        }

        private void onPlayerUpdateStance(UnturnedPlayer player, byte stance) {
            if (!playerDataMap.ContainsKey(player.Id) || playerDataMap[player.Id] == null)
                playerDataMap.Add(player.Id, new PlayerData());

            PlayerData playerData = playerDataMap[player.Id];

            if (stance == 6 && !playerData.isDriving) {
                // Player entered vehicle

                playerData.isDriving = true;
                playerData.vehicle = player.CurrentVehicle;
                playerDataMap[player.Id] = playerData;

                if (player.CurrentVehicle != null) {
                    CSteamID who = vehicleManager.getVehicleOwner(player.CurrentVehicle);
                    String vehicleName = vehicleManager.getVehicleTypeName(player.CurrentVehicle.id);
                    if (who != CSteamID.Nil) {
                        VehicleInfo info = vehicleManager.getOwnedVehicleInfoByInstanceId(who, player.CurrentVehicle.instanceID);
                        String whoNickname = (info != null ? info.ownerName : "Unknown player");

                        if (player.CSteamID.Equals(who))
                            UnturnedChat.Say(player, "Welcome back to your " + vehicleName + ", " + player.DisplayName + "!");
                        else
                            UnturnedChat.Say(player, "This " + vehicleName + " belongs to " + whoNickname + ".");
                    } else
                        UnturnedChat.Say(player, "This natural " + vehicleName + " will despawn when its fuel level is low.");

                    vehicleManager.onPlayerEnterVehicle(player, player.CurrentVehicle);
                }
            } else if (stance != 6 && playerData.isDriving) {
                // Player exited vehicle

                playerData.isDriving = false;
                playerDataMap[player.Id] = playerData;

                vehicleManager.onPlayerExitVehicle(player, playerData.vehicle);
            }
        }

    }
}
