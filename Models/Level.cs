using System.Drawing;

namespace Rex3.Models
{
    public class Level
    {
        public int EnergyRecoveryRate { get; set; }
        public int EnemiesCount { get; set; }
        public int BoxesCount { get; set; }
        public int EnergyRecoveryAmount { get; set; }
        public SecretGoal ClairvoyantGoal { get; set; }
        public SecretGoal ScribeGoal { get; set; }
        public SecretGoal NavigatorGoal { get; set; }
        public Point StairsLocation { get; set; }
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();
        public List<Box> Boxes { get; set; } = new List<Box>();
    }
}
