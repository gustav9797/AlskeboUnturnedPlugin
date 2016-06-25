using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AlskeboUnturnedPlugin {
    public class Advertiser {
        private List<string> messages = new List<string>();
        private string lastMessage = "";
        private Random r = new Random();
        private Timer timer;

        public Advertiser() {
            timer = new Timer();
            timer.Interval = 1000 * 60 * 2; // Every x minutes
            timer.Elapsed += Timer_Elapsed;

            messages.Add("Natural cars will respawn after some time being idle with low fuel level. You will notice it.");
            messages.Add("You can view structure health by executing the command \"/structureinfo\" and then punch close to any structures.");
            messages.Add("You can view barricade info and health by executing the command \"/barricadeinfo\" and then punch close to any barricades.");
            messages.Add("Alskebo.com is an Unturned server with many custom features. Take a look in \"/p\" to show some commands.");
            messages.Add("When you join Alskebo.com for the first time, you will get a free personal vehicle which does not despawn. Get it by executing \"/firstvehicle\".");

            timer.Start();
        }

        ~Advertiser() {
            timer.Stop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            displayMessage();
        }

        public void displayMessage() {
            String message = messages[r.Next(messages.Count)];
            if (message.Equals(lastMessage)) {
                displayMessage();
                return;
            }
            UnturnedChat.Say(message, UnityEngine.Color.yellow);
        }
    }
}
