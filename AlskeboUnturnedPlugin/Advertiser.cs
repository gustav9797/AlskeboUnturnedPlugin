using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace AlskeboUnturnedPlugin {
    public class Advertiser {
        private List<string> messages = new List<string>();
        private string lastMessage = "";
        private Random r = new Random();
        private Timer timer;
        private String prefix = "[INFO]";

        public Advertiser() {
            timer = new Timer();
            timer.Interval = 1000 * 60 * 2; // Every x minutes
            timer.Elapsed += Timer_Elapsed;

            messages.Add("Natural cars respawn after some time idle with low fuel.");
            messages.Add("Cars have 5x HP. Buildables have 2x HP.");
            messages.Add("You can view structure health with the command \"/structureinfo\".");
            messages.Add("You can view barricade health with the command \"/barricadeinfo\".");
            messages.Add("Alskebo.com has many custom features. Take a look in \"/p\".");
            //messages.Add("When you join for the first time you will get a personal vehicle. Get it with \"/firstvehicle\".");
            messages.Add("Several ways to get personal vehicles are being added.");
            messages.Add("Admin abuse is nonexistent in this server.");
            messages.Add("Natural vehicles can't be locked. You can drive them all!");
            messages.Add("You can only lock your own vehicles.");
            messages.Add("You can view your owned vehicles with \"/myvehicles\".");

            timer.Start();
        }

        ~Advertiser() {
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
