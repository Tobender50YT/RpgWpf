using System;

namespace RpgWpf.GameCore
{
    /// <summary>
    /// Basisklasse für alle Einheiten (Spieler & Gegner). Enthält HP Verwaltung
    /// und Standard Angriffswerte. Bewusst schlank gehalten (reine Spiel Logik).
    /// </summary>
    public abstract class Entity
    {
        /// <summary> Anzeigename der Entity. </summary>
        public string Name { get; }

        /// <summary> Aktuelle Lebenspunkte (0..MaxHP). </summary>
        public double HP { get; private set; }

        /// <summary> Maximale Lebenspunkte. </summary>
        public double MaxHP { get; private set; }

        /// <summary> Basis Schaden, den die Entity pro normalem Angriff verursacht. </summary>
        public double BaseAttackDamage { get; private set; }

        /// <summary> True, wenn HP == 0. </summary>
        public bool IsDead => HP <= 0;

        protected Entity(string name, double startHP = 200000, double baseAttack = 3)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MaxHP = Math.Max(1, startHP);
            HP = MaxHP;
            BaseAttackDamage = Math.Max(0, baseAttack);
        }

        /// <summary>
        /// Gibt den aktuellen Angriffs Schaden zurück. Gegner können dies überschreiben
        /// (z. B. Werwolf macht 3x Basis Schaden).
        /// </summary>
        public virtual double GetAttackDamage() => BaseAttackDamage;

        /// <summary>
        /// Wendet Schaden an. Gibt true zurück, wenn die Entity dadurch stirbt.
        /// </summary>
        public bool TakeDamage(double damage)
        {
            if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            HP = Math.Max(0, HP - damage);
            return HP == 0;
        }

        /// <summary>
        /// Heilt die Entity um einen Betrag. Gibt true zurück, wenn Heilung angewandt wurde
        /// (d. h. HP < MaxHP). Verbrauchslogik für Items kann dieses Ergebnis nutzen.
        /// </summary>
        public bool Heal(double amount)
        {
            if (amount <= 0) return false;
            if (HP >= MaxHP) return false; // schon voll
            HP = Math.Min(MaxHP, HP + amount);
            return true;
        }

        /// <summary> Setzt die maximalen Lebenspunkte (clamped) und passt HP ggf. an. </summary>
        public void SetMaxHP(double value)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            MaxHP = value;
            if (HP > MaxHP) HP = MaxHP;
        }

        /// <summary> Setzt die aktuellen Lebenspunkte (0..MaxHP). </summary>
        public void SetHP(double value)
        {
            HP = Math.Max(0, Math.Min(MaxHP, value));
        }

        /// <summary> Setzt die Basis Angriffsstärke (>= 0). </summary>
        public void SetBaseAttack(double value)
        {
            BaseAttackDamage = Math.Max(0, value);
        }

        // ==== Abwärtskompatible Wrapper (alte API Namen) ====
        [Obsolete("Nutze stattdessen MaxHP")] public double getVollHP() => MaxHP;
        [Obsolete("Nutze stattdessen SetMaxHP(double)")] public void setVollHP(double hp) => SetMaxHP(hp);
        [Obsolete("Nutze stattdessen BaseAttackDamage")] public double getUrspAttack_dmg() => BaseAttackDamage;
        [Obsolete("Nutze stattdessen SetHP(double)")] public void setHP(double hp) => SetHP(hp);
        [Obsolete("Nutze stattdessen HP")] public double getHP() => HP;
        [Obsolete("Nutze stattdessen SetBaseAttack(double)")] public void setAttack_dmg(double dmg) => SetBaseAttack(dmg);
        [Obsolete("Nutze stattdessen GetAttackDamage()")] public virtual double getAttack_damage() => GetAttackDamage();
        [Obsolete("Nutze stattdessen TakeDamage(double)")] public bool getDamage(double dmg) => TakeDamage(dmg);
    }
}
