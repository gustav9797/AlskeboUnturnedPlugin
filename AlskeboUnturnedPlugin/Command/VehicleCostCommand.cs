using Rocket.API;
using Rocket.Core.Assets;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class VehicleCostCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Both; }
        }

        public string Name {
            get { return "VehicleCost"; }
        }

        public string Help {
            get { return "Check the price of a vehicle."; }
        }

        public string Syntax {
            get { return "<id/name>"; }
        }

        public List<string> Aliases {
            get {
                List<string> a = new List<string>();
                a.Add("Cost");
                a.Add("Price");
                return a;
            }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            if (command.Length >= 1) {
                string stringId = command[0];
                String vehicleName = stringId;
                ushort id = 0;
                bool found = false;
                if (!ushort.TryParse(stringId, out id)) {
                    Asset[] assets = SDG.Unturned.Assets.find(EAssetType.VEHICLE);
                    foreach (VehicleAsset ia in assets) {
                        if (ia != null && ia.Name != null && ia.Name.ToLower().Contains(stringId.ToLower())) {
                            vehicleName = ia.Name;
                            id = ia.Id;
                            found = true;
                            break;
                        }
                    }
                } else {
                    Asset[] assets = SDG.Unturned.Assets.find(EAssetType.VEHICLE);
                    foreach (VehicleAsset ia in assets) {
                        if (ia != null && ia.id == id) {
                            vehicleName = ia.Name;
                            id = ia.Id;
                            found = true;
                            break;
                        }
                    }
                }

                if (!found) {
                    UnturnedChat.Say(caller, "Could not find the specified vehicle.");
                    return;
                } else {
                    int vehiclePrice = AlskeboUnturnedPlugin.vehicleShop.getPrice(id);
                    if (vehiclePrice == int.MaxValue) {
                        UnturnedChat.Say(caller, "This vehicle is not buyable. Contact admin(gustav9797)");
                        return;
                    }
                    UnturnedChat.Say(caller, "The " + vehicleName + " costs $" + vehiclePrice + ".");
                }
            } else {
                UnturnedChat.Say(caller, "Usage: /vehiclecost " + Syntax);
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.vehiclecost");
                return list;
            }
        }
    }
}
