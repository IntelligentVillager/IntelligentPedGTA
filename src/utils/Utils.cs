using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA.NPCTest.src.utils
{
    class Utils
    {
        public static string TEST_NODE_ID = "9dcf5d19-5c4c-4ae0-a75d-56ad27ea892b";

        public static String randomName()
        {
            List<string> firstNames = new List<string> { "Alice", "Bob", "Charlie", "Dave", "Eve", "Frank", "Gina", "Harry", "Ivy", "Jack", "Kate", "Linda", "Mike", "Nina", "Oscar", "Peggy", "Quincy", "Rita", "Sam", "Tina", "Ursula", "Violet", "Wendy", "Xander", "Yolanda", "Zach" };
            List<string> lastNames = new List<string> { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen", "Young", "King", "Wright", "Scott", "Green", "Baker", "Adams", "Nelson", "Carter", "Mitchell", "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker", "Evans", "Edwards", "Collins", "Stewart", "Sanchez", "Morris", "Rogers", "Reed", "Cook", "Morgan", "Bell", "Murphy", "Bailey", "Rivera", "Cooper", "Richardson", "Cox", "Howard", "Ward", "Torres", "Peterson", "Gray", "Ramirez", "James", "Watson", "Brooks", "Kelly", "Sanders", "Price", "Bennett", "Wood", "Barnes", "Ross", "Henderson", "Coleman", "Jenkins", "Perry", "Powell", "Long", "Patterson", "Hughes", "Flores", "Washington", "Butler", "Simmons", "Foster", "Gonzales", "Bryant", "Alexander", "Russell", "Griffin", "Diaz", "Hayes" };
            Random rng = new Random();
            string randomName = firstNames.OrderBy(x => rng.Next()).First() + " " + lastNames.OrderBy(x => rng.Next()).First();
            return randomName;
        }

        public static PedHash randomPedHash()
        {
            Random rng = new Random();
            //PedHash.AfriAmer01AMM, PedHash.BallaOrig01GMY, PedHash.Bevhills01AMM
            PedHash[] pedHashes = new PedHash[] { PedHash.Abigail };
            int randomIndex = rng.Next(0, pedHashes.Length);
            PedHash randomPedHash = pedHashes[randomIndex];
            return randomPedHash;
        }

        public static float CalculateDecayedVolume(float distance, float threshold = 10.0f, float initialVolume = 1.0f)
        {
            float decayConstant = 0.5f; 
            if (distance > threshold)
            {
                return 0;
            }
            float decayedVolume = (float)(initialVolume * System.Math.Exp(-decayConstant * distance));
            return decayedVolume;
        }
    }
}
