using System;

namespace RpgWpf.GameCore
{
    /// <summary>
    /// Spieler Charakter inkl. Inventar, Spezialangriff-Logik und Level/Erfahrungssystem.
    /// </summary>
    public class Charakter : Entity
    {
        private readonly Random _rng = new Random();

        public Inventar Inventar { get; }
        public string Vorname { get; }
        public string PlayerTag { get; }
        public int Alter { get; }

        /// <summary>
        /// Kennzeichnet, ob dieser Charakter Admin-Rechte hat.
        /// </summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary> Multiplikator für Spezialangriff (x facher Schaden). </summary>
        public int DamageMultiplier { get; private set; } = 5;

        /// <summary>
        /// Chance in Prozent (0..100), dass der Spezialangriff in dieser Runde triggert.
        /// </summary>
        public int SpecialAttackChancePercent { get; set; } = 20; // vormals "5" via Next(5)==0

        // ============================
        //   Level- und Erfahrungssystem
        // ============================

        /// <summary> Aktuelles Spielerlevel (mindestens 1). </summary>
        public int Level { get; private set; }

        /// <summary> Aktuell gesammelte Erfahrungspunkte im aktuellen Level. </summary>
        public int CurrentExp { get; private set; }

        /// <summary> Benötigte Erfahrungspunkte bis zum nächsten Level. </summary>
        public int ExpToNextLevel { get; private set; }

        public Charakter(string vorname, string playerTag, int alter, int invSize)
            : base(playerTag)
        {
            Vorname = vorname;
            PlayerTag = playerTag;
            Alter = alter;
            Inventar = new Inventar(invSize);

            // Startwerte
            SetBaseAttack(5);
            DamageMultiplier = 5;

            // Level-/EXP-Startwerte
            Level = 1;
            CurrentExp = 0;
            ExpToNextLevel = 100;
        }

        /// <summary> Berechnet den Schaden einer möglichen Spezialattacke. </summary>
        public double GetSpecialAttackDamage() => GetAttackDamage() * DamageMultiplier;

        /// <summary>
        /// Simuliert die Chance (in Prozent), ob die Spezialattacke in dieser Runde ausgelöst wird.
        /// </summary>
        public bool RollSpecialAttack()
        {
            int roll = _rng.Next(100);          // 0..99
            return roll < SpecialAttackChancePercent;
        }

        public void SetDamageMultiplier(int value)
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

                // Einfache Progression: nächstes Level benötigt ca. 30 % mehr EXP
                ExpToNextLevel = (int)(ExpToNextLevel * 1.3);
                if (ExpToNextLevel < 1)
                {
                    ExpToNextLevel = 1;
                }
            }

            if (leveledUp)
            {
                // HP nach Level-Up vollständig auffüllen
                SetHP(MaxHP);
            }

            return leveledUp;
        }

        // ==== Abwärtskompatible Wrapper ====

        [Obsolete("Nutze stattdessen Vorname")] public string getVorname() => Vorname;
        [Obsolete("Nutze stattdessen PlayerTag")] public string getplayer_tag() => PlayerTag;
        [Obsolete("Nutze stattdessen Alter")] public int getAlter() => Alter;
        [Obsolete("Nutze stattdessen GetSpecialAttackDamage()")] public double getSpecialAttack_dmg() => GetSpecialAttackDamage();
        [Obsolete("Nutze stattdessen RollSpecialAttack()")] public bool SpecialAttack() => RollSpecialAttack();
        [Obsolete("Nutze stattdessen DamageMultiplier")] public int getdmg_Multiplier() => DamageMultiplier;
        [Obsolete("Nutze stattdessen SetDamageMultiplier(int)")] public void setdmg_Multiplier(int v) => SetDamageMultiplier(v);

        // Inventar Property für alte Aufrufer
        [Obsolete("Nutze stattdessen Inventar")] public Inventar inventar => Inventar;
        [Obsolete("Nutze stattdessen PlayerTag")] public string player_tag => PlayerTag;
    }
}
