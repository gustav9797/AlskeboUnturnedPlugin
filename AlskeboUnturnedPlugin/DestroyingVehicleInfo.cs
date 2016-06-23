using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlskeboUnturnedPlugin {
    class DestroyingVehicleInfo {
        public InteractableVehicle vehicle;
        public DateTime lastActivity;

        public bool isBeingDestroyed = false;
        public bool lastHonked = false;
        public int timesHonked = 0;
    }
}
