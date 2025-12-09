namespace RpgWpf.GameCore
{
    /// <summary>
    /// Einfacher Gegner-Typ Slime mit wenig HP und geringem Schaden.
    /// </summary>
    public class Slime : Entity
    {
        public Slime(string name = "Slime")
            : base(name, startHP: 150, baseAttack: 2)
        {
        }
    }
}
