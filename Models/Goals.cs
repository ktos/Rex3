using System.Text;

namespace Rex3.Models
{
    public enum SecretGoal
    {
        None,
        KillThemAll,
        DoNotKill,
        DoNotRest,
        Die,
        DoNotTake
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

                case SecretGoal.DoNotKill:
                    return level.Enemies.Count == level.EnemiesCount;

                case SecretGoal.DoNotRest:
                    return level.RestCount == 0;

                case SecretGoal.Die:
                    return level.IsDead;

                case SecretGoal.DoNotTake:
                    return level.Boxes.Count == level.BoxesCount;

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
            mysteries.Add(
                "First Mystery: AI was created by {0} over 50 years ago. Nobody knew they will be so fast in dominating the {1}. AI and humans are {2}."
            );
            mysteries.Add(
                "Second Mystery: Going through the mazes of {0} is not easy, because there is a lot of {1}. Later, it was discovered that {2}."
            );
            mysteries.Add(
                "Trzecia Tajemnica: Rex and SI always were {0}. It was one of the reasons he was called the renegade and banished from the Earth. Rex is however {1}, while the true {2}."
            );

            secrets = new List<string>();
            secrets.Add("humans");
            secrets.Add("cyberspace");
            secrets.Add("allies");

            secrets.Add("your own memory");
            secrets.Add("mad AIs, lost experiments and wildlings");
            secrets.Add("you have to divide the AI into three parts for stability");

            secrets.Add("close friends");
            secrets.Add(
                "an artificial human, a flesh body for a human mind, but a copy of the mind"
            );
            secrets.Add("the original Rex is You.");
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
