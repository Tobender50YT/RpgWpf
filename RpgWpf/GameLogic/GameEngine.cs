using System.Text;
using RpgWpf.GameCore;

namespace RpgWpf.GameLogic
{
    public class GameEngine
    {
        // --- Öffentliche Spielobjekte (fürs UI lesbar) ---
        public Charakter Player { get; }
        public Goblin Goblin { get; }
        public Werwolf Werwolf { get; }
        public Elfe Elfe { get; }

        // --- Zustände ---
        public bool GoblinTot { get; private set; }
        public bool WerwolfTot { get; private set; }
        public bool ElfeTot { get; private set; }
        public bool CharakterTot => Player.IsDead;

        public bool AlleGegnerTot => GoblinTot && WerwolfTot && ElfeTot;
        public bool GameOver => CharakterTot || AlleGegnerTot;

        // später fürs Admin-Panel nutzbar
        private readonly string _adminPasswort = "123";

        public GameEngine(string vorname, string playerTag, int alter, int inventarGroesse)
        {
            Player = new Charakter(vorname, playerTag, alter, inventarGroesse);
            Goblin = new Goblin();
            Werwolf = new Werwolf();
            Elfe = new Elfe();
        }

        // --- Status-Text für die Charakter-Anzeige (links oben) ---
        public string GetStatusText()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Playertag: {Player.PlayerTag}");
            sb.AppendLine($"Alter: {Player.Alter}");
            sb.AppendLine();
            sb.AppendLine($"Health Points: {Player.HP} / {Player.MaxHP}");
            sb.AppendLine($"Attack damage: {Player.GetAttackDamage()}");
            sb.AppendLine($"Multiplikator (Special Attack): {Player.DamageMultiplier}x");
            sb.AppendLine($"Special Attack damage: {Player.GetSpecialAttackDamage()}");
            sb.AppendLine($"Wahrscheinlichkeit (Special Attack): {Player.SpecialAttackChancePercent}%");

            return sb.ToString();
        }

        // Optional: Status für Gegner-Box (rechts)
        public string GetEnemyStatusText(Entity gegner)
        {
            if (gegner == null) return string.Empty;
            return $"{gegner.Name}\nHP: {gegner.HP} / {gegner.MaxHP}\nDamage: {gegner.GetAttackDamage()}";
        }

        // --- Öffentliche Kampf-Methoden fürs UI ---
        public string AttackGoblin() => Attack(Goblin);
        public string AttackElfe() => Attack(Elfe);
        public string AttackWerwolf() => Attack(Werwolf);

        // --- Zentrale Kampf-Logik (1 Runde) ---
        private string Attack(Entity gegner)
        {
            var sb = new StringBuilder();

            if (gegner == null)
            {
                sb.AppendLine("Kein Gegner ausgewählt.");
                return sb.ToString();
            }

            if (gegner.IsDead)
            {
                sb.AppendLine($"{gegner.Name} ist bereits besiegt!");
                return sb.ToString();
            }

            if (Player.IsDead)
            {
                sb.AppendLine("Du wurdest bereits besiegt!");
                return sb.ToString();
            }

            // Spezialangriff / normaler Angriff wie in deiner refactorten Version
            bool special = Player.RollSpecialAttack();
            double outDmg = special
                ? Player.GetSpecialAttackDamage()
                : Player.GetAttackDamage();

            // Vorherige HP anzeigen
            sb.AppendLine($"Deine Leben: {Player.HP} / {Player.MaxHP}");
            sb.AppendLine($"{gegner.Name} Leben: {gegner.HP} / {gegner.MaxHP}");
            sb.AppendLine();

            // Schaden anwenden
            bool gegnerTot = gegner.TakeDamage(outDmg);
            bool charakterTot = Player.TakeDamage(gegner.GetAttackDamage());

            // Text zu Schaden / Spezialangriff
            if (special)
            {
                sb.AppendLine($"SPEZIALATTACKE x{Player.DamageMultiplier} DAMAGE!!!");
            }

            sb.AppendLine($"Du hast {outDmg} Schaden gemacht!");
            sb.AppendLine($"{gegner.Name} hat {gegner.GetAttackDamage()} Schaden gemacht!");
            sb.AppendLine();

            // Neue HP anzeigen
            sb.AppendLine($"Deine Leben: {Player.HP} / {Player.MaxHP}");
            sb.AppendLine($"{gegner.Name} Leben: {gegner.HP} / {gegner.MaxHP}");

            // Flags setzen, wenn jemand gestorben ist
            if (gegnerTot)
            {
                sb.AppendLine();
                sb.AppendLine($"{gegner.Name} wurde besiegt!");

                if (gegner == Goblin) GoblinTot = true;
                if (gegner == Elfe) ElfeTot = true;
                if (gegner == Werwolf) WerwolfTot = true;
            }

            if (charakterTot)
            {
                sb.AppendLine();
                sb.AppendLine("Du wurdest besiegt!");
            }

            return sb.ToString();
        }
    }
}
