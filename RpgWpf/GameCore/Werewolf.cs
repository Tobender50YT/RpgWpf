namespace RpgWpf.GameCore
{
    /// <summary>
    /// Starker Gegner-Typ Werewolf mit vielen HP und sehr hohem Schaden.
    /// </summary>
    public class Werewolf : Entity
    {
        public Werewolf(string name = "Werewolf") : base(name, startHP: 600)
        {
        }

        /// <summary>
        /// Werewolf verursacht dreifachen Basis-Schaden.
        /// </summary>
        public override double GetAttackDamage() => BaseAttackDamage * 3;
    }
}
