using Rocket.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlskeboUnturnedPlugin {
    public class VehicleShop {
        private Dictionary<ushort, int> buyableVehicles = new Dictionary<ushort, int>();

        public VehicleShop() {
            for (ushort i = 59; i <= 74; ++i) { //snowmobile,quad
                buyableVehicles.Add(i, 50);
            }

            for (ushort i = 98; i <= 105; ++i) { //jetski
                buyableVehicles.Add(i, 50);
            }
            buyableVehicles.Add(75, 50); //golfcart
            buyableVehicles.Add(91, 50); //makeshift small
            buyableVehicles.Add(97, 50); //runabout

            for (ushort i = 1; i <= 50; ++i) { //Offroader,hatchback,truck,sedan,van,roadster
                if (i != 33 && i != 34)
                    buyableVehicles.Add(i, 75);
            }
            buyableVehicles.Add(76, 75); //taxi
            buyableVehicles.Add(85, 75); //tractor
            buyableVehicles.Add(89, 75); //makeshift medium

            buyableVehicles.Add(33, 125); //policecar
            buyableVehicles.Add(34, 125); //firetruck
            buyableVehicles.Add(54, 125); //ambulance
            buyableVehicles.Add(86, 125); //bus
            buyableVehicles.Add(90, 125); //makeshift large

            buyableVehicles.Add(51, 150); //forest ural
            buyableVehicles.Add(52, 150); //forest humvee
            buyableVehicles.Add(55, 150); //desert ural
            buyableVehicles.Add(56, 150); //desert humvee
            buyableVehicles.Add(87, 150); //forest jeep
            buyableVehicles.Add(88, 150); //desert jeep
            for (ushort i = 77; i <= 84; ++i) { //racecars
                buyableVehicles.Add(i, 150);
            }

            buyableVehicles.Add(92, 150); //sandpiper
            buyableVehicles.Add(96, 150); //otter

            buyableVehicles.Add(53, 200); //forest apc
            buyableVehicles.Add(57, 200); //desert apc
            buyableVehicles.Add(58, 200); //explorer
            buyableVehicles.Add(93, 200); //forest huey
            buyableVehicles.Add(94, 200); //desert huey
            buyableVehicles.Add(95, 200); //skycrane
            buyableVehicles.Add(107, 200); //hummingbird
            buyableVehicles.Add(106, 200); //policeheli

            buyableVehicles.Add(120, 250); //forest tank
            buyableVehicles.Add(121, 250); //desert tank

            for (ushort i = 1; i <= 121; ++i) {
                if (!buyableVehicles.ContainsKey(i))
                    Logger.LogWarning("Vehicle ID " + i + " is not buyable.");
            }
        }

        public int getPrice(ushort id) {
            if (buyableVehicles.ContainsKey(id))
                return buyableVehicles[id];
            return int.MaxValue;
        }
    }
}
