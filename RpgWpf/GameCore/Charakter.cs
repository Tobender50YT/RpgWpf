using System;
using System.Windows.Media;

namespace RpgWpf.GameCore
{
    /// <summary>
    /// Spieler-Charakter inkl. Inventar, Spezialangriff-Logik und Level-/Erfahrungssystem.
    /// </summary>
    public class Charakter : Entity
    {
        private readonly Random _rng = new Random();

        /// <summary>
        /// Inventar des Charakters.
        /// </summary>
        public Inventar Inventar { get; }

        /// <summary>
        /// Vorname des Spielers (rein informativ).
        /// </summary>
        public string Vorname { get; }

        /// <summary>
        /// Tag/Handle des Spielers.
        /// </summary>
        public string PlayerTag { get; }

        /// <summary>
        /// Alter des Spielers (rein informativ).
        /// </summary>
        public int Alter { get; }

        /// <summary>
        /// Kennzeichnet, ob dieser Charakter Admin-Rechte hat.
        /// </summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// Multiplikator für Spezialangriffe (x-facher Schaden).
        /// </summary>
        public int DamageMultiplier { get; private set; } = 5;

        /// <summary>
        /// Chance in Prozent (0..100), dass der Spezialangriff in dieser Runde triggert.
        /// </summary>
        public int SpecialAttackChancePercent { get; set; } = 20; // vormals "5" via Next(5)==0

        // ============================
        //   Level- und Erfahrungssystem
        // ============================

        /// <summary>
        /// Aktuelles Spielerlevel (mindestens 1).
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Aktuell gesammelte Erfahrungspunkte im aktuellen Level.
        /// </summary>
        public int CurrentExp { get; private set; }

        /// <summary>
        /// Benötigte Erfahrungspunkte bis zum nächsten Level.
        /// </summary>
        public int ExpToNextLevel { get; private set; }

        /// <summary>
        /// Erstellt einen neuen Charakter mit den angegebenen Basisdaten.
        /// </summary>
        /// <param name="vorname">Vorname des Spielers.</param>
        /// <param name="playerTag">Spieler-Tag/Handle.</param>
        /// <param name="alter">Alter des Spielers.</param>
        /// <param name="invSize">Maximale Inventargröße.</param>
        public Charakter(string vorname, string playerTag, int alter, int invSize)
            : base(playerTag)
        {
            Vorname = vorname;
            PlayerTag = playerTag;
            Alter = alter;
            Inventar = new Inventar(invSize);

            // Startwerte für Kampfwerte
            SetBaseAttack(5);
            DamageMultiplier = 5;

            // Level-/EXP-Startwerte
            Level = 1;
            CurrentExp = 0;
            ExpToNextLevel = CalculateExpRequirementForLevel(Level);
        }

        /// <summary>
        /// Berechnet den Schaden einer möglichen Spezialattacke.
        /// </summary>
        public double GetSpecialAttackDamage() => GetAttackDamage() * DamageMultiplier;

        /// <summary>
        /// Simuliert die Chance (in Prozent), ob die Spezialattacke in dieser Runde ausgelöst wird.
        /// </summary>
        public bool RollSpecialAttack()
        {
            int roll = _rng.Next(100); // 0..99
            return roll < SpecialAttackChancePercent;
        }

        /// <summary>
        /// Setzt den Multiplikator für Spezialangriffe.
        /// </summary>
        /// <param name="value">Neuer Multiplikator (mindestens 1).</param>
        public void SetDamageMultiplierFromAdmin(int value)
        {
            DamageMultiplier = Math.Max(1, value);
        }

        /// <summary>
        /// Fügt Erfahrungspunkte hinzu und prüft auf Level-Ups.
        /// Bei mindestens einem Level-Up werden die Lebenspunkte vollständig aufgefüllt.
        /// </summary>
        /// <param name="amount">Anzahl der vergebenen Erfahrungspunkte.</param>
        /// <returns>True, wenn mindestens ein Level-Up stattgefunden hat.</returns>
        public bool GainExp(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            CurrentExp += amount;
            bool leveledUp = false;

            // Mehrere Level-Ups hintereinander ermöglichen (bei vielen EXP auf einmal)
            while (CurrentExp >= ExpToNextLevel)
            {
                CurrentExp -= ExpToNextLevel;
                Level++;
                leveledUp = true;

                // EXP-Anforderung für das nächste Level anhand der Progression neu berechnen
                ExpToNextLevel = CalculateExpRequirementForLevel(Level);
            }

            if (leveledUp)
            {
                // HP nach Level-Up vollständig auffüllen
                SetHP(MaxHP);
            }

            return leveledUp;
        }

