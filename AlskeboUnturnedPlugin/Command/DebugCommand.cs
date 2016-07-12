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
    public class DebugCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "debugtest"; }
        }

        public string Help {
            get { return ""; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            //Lottery.win(player);
            for (int i = 0; i < 50; ++i) {
                //DamageTool.explode(player.Position, 8f, EDeathCause.VEHICLE, 200f, 200f, 200f, 0f, 0f, 500f, 2000f, 500f);
                var vehicle = AlskeboUnturnedPlugin.vehicleManager.spawnNaturalVehicle(player.Position, 40);
                AlskeboUnturnedPlugin.vehicleManager.checkVehicleDestroy(AlskeboUnturnedPlugin.vehicleManager.getOwnedVehicleInfo(vehicle.instanceID), vehicle);
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.debug");
                return list;
            }
        }
    }
}
