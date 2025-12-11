namespace RpgWpf.GameCore
{
    /// <summary>
    /// Basis Klasse für Tränke. Konkrete Wirkung wird in abgeleiteten Klassen implementiert.
    /// </summary>
    public class Potion : IInventarItem
    {
        public int ItemGroesse { get; }
        public string ItemName { get; }

        public Potion(int itemGroesse, string name)
        {
            ItemName = name;
            ItemGroesse = Math.Max(1, itemGroesse);
        }

        /// <summary>
        /// Default Verhalten – sollte überschrieben werden. Gibt false (nicht verbraucht) zurück.
        /// </summary>
        public virtual bool useItem(Entity entity)
        {
            Console.WriteLine("Potiontyp unbekannt.");
            return false;
        }
    }
}
