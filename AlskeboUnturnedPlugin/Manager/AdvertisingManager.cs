using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace AlskeboUnturnedPlugin {
    public class AdvertisingManager {
        public List<string> messages = new List<string>();
        private string lastMessage = "";
        private Random r = new Random();
        private Timer timer;
        private String prefix = "[INFO]";

        public AdvertisingManager() {
            timer = new Timer();
            timer.Interval = 1000 * 60 * 2; // Every x minutes
            timer.Elapsed += Timer_Elapsed;

            messages.Add("Natural cars respawn after " + AlskeboVehicleManager.vehicleDestroyMinutes + " minutes idle.");
            messages.Add("Cars have 5x HP. Buildables have 2x HP.");
            messages.Add("You can view structure health with the command \"/sinfo\".");
            messages.Add("You can view barricade health with the command \"/binfo\".");
            messages.Add("Alskebo.com has many custom features. Take a look in \"/p\".");
            messages.Add("Buy vehicles with \"/buyvehicle <id/name>\".");
            messages.Add("Admin abuse is nonexistent in this server.");
            messages.Add("Natural vehicles can't be locked. You can drive them all!");
            messages.Add("You can view your owned vehicles with \"/myvehicles\".");
            messages.Add("When you find money items you can deposit them with \"/depositmoney\".");
            messages.Add("To check your balance, use \"/balance\".");
            messages.Add("Use \"/info\" to display all help messages.");
            messages.Add("You can only lock your own vehicles.");
            messages.Add("Buy a lottery ticket for a chance to win some great loot! (\"/buyticket\")");
            messages.Add("Lock your vehicles remotely. (\"/lock\")");
            messages.Add("Unlock your vehicles remotely. (\"/lock\")");

            timer.Start();
        }

        ~AdvertisingManager() {
            timer.Stop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            displayMessage();
        }

        public void displayMessage() {
            if (Provider.Players.Count <= 0)
                return;

            String message = messages[r.Next(messages.Count)];
            if (message.Equals(lastMessage)) {
                displayMessage();
                return;
            }
            UnturnedChat.Say(prefix + " " + message, UnityEngine.Color.yellow);
            lastMessage = message;
        }
    }
}