        // ============================
        //   Admin-Helfer
        // ============================

        /// <summary>
        /// Setzt das Spielerlevel direkt (Admin-Funktion).
        /// EXP werden auf 0 zurückgesetzt, die EXP-Anforderung für das nächste Level
        /// wird anhand der Progression neu berechnet und die HP voll aufgefüllt.
        /// </summary>
        /// <param name="level">Ziel-Level (wird mindestens 1 gesetzt).</param>
        public void SetLevelFromAdmin(int level)
        {
            if (level < 1)
            {
                level = 1;
            }

            Level = level;
            CurrentExp = 0;
            ExpToNextLevel = CalculateExpRequirementForLevel(Level);

            // HP wie bei normalen Level-Ups voll auffüllen
            SetHP(MaxHP);
        }

        /// <summary>
        /// Setzt die aktuellen Erfahrungspunkte direkt (Admin-Funktion).
        /// Der Wert wird auf den Bereich 0..ExpToNextLevel begrenzt.
        /// </summary>
        /// <param name="currentExp">Ziel-EXP.</param>
        public void SetCurrentExpFromAdmin(int currentExp)
        {
            if (currentExp < 0)
            {
                currentExp = 0;
            }

            if (currentExp > ExpToNextLevel)
            {
                currentExp = ExpToNextLevel;
            }

            CurrentExp = currentExp;
        }

        /// <summary>
        /// Setzt die Spezialangriffs-Chance in Prozent (Admin-Funktion).
        /// </summary>
        /// <param name="percent">Prozentwert im Bereich 0..100.</param>
        public void SetSpecialAttackChancePercentFromAdmin(int percent)
        {
            if (percent < 0)
            {
                percent = 0;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            SpecialAttackChancePercent = percent;
        }

        /// <summary>
        /// Berechnet die benötigten EXP für das nächste Level anhand einer einfachen Progression.
        /// Startwert für Level 1 = 100, jedes weitere Level erhöht die Anforderung um ca. 30 %.
        /// </summary>
        /// <param name="level">Ziel-Level (mindestens 1).</param>
        /// <returns>EXP-Anforderung für den Sprung von <paramref name="level"/> auf <paramref name="level"/>+1.</returns>
        private static int CalculateExpRequirementForLevel(int level)
        {
            if (level <= 1)
            {
                return 100;
            }

            double required = 100;

            // Progression wird so berechnet, wie sie in GainExp inkrementell aufgebaut wird:
            // Start bei 100, für jedes Level: ExpToNextLevel = (int)(ExpToNextLevel * 1.3)
            for (int i = 1; i < level; i++)
            {
                required = (int)(required * 1.3);
                if (required < 1)
                {
                    required = 1;
                }
            }

            return (int)required;
        }

        // ============================
        //   Abwärtskompatible Wrapper
        // ============================

        [Obsolete("Nutze stattdessen Vorname")]
        public string getVorname() => Vorname;

        [Obsolete("Nutze stattdessen PlayerTag")]
        public string getplayer_tag() => PlayerTag;

        [Obsolete("Nutze stattdessen Alter")]
        public int getAlter() => Alter;

        [Obsolete("Nutze stattdessen GetSpecialAttackDamage()")]
        public double getSpecialAttack_dmg() => GetSpecialAttackDamage();

        [Obsolete("Nutze stattdessen RollSpecialAttack()")]
        public bool SpecialAttack() => RollSpecialAttack();

        [Obsolete("Nutze stattdessen DamageMultiplier")]
        public int getdmg_Multiplier() => DamageMultiplier;

        [Obsolete("Nutze stattdessen SetDamageMultiplier(int)")]
        public void setdmg_Multiplier(int v) => SetDamageMultiplierFromAdmin(v);

        // Inventar Property für alte Aufrufer
        [Obsolete("Nutze stattdessen Inventar")]
        public Inventar inventar => Inventar;

        [Obsolete("Nutze stattdessen PlayerTag")]
        public string player_tag => PlayerTag;
    }
}