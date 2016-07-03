using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

using System.Text;
using UnityEngine;
using Rocket.API;
using System.Threading;
using Rocket.API.Extensions;

namespace AlskeboUnturnedPlugin {
    public class AlskeboUnturnedPlugin : RocketPlugin<AlskeboConfiguration> {
        class PlayerData {
            public bool isInsideVehicle = false;
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

            databaseManager = new DatabaseManager(Configuration.Instance);
            vehicleManager = new AlskeboVehicleManager();
            playerManager = new AlskeboPlayerManager();
            advertiser = new Advertiser();
            U.Events.OnPlayerConnected += onPlayerConnected;
            U.Events.OnPlayerDisconnected += onPlayerDisconnected;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateGesture += onPlayerUpdateGesture;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdatePosition += onPlayerUpdatePosition;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStance += onPlayerUpdateStance;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStat += onPlayerUpdateStat;

            vehicleManager.onPluginLoaded();

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
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStat -= onPlayerUpdateStat;

            vehicleManager.onPluginUnloaded();

            wasUnloaded = true;
            Logger.LogWarning("\tAlskeboPlugin Unloaded");
        }

        private void onPlayerConnected(UnturnedPlayer player) {
            UnturnedChat.Say(player.DisplayName + " has connected.");

            playerManager.onPlayerConnected(player);

            new Thread(delegate () {
                if (!databaseManager.playerExists(player.CSteamID))
                    databaseManager.insertPlayer(player.CSteamID, player.DisplayName, false);
                else
                    databaseManager.setPlayerLastJoin(player.CSteamID);

                DatabasePlayer dbp = AlskeboUnturnedPlugin.databaseManager.receivePlayer(player.CSteamID);
                playerManager.setPlayerData(player, "balance", dbp.balance);
                playerManager.setPlayerData(player, "receivedvehicle", dbp.receivedVehicle);
                if (!dbp.receivedVehicle)
                    UnturnedChat.Say(player, "Receive your one-time free personal car with \"/firstvehicle\"!");
            }).Start();

            vehicleManager.onPlayerConnected(player);
        }

        private void onPlayerDisconnected(UnturnedPlayer player) {
            playerManager.onPlayerDisconnected(player);
            vehicleManager.onPlayerDisconnected(player);
            UnturnedChat.Say(player.DisplayName + " has disconnected.");
        }

        private void onPlayerUpdateGesture(UnturnedPlayer player, Rocket.Unturned.Events.UnturnedPlayerEvents.PlayerGesture gesture) {
            if (gesture == Rocket.Unturned.Events.UnturnedPlayerEvents.PlayerGesture.PunchLeft || gesture == Rocket.Unturned.Events.UnturnedPlayerEvents.PlayerGesture.PunchRight) {
                if (playerManager.getPlayerBoolean(player, "structureinfo")) {
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
                } else if (playerManager.getPlayerBoolean(player, "barricadeinfo")) {
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

            if ((stance == 6 || stance == 7) && !playerData.isInsideVehicle) {
                playerData.isInsideVehicle = true;
                playerData.vehicle = player.CurrentVehicle;
                playerDataMap[player.Id] = playerData;

                vehicleManager.onPlayerEnterVehicle(player, player.CurrentVehicle);
            } else if (stance != 6 && stance != 7 && playerData.isInsideVehicle) {
                playerData.isInsideVehicle = false;
                playerDataMap[player.Id] = playerData;

                vehicleManager.onPlayerExitVehicle(player, playerData.vehicle);
            }
        }

        private void onPlayerUpdateStat(UnturnedPlayer player, EPlayerStat stat) {
            if (stat == EPlayerStat.KILLS_ZOMBIES_MEGA)
                UnturnedChat.Say("Rumours say " + player.DisplayName + " has killed a mega zombie...", Color.blue);
        }

        public static LocationNode getClosestLocation(Vector3 pos) {
            LocationNode closest = null;
            float closestDistance = float.MaxValue;
            foreach (Node node in LevelNodes.Nodes) {
                if (node.type == ENodeType.LOCATION) {
                    float dist = Vector3.Distance(node.Position, pos);
                    if (closest == null || dist < closestDistance) {
                        closest = (LocationNode)node;
                        closestDistance = dist;
                    }
                }
            }
            return closest;
        }

    }
}
