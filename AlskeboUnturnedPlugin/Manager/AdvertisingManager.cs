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

            messages.Add("Natural cars respawn after 5 minutes idle with a fuel level below 1%.");
            messages.Add("Alskebo.com has many custom features. Take a look at the guide: \"/guide\".");
            messages.Add("Admin abuse is nonexistent in this server.");
            messages.Add("Natural vehicles can't be locked. You can drive them all!");
            messages.Add("Type \"/help\" to show useful help.");
            messages.Add("You can only lock your own vehicles.");
            messages.Add("Buy a lottery ticket for a chance to win some great loot! (\"/buyticket\")");
            messages.Add("Check out the website! Type \"/website\".");
            messages.Add("Type \"/vote\" and vote, then type \"/reward\" to get a reward.");

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
            UnturnedChat.Say(prefix + " " + message, UnturnedChat.GetColorFromRGB(255, 215, 0));
            lastMessage = message;
        }
    }
}
