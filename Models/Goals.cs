using System.Text;

namespace Rex3.Models
{
    public enum SecretGoal
    {
        None,
        KillThemAll
    }

    public static class Extensions
    {
        public static bool IsAchieved(this SecretGoal goal, Level level)
        {
            switch (goal)
            {
                case SecretGoal.None:
                    return true;

                case SecretGoal.KillThemAll:
                    return level.Enemies.Count == 0;

                default:
                    return false;
            }
        }

        public static string ToDescription(this SecretGoal goal)
        {
            switch (goal)
            {
                case SecretGoal.None:
                    return "There is no special goal, just find the stairs";

                case SecretGoal.KillThemAll:
                    return "Kill all enemies";

                default:
                    return "Unknown goal";
            }
        }
    }

    public static class Mysteries
    {
        private static List<string> mysteries;
        private static List<string> secrets;

        static Mysteries()
        {
            mysteries = new List<string>();
            mysteries.Add("pierwsza tajemnica {0} {1} {2}");
            mysteries.Add("druga tajemnica {0} {1} {2}");
            mysteries.Add("trzecia tajemnica {0} {1} {2}");

            secrets = new List<string>();
            secrets.Add("część ukryta 1 1");
            secrets.Add("część ukryta 1 2");
            secrets.Add("część ukryta 1 3");

            secrets.Add("część ukryta 2 1");
            secrets.Add("część ukryta 2 2");
            secrets.Add("część ukryta 2 3");

            secrets.Add("część ukryta 3 1");
            secrets.Add("część ukryta 3 2");
            secrets.Add("część ukryta 3 3");
        }

        public static string GenerateLoseMystery(Level level, int levelIndex)
        {
            var sb = new StringBuilder();
            var m = GenerateMystery(level, levelIndex);
            var r = new Random();

            foreach (var c in m)
            {
                if (r.Next(10) < 3)
                    sb.Append(c);
                else
                    sb.Append('_');
            }

            return sb.ToString();
        }

        public static string GenerateMystery(Level level, int levelIndex)
        {
            int goals = 0;
            if (level.ClairvoyantGoal.IsAchieved(level))
                goals++;
            if (level.ScribeGoal.IsAchieved(level))
                goals++;
            if (level.NavigatorGoal.IsAchieved(level))
                goals++;

            string secr1 = secrets[levelIndex * 3];
            string secr2 = secrets[levelIndex * 3 + 1];
            string secr3 = secrets[levelIndex * 3 + 2];

            if (goals == 2)
                secr3 = string.Join("", secr3.ToCharArray().Select(c => "*"));

            if (goals == 1)
                secr2 = string.Join("", secr2.ToCharArray().Select(c => "*"));

            if (goals == 0)
                secr1 = string.Join("", secr1.ToCharArray().Select(c => "*"));

            return string.Format(mysteries[levelIndex], secr1, secr2, secr3);
        }
    }
}
