namespace RpgWpf.GameCore
{
    /// <summary>
    /// Mittlerer Gegner-Typ Elf mit hohen HP, aber geringem Schaden.
    /// </summary>
    public class Elf : Entity
    {
        public Elf(string name = "Elf") : base(name, startHP: 400)
        {
        }
    }
}
