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
    public class BuyVehicleCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "BuyVehicle"; }
        }

        public string Help {
            get { return "Purchase vehicles."; }
        }

        public string Syntax {
            get { return "<id/name>"; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;

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

                    int playerMoney = AlskeboUnturnedPlugin.playerManager.getPlayerInt(player, "balance");
                    if (playerMoney < vehiclePrice) {
                        UnturnedChat.Say(caller, "You do not have enough money to buy a " + vehicleName + ". It costs $" + vehiclePrice + " and you need $" + (vehiclePrice - playerMoney) + " more to buy it.");
                        return;
                    }

                    if (command.Length <= 1 || !command[1].Equals("confirm")) {
                        UnturnedChat.Say(caller, "You are about to buy a " + vehicleName + " for $" + vehiclePrice + ". Confirm with \"/buyvehicle " + id + " confirm\".");
                    } else {
                        int remainingMoney = playerMoney - vehiclePrice;
                        AlskeboUnturnedPlugin.playerManager.setPlayerData(player, "balance", remainingMoney);
                        AlskeboUnturnedPlugin.databaseManager.setPlayerBalance(player.CSteamID, remainingMoney);
                        AlskeboUnturnedPlugin.vehicleManager.givePlayerOwnedVehicle(player, id);
                        UnturnedChat.Say(player, "Enjoy your personal " + vehicleName + "!");
                    }
                }
            } else {
                UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                throw new WrongUsageOfCommandException(caller, this);
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.buyvehicle");
                return list;
            }
        }
    }
}
