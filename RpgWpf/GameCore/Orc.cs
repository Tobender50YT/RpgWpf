namespace RpgWpf.GameCore
{
    /// <summary>
    /// Mittlerer Gegner-Typ Orc mit soliden HP und spürbarem Schaden.
    /// </summary>
    public class Orc : Entity
    {
        public Orc(string name = "Orc")
            : base(name, startHP: 500, baseAttack: 7)
        {
        }
    }
}
