using Rocket.API;
using Rocket.Core.Assets;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlskeboUnturnedPlugin {
    class GiveVehicleCommand {
        public class CommandHello : IRocketCommand {
            public AllowedCaller AllowedCaller {
                get { return AllowedCaller.Both; }
            }

            public string Name {
                get { return "GiveVehicle"; }
            }

            public string Help {
                get { return "Player parameter is optional. Vehicle can be either ID or vehicle name."; }
            }

            public string Syntax {
                get { return "<player> <vehicle>"; }
            }

            public List<string> Aliases {
                get { return new List<string>(); }
            }

            public void Execute(IRocketPlayer caller, string[] command) {
                UnturnedPlayer who = null;
                string stringId = "";
                if (command.Length >= 2) {
                    who = UnturnedPlayer.FromName(command[0]);
                    stringId = command[1];
                } else if (command.Length >= 1) {
                    who = UnturnedPlayer.FromName(caller.DisplayName);
                    if (who == null) {
                        UnturnedChat.Say(caller, "The target player could not be found.");
                        return;
                    }
                    stringId = command[0];
                } else {
                    UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                    throw new WrongUsageOfCommandException(caller, this);
                }

                String vehicleName = stringId;
                ushort id = 0;
                if (!ushort.TryParse(stringId, out id)) {

                    bool found = false;
                    Asset[] assets = SDG.Unturned.Assets.find(EAssetType.VEHICLE);
                    foreach (VehicleAsset ia in assets) {
                        if (ia != null && ia.Name != null && ia.Name.ToLower().Contains(stringId.ToLower())) {
                            vehicleName = ia.name;
                            id = ia.Id;
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        UnturnedChat.Say(caller, "Could not find the specified vehicle.");
                        return;
                    }
                }

                InteractableVehicle vehicle = AlskeboUnturnedPlugin.vehicleManager.givePlayerOwnedCar(who, id);
                if (vehicle == null) {
                    if (who.DisplayName != caller.DisplayName)
                        UnturnedChat.Say(caller, "Could not give target an owned vehicle.");
                } else {
                    //VehicleManager.Instance.askEnterVehicle(who.CSteamID, vehicle.instanceID, vehicle.asset.hash, (byte)vehicle.asset.engine);
                    UnturnedChat.Say(who, "You were given a " + vehicleName.ToLower() + ".");
                    if (who.DisplayName != caller.DisplayName)
                        UnturnedChat.Say(caller, who.DisplayName + " was given a " + vehicleName.ToLower() + ".");
                }

            }

            public List<string> Permissions {
                get {
                    List<String> list = new List<string>();
                    list.Add("alskebo.giveownedvehicle");
                    return list;
                }
            }
        }
    }
}
