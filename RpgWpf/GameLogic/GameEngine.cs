using System.Text;
using RpgWpf.GameCore;

namespace RpgWpf.GameLogic
{
    public class GameEngine
    {
        public Charakter Player { get; }
        public Goblin Goblin { get; }
        public Werwolf Werwolf { get; }
        public Elfe Elfe { get; }

        public bool GoblinTot { get; private set; }
        public bool WerwolfTot { get; private set; }
        public bool ElfeTot { get; private set; }
        public bool CharakterTot => Player.IsDead;

        private readonly string _adminPasswort = "123";

        public GameEngine(string vorname, string playerTag, int alter, int inventarGroesse)
        {
            Player = new Charakter(vorname, playerTag, alter, inventarGroesse);
            Goblin = new Goblin();
            Werwolf = new Werwolf();
            Elfe = new Elfe();
        }

        public string GetStatusText()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Playertag: {Player.PlayerTag}");
            sb.AppendLine($"Alter: {Player.Alter}");
            sb.AppendLine();
            sb.AppendLine($"Health Points: {Player.HP}");
            sb.AppendLine($"Max. Health: {Player.MaxHP}");
            sb.AppendLine($"Attack damage: {Player.GetAttackDamage()}");
            sb.AppendLine($"Multiplikator (Special Attack): {Player.DamageMultiplier}x");
            sb.AppendLine($"Special Attack damage: {Player.GetSpecialAttackDamage()}");
            sb.AppendLine($"Wahrscheinlichkeit (Special Attack): {Player.SpecialAttackChancePercent}%");
            return sb.ToString();
        }

        public string Attack(Entity gegner)
        {
            var sb = new StringBuilder();

            // Beispiel: Leben checken
            if (gegner.HP <= 0)
            {
                sb.AppendLine($"{gegner.Name} ist bereits tot.");
                return sb.ToString();
            }

            // Attacke durchführen …
            // (hier kommt dann deine refactorte KampfRunde-Logik)

            // Gegner tot?
            if (gegner.HP <= 0)
            {
                if (gegner is Goblin) GoblinTot = true;
                if (gegner is Elfe) ElfeTot = true;
                if (gegner is Werwolf) WerwolfTot = true;
            }

            return sb.ToString();
        }

        public string AttackGoblin()
        {
            return Attack(Goblin);
        }


        // Analog AttackElfe(), AttackWerwolf(), UseHealPotion(), UsePoisonPotion() etc.
    }
}
