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
    public class DespawnVehicleCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "DespawnVehicle"; }
        }

        public string Help {
            get { return "Despawn the current vehicle."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get {
                List<string> a = new List<string>();
                a.Add("despawn");
                return a;
            }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer sender = (UnturnedPlayer)caller;
            if (!sender.IsInVehicle && sender.CurrentVehicle != null) {
                UnturnedChat.Say(sender, "You have to be inside a vehicle.");
                return;
            }

            sender.CurrentVehicle.isExploded = true;
            UnturnedChat.Say(sender, "The vehicle was despawned.");
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.despawnvehicle");
                return list;
            }
        }
    }
}
