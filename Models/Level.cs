using System.Drawing;

namespace Rex3.Models
{
    public class Level
    {
        public int EnergyRecoveryRate { get; set; }
        public int EnergyRecoveryAmount { get; set; }
        public SecretGoals ClairvoyantGoal { get; set; }
        public SecretGoals ScribeGoal { get; set; }
        public SecretGoals NavigatorGoal { get; set; }
        public Point StairsLocation { get; set; }
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();
        public List<Box> Boxes { get; set; } = new List<Box>();
    }
}
