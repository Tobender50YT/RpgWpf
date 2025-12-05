using System;

namespace RpgWpf.GameCore
{
    /// <summary>
    /// Spieler Charakter inkl. Inventar und Spezialangriff Logik.
    /// </summary>
    public class Charakter : Entity
    {
        private readonly Random _rng = new Random();

        public Inventar Inventar { get; }
        public string Vorname { get; }
        public string PlayerTag { get; }
        public int Alter { get; }

        /// <summary> Multiplikator für Spezialangriff (x facher Schaden). </summary>
        public int DamageMultiplier { get; private set; } = 5;

        /// <summary>
        /// Chance in Prozent (0..100), dass der Spezialangriff in dieser Runde triggert.
        /// </summary>
        public int SpecialAttackChancePercent { get; set; } = 20; // vormals "5" via Next(5)==0

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
