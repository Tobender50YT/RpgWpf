using RpgWpf.GameCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RpgWpf.GameLogic
{
    /// <summary>
    /// Zentrale Spiel-Engine für das WPF-RPG. Kapselt Kampf, Drops, Coins, Level/EXP und Shop-Logik.
    /// </summary>
    public class GameEngine
    {
        private readonly Random _rng = new Random();

        // ============================
        //   Öffentliche Spielobjekte
        // ============================

        /// <summary> Spieler-Charakter mit Inventar und Spezialangriff-Logik. </summary>
        public Charakter Player { get; }

        /// <summary> Bekannte Gegner-Instanzen. </summary>
        public Goblin Goblin { get; }
        public Elfe Elfe { get; }
        public Werwolf Werwolf { get; }

        public Slime Slime { get; }
        public Orc Orc { get; }
        public Dragon Dragon { get; }

        /// <summary>
        /// Alle Gegner in einer Liste, z. B. für Bindings in der WPF-UI.
        /// </summary>
        public List<Entity> Enemies { get; }

        // ============================
        //   Progress / Meta
        // ============================

        /// <summary> Aktuelle Coin-Anzahl des Spielers. </summary>
        public int Coins { get; private set; }

        /// <summary>
        /// Anzahl aller abgeschlossenen Kämpfe (eine "Runde" im Sinne: Kampf bis Spieler oder Gegner tot ist).
        /// </summary>
        public int FinishedBattles { get; private set; }

        /// <summary>
        /// Zählt, wie oft ein bestimmter Gegner bereits besiegt wurde.
        /// </summary>
        private readonly Dictionary<Entity, int> _defeatCounter = new Dictionary<Entity, int>();

        private readonly string _adminPassword = "123";

        // ============================
        //   Konstruktor
        // ============================

        public GameEngine(string vorname, string playerTag, int alter, int inventarGroesse)
        {
            Player = new Charakter(vorname, playerTag, alter, inventarGroesse);

            Goblin = new Goblin();
            Elfe = new Elfe();
            Werwolf = new Werwolf();

            Slime = new Slime();
            Orc = new Orc();
            Dragon = new Dragon();

            Enemies = new List<Entity>
            {
                Slime,
                Goblin,
                Elfe,
                Orc,
                Werwolf,
                Dragon
            };

            // Startwert – kann später reduziert werden, wenn Balancing angepasst werden soll
            Coins = 10000;
        }

        // ============================
        //   Status-Helfer
        // ============================

        /// <summary>
        /// Liefert eine mehrzeilige Zusammenfassung des Spielers inkl. Level, EXP, Coins.
        /// </summary>
        public string GetStatusText()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Playertag: {Player.PlayerTag}");
            sb.AppendLine($"Alter: {Player.Alter}");
            sb.AppendLine($"Level: {Player.Level}");
            sb.AppendLine($"EXP: {Player.CurrentExp} / {Player.ExpToNextLevel}");
            sb.AppendLine();
            sb.AppendLine($"Health Points: {Player.HP} / {Player.MaxHP}");
            sb.AppendLine($"Attack damage: {Player.GetAttackDamage()}");
            sb.AppendLine($"Multiplikator (Special Attack): {Player.DamageMultiplier}x");
            sb.AppendLine($"Special Attack damage: {Player.GetSpecialAttackDamage()}");
            sb.AppendLine($"Special chance: {Player.SpecialAttackChancePercent}%");
            sb.AppendLine();
            sb.AppendLine($"Coins: {Coins}");

            return sb.ToString();
        }

        /// <summary>
        /// Liefert eine kompakte Statusanzeige für einen Gegner.
        /// </summary>
        public string GetEnemyStatusText(Entity enemy)
        {
            if (enemy == null) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine(enemy.Name);
            sb.AppendLine($"HP: {enemy.HP} / {enemy.MaxHP}");
            sb.AppendLine($"Attack: {enemy.GetAttackDamage()}");
            sb.AppendLine($"Besiegt: {GetDefeatCount(enemy)}x");
            return sb.ToString();
        }

        /// <summary>
        /// Gibt zurück, wie oft ein bestimmter Gegner besiegt wurde.
        /// </summary>
        public int GetDefeatCount(Entity enemy)
        {
            if (enemy == null) return 0;
            return _defeatCounter.TryGetValue(enemy, out var count) ? count : 0;
        }

        /// <summary> True, wenn der Spieler keine HP mehr hat. </summary>
        public bool IsGameOver => Player.IsDead;

        /// <summary> Passwortprüfung für Admin-Funktionen. </summary>
        public bool IsValidAdminPassword(string input) =>
            string.Equals(input, _adminPassword, StringComparison.Ordinal);

        // ============================
        //   Kampf-API für das UI
        // ============================

        /// <summary> Komfort-Wrapper für alte Click-Handler – greift den Goblin an. </summary>
        public string AttackGoblin() => Attack(Goblin);

        /// <summary> Komfort-Wrapper für alte Click-Handler – greift die Elfe an. </summary>
        public string AttackElfe() => Attack(Elfe);

        /// <summary> Komfort-Wrapper für alte Click-Handler – greift den Werwolf an. </summary>
        public string AttackWerwolf() => Attack(Werwolf);

        /// <summary>
        /// Führt eine komplette Kampfrunde gegen den angegebenen Gegner aus.
        /// Eine Runde endet, wenn entweder der Gegner oder der Spieler 0 HP erreicht.
        /// </summary>
        /// <param name="enemy">Zielgegner der Runde.</param>
        /// <returns>Mehrzeiliger Log-Text für das Game-Log.</returns>
        public string Attack(Entity enemy)
        {
            var sb = new StringBuilder();

            if (enemy == null)
            {
                sb.AppendLine("Kein Gegner ausgewählt.");
                return sb.ToString();
            }

            if (Player.IsDead)
            {
                sb.AppendLine("Der Charakter ist bereits besiegt.");
                return sb.ToString();
            }

            // Ausgangszustand vor der Runde
            sb.AppendLine($"Deine Leben: {Player.HP} / {Player.MaxHP}");
            sb.AppendLine($"{enemy.Name} Leben: {enemy.HP} / {enemy.MaxHP}");
            sb.AppendLine();

            // Spieler greift an (ggf. Spezialangriff)
            bool special = Player.RollSpecialAttack();
            double playerDamage = special
                ? Player.GetSpecialAttackDamage()
                : Player.GetAttackDamage();

            // Gegner greift zurück
            double enemyDamage = enemy.GetAttackDamage();

            bool enemyDied = enemy.TakeDamage(playerDamage);
            bool playerDied = Player.TakeDamage(enemyDamage);

            // Log-Ausgaben zur Runde
            if (special)
            {
                sb.AppendLine($"Spezialangriff x{Player.DamageMultiplier} damage!");
            }

            sb.AppendLine($"Du hast {playerDamage} Schaden gemacht.");

            if (enemyDamage > 0)
            {
                sb.AppendLine($"{enemy.Name} hat {enemyDamage} Schaden gemacht.");
            }
            else
            {
                sb.AppendLine($"{enemy.Name} hat keinen Schaden verursacht.");
            }

            sb.AppendLine();

            // Neue HP nach der Runde
            sb.AppendLine($"Deine Leben: {Player.HP} / {Player.MaxHP}");
            sb.AppendLine($"{enemy.Name} Leben: {enemy.HP} / {enemy.MaxHP}");

            // Abschluss einer Kampf-Runde (bis Spieler oder Gegner tot ist)
            if (enemyDied || playerDied)
            {
                FinishedBattles++;

                if (enemyDied)
                {
                    HandleEnemyDefeated(enemy, sb);
                }

                if (playerDied)
                {
                    sb.AppendLine();
                    sb.AppendLine("Du wurdest besiegt.");
                    HandlePlayerDefeated(sb);
                }
            }

            return sb.ToString();
        }

        // ============================
        //   Potions / Inventar
        // ============================

        /// <summary>
        /// Verwendet ein ausgewähltes Inventar-Item.
        /// HealPotion wird auf den Spieler angewendet.
        /// PoisonPotion benötigt ein Ziel (Gegner).
        /// </summary>
        public string UseInventoryItem(IInventarItem item, Entity target)
        {
            if (item == null)
            {
                return "Es wurde kein Item ausgewählt.";
            }

            var inv = Player.Inventar;

            // HealPotion: wird auf den Spieler angewendet
            if (item is HealPotion heal)
            {
                bool used = heal.useItem(Player);
                if (!used)
                {
                    return "HealPotion hatte keinen Effekt (HP vermutlich bereits voll).";
                }

                inv.Remove(heal);
                return "HealPotion wurde auf den Charakter angewendet.";
            }

            // PoisonPotion: Ziel-Gegner erforderlich
            if (item is PoisonPotion poison)
            {
                if (target == null)
                {
                    return "Für PoisonPotion muss ein Ziel ausgewählt werden.";
                }

                bool used = poison.useItem(target);
                if (!used)
                {
                    return "PoisonPotion konnte nicht angewendet werden.";
                }

                inv.Remove(poison);
                return $"{target.Name} wurde durch eine PoisonPotion verletzt.";
            }

            // Generische Items: Standardverhalten – wenn kein Ziel angegeben ist, auf den Spieler
            bool consumed = item.useItem(target ?? Player);
            if (consumed)
            {
                inv.Remove(item);
                return $"{item.ItemName} wurde verwendet.";
            }

            return $"{item.ItemName} hatte keinen Effekt.";
        }

        // ============================
        //   Shop-Logik (Attack / HP / Potions)
        // ============================

        /// <summary>
        /// Versucht, einen Angriffs-Upgrade zu kaufen. Kosten werden in Coins abgezogen.
        /// </summary>
        public bool TryBuyAttackUpgrade(int cost, double attackIncrease, out string message)
        {
            if (!CheckCoins(cost, out message))
            {
                return false;
            }

            Coins -= cost;

            // Basis-Angriffsschaden wird erhöht
            double newBaseAttack = Player.GetAttackDamage() + attackIncrease;
            Player.SetBaseAttack(newBaseAttack);

            message = $"Attack damage wurde um {attackIncrease} erhöht (neu: {Player.GetAttackDamage()}).";
            return true;
        }

        /// <summary>
        /// Versucht, ein MaxHP-Upgrade zu kaufen. Kosten werden in Coins abgezogen.
        /// MaxHP wird erhöht und der Spieler danach vollständig geheilt.
        /// </summary>
        public bool TryBuyMaxHealthUpgrade(int cost, double hpIncrease, out string message)
        {
            if (!CheckCoins(cost, out message))
            {
                return false;
            }

            Coins -= cost;

            // MaxHP des Spielers erhöhen
            Player.SetMaxHP(Player.MaxHP + hpIncrease);

            // Nur um den Upgrade-Betrag heilen – aber nicht über MaxHP
            double healedHp = Player.HP + hpIncrease;
            if (healedHp > Player.MaxHP)
                healedHp = Player.MaxHP;

            Player.SetHP(healedHp);


            message = $"Max health wurde um {hpIncrease} erhöht (jetzt: {Player.MaxHP}).";
            return true;
        }

        /// <summary>
        /// Versucht, eine HealPotion mit gegebener Stärke (1 oder 2) zu kaufen.
        /// </summary>
        public bool TryBuyHealPotion(int strength, out string message)
        {
            int cost;
            if (strength == 1)
                cost = 12;
            else if (strength == 2)
                cost = 20;
            else
            {
                message = "Nur HealPotions der Stufe 1 oder 2 können gekauft werden.";
                return false;
            }

            if (!CheckCoins(cost, out message))
            {
                return false;
            }

            Coins -= cost;
            var potion = new HealPotion(ItemGroesse: strength);
            bool stored = Player.Inventar.Add(potion);
            if (!stored)
            {
                message = "Inventar ist voll, die Potion konnte nicht hinzugefügt werden.";
                return false;
            }

            message = $"HealPotion (Stufe {strength}) wurde gekauft und dem Inventar hinzugefügt.";
            return true;
        }

        /// <summary>
        /// Versucht, eine PoisonPotion mit gegebener Stärke (1 oder 2) zu kaufen.
        /// </summary>
        public bool TryBuyPoisonPotion(int strength, out string message)
        {
            int cost;
            if (strength == 1)
                cost = 10;
            else if (strength == 2)
                cost = 18;
            else
            {
                message = "Nur PoisonPotions der Stufe 1 oder 2 können gekauft werden.";
                return false;
            }

            if (!CheckCoins(cost, out message))
            {
                return false;
            }

            Coins -= cost;
            var potion = new PoisonPotion(ItemGroesse: strength);
            bool stored = Player.Inventar.Add(potion);
            if (!stored)
            {
                message = "Inventar ist voll, die Potion konnte nicht hinzugefügt werden.";
                return false;
            }

            message = $"PoisonPotion (Stufe {strength}) wurde gekauft und dem Inventar hinzugefügt.";
            return true;
        }

        /// <summary>
        /// Prüft, ob genügend Coins für einen Kauf vorhanden sind.
        /// </summary>
        private bool CheckCoins(int cost, out string message)
        {
            if (cost <= 0)
            {
                message = "Kosten müssen größer als 0 sein.";
                return false;
            }

            if (Coins < cost)
            {
                message = $"Es werden {cost} Coins benötigt (aktuell: {Coins}).";
                return false;
            }

            message = string.Empty;
            return true;
        }

        // ============================
        //   Interne Helfer (Drops, Sieg, Niederlage)
        // ============================

        /// <summary>
        /// Behandelt die Konsequenzen, wenn ein Gegner besiegt wurde:
        /// Coins, EXP, Potion-Drop, Sieg-Regeneration und Respawn des Gegners.
        /// </summary>
        private void HandleEnemyDefeated(Entity enemy, StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine($"{enemy.Name} wurde besiegt.");

            // Kill-Zähler für diesen Gegner
            if (_defeatCounter.ContainsKey(enemy))
            {
                _defeatCounter[enemy]++;
            }
            else
            {
                _defeatCounter[enemy] = 1;
            }

            // Coin-Reward abhängig vom Gegnertyp
            int reward = GetCoinReward(enemy);
            if (reward > 0)
            {
                Coins += reward;
                sb.AppendLine($"+{reward} Coins erhalten (insgesamt: {Coins}).");
            }

            // EXP-Reward
            int exp = GetExpReward(enemy);
            if (exp > 0)
            {
                bool leveledUp = Player.GainExp(exp);
                sb.AppendLine($"+{exp} EXP erhalten (Level {Player.Level}, {Player.CurrentExp}/{Player.ExpToNextLevel} EXP).");

                if (leveledUp)
                {
                    sb.AppendLine("Level-Up! HP wurden vollständig aufgefüllt.");
                }
            }

            // Potion-Drop – alle Potions immer ins Inventar
            var drop = CreateRandomDrop(enemy);
            if (drop != null)
            {
                bool stored = Player.Inventar.Add(drop);
                if (stored)
                {
                    sb.AppendLine($"{drop.ItemName} wurde dem Inventar hinzugefügt.");
                }
                else
                {
                    sb.AppendLine($"Inventar ist voll, {drop.ItemName} wurde verworfen.");
                }
            }

            // Sieg-Regeneration (nur bei Sieg, nicht bei Niederlage)
            ApplyVictoryRegeneration(sb);

            // Monster respawnt mit vollen Leben
            enemy.SetHP(enemy.MaxHP);
        }

        /// <summary>
        /// Gibt die Coin-Belohnung für einen besiegten Gegner zurück.
        /// </summary>
        private int GetCoinReward(Entity enemy)
        {
            if (enemy is Slime) return 5;
            if (enemy is Goblin) return 10;
            if (enemy is Elfe) return 20;
            if (enemy is Orc) return 35;
            if (enemy is Werwolf) return 50;
            if (enemy is Dragon) return 80;

            return 10;
        }

        /// <summary>
        /// Gibt die EXP-Belohnung für einen besiegten Gegner zurück.
        /// </summary>
        private int GetExpReward(Entity enemy)
        {
            if (enemy is Slime) return 5;
            if (enemy is Goblin) return 10;
            if (enemy is Elfe) return 20;
            if (enemy is Orc) return 35;
            if (enemy is Werwolf) return 50;
            if (enemy is Dragon) return 80;

            return 10;
        }

        /// <summary>
        /// Regeneration nach einem gewonnenen Kampf (z. B. 10 % der MaxHP).
        /// </summary>
        private void ApplyVictoryRegeneration(StringBuilder sb)
        {
            double healAmount = Player.MaxHP * 0.1; // 10 %
            double before = Player.HP;
            double after = Math.Min(Player.MaxHP, Player.HP + healAmount);

            Player.SetHP(after);

            double actualHeal = after - before;
            if (actualHeal > 0)
            {
                sb.AppendLine($"Regeneration nach Sieg: +{actualHeal} HP (jetzt {Player.HP}/{Player.MaxHP}).");
            }
        }

        /// <summary>
        /// Behandelt eine Niederlage des Spielers:
        /// Coins und Inventar werden gelöscht, HP werden aufgefüllt.
        /// Gegner bleiben unverändert.
        /// </summary>
        private void HandlePlayerDefeated(StringBuilder sb)
        {
            // Coins löschen
            Coins = 0;

            // Inventar leeren
            var inv = Player.Inventar;
            var items = inv.Snapshot();
            foreach (var item in items)
            {
                inv.Remove(item);
            }

            // HP wiederherstellen
            Player.SetHP(Player.MaxHP);

            sb.AppendLine("Alle Coins und das komplette Inventar wurden gelöscht.");
            sb.AppendLine("Deine HP wurden vollständig wiederhergestellt.");
        }

        /// <summary>
        /// Erstellt einen zufälligen Potion-Drop. Rückgabewert kann null sein, wenn nichts droppt.
        /// </summary>
        private IInventarItem CreateRandomDrop(Entity enemy)
        {
            // Drop-Chance: ca. 70 %
            if (_rng.NextDouble() > 0.7)
                return null;

            int level = _rng.Next(1, 4); // ItemGroesse 1..3
            bool isHeal = _rng.Next(2) == 0;

            if (isHeal)
            {
                return new HealPotion(ItemGroesse: level);
            }

            return new PoisonPotion(ItemGroesse: level);
        }
    }
}