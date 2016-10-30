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

        private bool wasUnloaded = false;
        public static AlskeboUnturnedPlugin instance;
        public static DatabaseManager databaseManager;
        public static AlskeboVehicleManager vehicleManager;
        public static AlskeboPlayerManager playerManager;
        public static AdvertisingManager advertiser;
        public static VehicleShop vehicleShop;
        public static BackupManager backupManager;
        public static Lottery lottery;
        public static System.Random r = new System.Random();
        //public static XmppClientConnection xmpp;
        //Jid room = new Jid("unturned@conference.alskebo.com");

        public static AlskeboConfiguration Config { get { return AlskeboUnturnedPlugin.instance.Configuration.Instance; } }

        public override void LoadPlugin() {
            base.LoadPlugin();

            instance = this;
            databaseManager = new DatabaseManager(Configuration.Instance);
            vehicleManager = new AlskeboVehicleManager();
            playerManager = new AlskeboPlayerManager();
            advertiser = new AdvertisingManager();
            vehicleShop = new VehicleShop();
            backupManager = new BackupManager();
            lottery = new Lottery();
            U.Events.OnPlayerConnected += onPlayerConnected;
            U.Events.OnPlayerDisconnected += onPlayerDisconnected;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateGesture += onPlayerUpdateGesture;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStance += onPlayerUpdateStance;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStat += onPlayerUpdateStat;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerChatted += onPlayerChatted;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath += onPlayerDeath;

            if (wasUnloaded) {
                Rocket.Core.Logging.Logger.LogWarning("\tRe-sending onPlayerConnected calls");
                foreach (SteamPlayer player in Provider.clients) {
                    onPlayerConnected(UnturnedPlayer.FromSteamPlayer(player));
                }
            }
            wasUnloaded = false;

            Level.onLevelLoaded += onLevelLoaded;

            /*xmpp = new XmppClientConnection("alskebo.com");
            xmpp.Open("ChatBot", Config.password);
            xmpp.OnLogin += delegate (object sender) {
                MucManager muc = new MucManager(xmpp);
                muc.JoinRoom(room, "ChatBot");
                xmpp.OnMessage += delegate (object sender2, Message message) {
                    if (message.From.Resource == "ChatBot")
                        return;
                    UnturnedChat.Say("[Webchat] " + message.From.Resource + ": " + message.Body);
                };
            };*/

            Rocket.Core.Logging.Logger.LogWarning("\tAlskeboPlugin Loaded Sucessfully");
        }

        public override void UnloadPlugin(PluginState state = PluginState.Unloaded) {
            base.UnloadPlugin(state);
            U.Events.OnPlayerConnected -= onPlayerConnected;
            U.Events.OnPlayerDisconnected -= onPlayerDisconnected;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateGesture -= onPlayerUpdateGesture;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStance -= onPlayerUpdateStance;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStat -= onPlayerUpdateStat;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerChatted -= onPlayerChatted;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath -= onPlayerDeath;

            wasUnloaded = true;
            Rocket.Core.Logging.Logger.LogWarning("\tAlskeboPlugin Unloaded");
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
                    msg = "forgot to use a dressing";
                    break;
                case EDeathCause.BONES:
                    msg = "hit the ground really hard";
                    break;
                case EDeathCause.BOULDER:
                    msg = "was hit by a boulder";
                    break;
                case EDeathCause.BREATH:
                    msg = "ran out of air";
                    break;
                case EDeathCause.BURNER:
                    msg = "met burning zombie";
                    break;
                case EDeathCause.BURNING:
                    msg = "went up in flames";
                    break;
                case EDeathCause.SPIT:
                    msg = "got poisonous spit in their eyes";
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
                    msg = "stepped on barbed wire";
                    break;
                case EDeathCause.SPLASH:
                    msg = "was hit by an explosive bullet";
                    break;
                case EDeathCause.SUICIDE:
                    return;
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
                    msg = "exploded with their vehicle";
                    break;
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
            //if (chatMode == EChatMode.GLOBAL && !message.StartsWith("/")) {
                //xmpp.Send(new Message(room, MessageType.groupchat, "[In-Game] " + player.DisplayName + ": " + message));
            //}
            databaseManager.logPlayerAsync(player.CSteamID, PlayerLogType.CHAT, message);
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
                                Rocket.Core.Logging.Logger.Log("Disabled ATM at " + o.transform.position.ToString());
                                InteractableObjectDropper d = o.transform.gameObject.GetComponent<InteractableObjectDropper>();
                                UnityEngine.Transform.Destroy(d);
                                disabledATMs++;
                            }
                        }
                    }
                }
            }
            if (disabledATMs < 6) {
                Rocket.Core.Logging.Logger.Log("Not all 6 ATMs were disabled.");
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
                    UnturnedChat.Say(player, "Welcome to Alskebo. Type /info to get started.");
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

        }

        private void onPlayerDisconnected(UnturnedPlayer player) {
            databaseManager.logPlayerAsync(player.CSteamID, PlayerLogType.DISCONNECT);

            if (playerManager.getPlayerBoolean(player, "isInsideVehicle"))
                vehicleManager.onPlayerExitVehicle(player, (InteractableVehicle)playerManager.getPlayerData(player, "currentVehicle"));

            playerManager.onPlayerDisconnected(player);
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
                            StructureData structureData = region.structures[(int)index];
                            Structure structure = structureData.structure;
                            ItemStructureAsset itemStructureAsset = (ItemStructureAsset)Assets.find(EAssetType.ITEM, structure.id);
                            new Thread(delegate () {
                                UnturnedChat.Say(player, "Structure status of " + itemStructureAsset.itemName + " (" + itemStructureAsset.id + "):");
                                UnturnedChat.Say(player, "Owner: " + databaseManager.getPlayerDisplayName(new CSteamID(structureData.owner)) + " - " + new CSteamID(structureData.owner).ToString());
                                UnturnedChat.Say(player, "Health: " + structure.health + "/" + structure.asset.health);
                                UnturnedChat.Say(player, "Last activity: " + Utils.toDelay(structureData.objActiveDate));
                            }).Start();
                        }
                    } else
                        UnturnedChat.Say(player, "Could not find any close structures.", Color.red);
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
                            BarricadeData barricadeData = region.barricades[(int)index];
                            Barricade barricade = barricadeData.barricade;
                            ItemBarricadeAsset itemBarricadeAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, barricade.id);
                            new Thread(delegate () {
                                UnturnedChat.Say(player, "Structure status of " + itemBarricadeAsset.itemName + " (" + itemBarricadeAsset.id + "):");
                                UnturnedChat.Say(player, "Owner: " + databaseManager.getPlayerDisplayName(new CSteamID(barricadeData.owner)) + " - " + new CSteamID(barricadeData.owner).ToString());
                                UnturnedChat.Say(player, "Health: " + barricade.health + "/" + barricade.asset.health);
                                UnturnedChat.Say(player, "Last activity: " + Utils.toDelay(barricadeData.objActiveDate));
                            }).Start();
                        }
                    } else
                        UnturnedChat.Say(player, "Could not find any close barricades.", Color.red);
                }
            }
        }

        private void onPlayerUpdateStance(UnturnedPlayer player, byte stance) {
            bool isInsideVehicle = playerManager.getPlayerBoolean(player, "isInsideVehicle");

            if ((stance == 6 || stance == 7) && !isInsideVehicle) {
                playerManager.setPlayerData(player, "isInsideVehicle", true);
                playerManager.setPlayerData(player, "currentVehicle", player.CurrentVehicle);
                vehicleManager.onPlayerEnterVehicle(player, player.CurrentVehicle);
            } else if (stance != 6 && stance != 7 && isInsideVehicle) {
                playerManager.setPlayerData(player, "isInsideVehicle", false);
                vehicleManager.onPlayerExitVehicle(player, (InteractableVehicle)playerManager.getPlayerData(player, "currentVehicle"));
                playerManager.setPlayerData(player, "currentVehicle", null);
            }
        }

        private void onPlayerUpdateStat(UnturnedPlayer player, EPlayerStat stat) {
            if (stat == EPlayerStat.KILLS_ZOMBIES_MEGA)
                UnturnedChat.Say("Rumours say " + player.DisplayName + " has killed a mega zombie...", Color.blue);
        }

    }
}
