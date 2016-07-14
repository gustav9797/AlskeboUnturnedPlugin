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
        public static AdvertisingManager advertiser;
        public static VehicleShop vehicleShop;
        public static Lottery lottery;
        public static System.Random r = new System.Random();

        public override void LoadPlugin() {
            base.LoadPlugin();

            databaseManager = new DatabaseManager(Configuration.Instance);
            vehicleManager = new AlskeboVehicleManager();
            playerManager = new AlskeboPlayerManager();
            advertiser = new AdvertisingManager();
            vehicleShop = new VehicleShop();
            lottery = new Lottery();
            U.Events.OnPlayerConnected += onPlayerConnected;
            U.Events.OnPlayerDisconnected += onPlayerDisconnected;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateGesture += onPlayerUpdateGesture;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStance += onPlayerUpdateStance;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStat += onPlayerUpdateStat;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerChatted += onPlayerChatted;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath += onPlayerDeath;

            vehicleManager.onPluginLoaded();

            if (wasUnloaded) {
                Logger.LogWarning("\tRe-sending onPlayerConnected calls");
                foreach (SteamPlayer player in Provider.clients) {
                    onPlayerConnected(UnturnedPlayer.FromSteamPlayer(player));
                }
            }
            wasUnloaded = false;

            Level.onLevelLoaded += onLevelLoaded;

            Logger.LogWarning("\tAlskeboPlugin Loaded Sucessfully");
        }

        private void onPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer) {
            String limbName = "";
            switch (limb) {
                case ELimb.LEFT_ARM:
                    limbName = "left arm";
                    break;
                case ELimb.LEFT_BACK:
                case ELimb.LEFT_FRONT:
                case ELimb.RIGHT_BACK:
                case ELimb.RIGHT_FRONT:
                case ELimb.SPINE:
                    limbName = "torso";
                    break;
                case ELimb.LEFT_FOOT:
                    limbName = "left foot";
                    break;
                case ELimb.LEFT_HAND:
                    limbName = "left hand";
                    break;
                case ELimb.LEFT_LEG:
                    limbName = "left leg";
                    break;
                case ELimb.RIGHT_ARM:
                    limbName = "right arm";
                    break;
                case ELimb.RIGHT_FOOT:
                    limbName = "right foot";
                    break;
                case ELimb.RIGHT_HAND:
                    limbName = "right hand";
                    break;
                case ELimb.RIGHT_LEG:
                    limbName = "right leg";
                    break;
                case ELimb.SKULL:
                    limbName = "head";
                    break;
            }

            String msg = "";
            switch (cause) {
                case EDeathCause.ACID:
                    msg = "was poisoned to death";
                    break;
                case EDeathCause.ANIMAL:
                    msg = "was eaten by an angry animal";
                    break;
                case EDeathCause.ARENA:
                    msg = "was eaten by an arena";
                    break;
                case EDeathCause.BLEEDING:
                    msg = "forgot how to use a dressing";
                    break;
                case EDeathCause.BONES:
                    msg = "hit the ground really hard";
                    break;
                case EDeathCause.BREATH:
                    msg = "ran out of air";
                    break;
                case EDeathCause.BURNING:
                    msg = "went up in flames";
                    break;
                case EDeathCause.FOOD:
                    msg = "didn't eat properly";
                    break;
                case EDeathCause.FREEZING:
                    msg = "didn't craft a campfire in time";
                    break;
                case EDeathCause.INFECTION:
                    msg = "caught a serious infection";
                    break;
                case EDeathCause.LANDMINE:
                    msg = "walked on a landmine";
                    break;
                case EDeathCause.MISSILE:
                    msg = "was shot by a missile";
                    break;
                case EDeathCause.SENTRY:
                    msg = "was shot by a sentry";
                    break;
                case EDeathCause.SHRED:
                    msg = "was shredded";
                    break;
                case EDeathCause.SPLASH:
                    msg = "was spashed to death";
                    break;
                case EDeathCause.SUICIDE:
                    msg = "didn't want to live anymore";
                    break;
                case EDeathCause.WATER:
                    msg = "was really thirsty";
                    break;
                case EDeathCause.ZOMBIE:
                    msg = "was eaten up by a zombie";
                    break;
                case EDeathCause.CHARGE:
                    msg = "was blown up by a charge";
                    break;
                case EDeathCause.GRENADE:
                    msg = "was blown up by a grenade";
                    break;
                case EDeathCause.GUN:
                    msg = "was shot" + (limbName != "" ? " in the " + limbName : "");
                    break;
                case EDeathCause.KILL:
                    msg = "was killed";
                    break;
                case EDeathCause.PUNCH:
                    msg = "got punched" + (limbName != "" ? " in the " + limbName : " to death");
                    break;
                case EDeathCause.MELEE:
                    msg = "was stabbed" + (limbName != "" ? " in the " + limbName : " to death");
                    break;
                case EDeathCause.VEHICLE:
                case EDeathCause.ROADKILL:
                    msg = "was run over";
                    break;
            }
            if (msg == "")
                msg = "was killed";

            if (murderer != null && murderer != CSteamID.Nil && murderer != Provider.server && murderer != player.CSteamID) {
                UnturnedPlayer murdererPlayer = UnturnedPlayer.FromCSteamID(murderer);
                if (murdererPlayer != null)
                    msg += " by " + murdererPlayer.DisplayName;
            }
            msg += ".";
            UnturnedChat.Say(player.DisplayName + " " + msg, Color.red);
        }

        private void onPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel) {
            databaseManager.logPlayerAsync(player.CSteamID, PlayerLogType.CHAT, message);
        }

        public override void UnloadPlugin(PluginState state = PluginState.Unloaded) {
            base.UnloadPlugin(state);
            U.Events.OnPlayerConnected -= onPlayerConnected;
            U.Events.OnPlayerDisconnected -= onPlayerDisconnected;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateGesture -= onPlayerUpdateGesture;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStance -= onPlayerUpdateStance;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStat -= onPlayerUpdateStat;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerChatted -= onPlayerChatted;

            vehicleManager.onPluginUnloaded();

            wasUnloaded = true;
            Logger.LogWarning("\tAlskeboPlugin Unloaded");
        }

        private void onLevelLoaded(int level) {
            Level.onLevelLoaded -= onLevelLoaded;

            int disabledATMs = 0;
            for (int x = 0; x < Regions.WORLD_SIZE; ++x) {
                for (int y = 0; y < Regions.WORLD_SIZE; ++y) {
                    List<LevelObject> list = LevelObjects.objects[x, y];
                    for (int i = 0; i < list.Count; ++i) {
                        LevelObject o = list[i];
                        if (o.asset.interactability == EObjectInteractability.DROPPER) {
                            if (o.asset.name.Equals("ATM_0")) {
                                Logger.Log("Disabled ATM at " + o.transform.position.ToString());
                                InteractableObjectDropper d = o.transform.gameObject.GetComponent<InteractableObjectDropper>();
                                UnityEngine.Transform.Destroy(d);
                                disabledATMs++;
                            }
                        }
                    }
                }
            }
            if (disabledATMs < 6) {
                throw new Exception("Not all 6 ATMs were disabled.");
            }
        }

        private void onPlayerConnected(UnturnedPlayer player) {
            UnturnedChat.Say(player.DisplayName + " has connected.");
            databaseManager.logPlayerAsync(player.CSteamID, PlayerLogType.CONNECT);
            playerManager.onPlayerConnected(player);

            new Thread(delegate () {
                DatabasePlayer dbp;
                if (!databaseManager.playerExists(player.CSteamID)) {
                    databaseManager.insertPlayer(player.CSteamID, player.SteamGroupID, player.DisplayName, player.SteamName, player.CharacterName, false);
                    dbp = AlskeboUnturnedPlugin.databaseManager.receivePlayer(player.CSteamID);
                    UnturnedChat.Say(player, "Welcome to Alskebo. Use /info to get started.");
                } else {
                    databaseManager.updatePlayer(player.CSteamID, player.SteamGroupID, player.DisplayName, player.SteamName, player.CharacterName);
                    dbp = AlskeboUnturnedPlugin.databaseManager.receivePlayer(player.CSteamID);
                    TimeSpan timeSpan = DateTime.Now - dbp.lastJoin;
                    UnturnedChat.Say(player, "Welcome back. Your last login was " + (timeSpan.Days >= 1 ? (timeSpan.Days + " days and ") : "") + (timeSpan.Hours >= 1 ? (timeSpan.Hours + " hours and ") : "") + timeSpan.Minutes + " minutes ago.");
                    databaseManager.setPlayerLastJoin(player.CSteamID);
                }

                playerManager.setPlayerData(player, "balance", dbp.balance);
                playerManager.setPlayerData(player, "receivedvehicle", dbp.receivedVehicle);
                if (!dbp.receivedVehicle)
                    UnturnedChat.Say(player, "Receive your one-time free personal car with \"/firstvehicle\"!");
            }).Start();

            vehicleManager.onPlayerConnected(player);
        }

        private void onPlayerDisconnected(UnturnedPlayer player) {
            databaseManager.logPlayerAsync(player.CSteamID, PlayerLogType.DISCONNECT);
            playerManager.onPlayerDisconnected(player);
            vehicleManager.onPlayerDisconnected(player);
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
