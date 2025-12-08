namespace RpgWpf.GameCore
{
    /// <summary>
    /// Starker Gegner-Typ Dragon mit vielen HP und hohem Schaden.
    /// </summary>
    public class Dragon : Entity
    {
        public Dragon(string name = "Dragon")
            : base(name, startHP: 900, baseAttack: 10)
        {
        }

        /// <summary>
        /// Dragon verursacht doppelten Basis-Schaden.
        /// </summary>
        public override double GetAttackDamage() => BaseAttackDamage * 2;
    }
}
