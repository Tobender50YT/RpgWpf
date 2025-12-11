namespace RpgWpf.GameCore
{
    /// <summary>
    /// Basis Klasse für Tränke. Konkrete Wirkung wird in abgeleiteten Klassen implementiert.
    /// </summary>
    public class Potion : IInventarItem
    {
        public int InventarGroesse { get; }
        public string ItemName { get; }

        public Potion(int itemGroesse, string name)
        {
            ItemName = name;
            InventarGroesse = Math.Max(1, itemGroesse);
        }

        /// <summary>
        /// Default Verhalten – sollte überschrieben werden. Gibt false (nicht verbraucht) zurück.
        /// </summary>
        public virtual bool useItem(Entity entity)
        {
            Console.WriteLine("Potiontyp unbekannt.");
            return false;
        }

        /// <summary>
        /// Wendet das Item an (Standard-Implementierung).
        /// </summary>
        /// <param name="user">Die Entity, die das Item einsetzt.</param>
        /// <param name="target">Optionales Ziel; wenn nicht angegeben, wird das Item auf den Anwender selbst angewendet.</param>
        /// <returns>Eine Erfolgs- oder Fehlermeldung der Item-Anwendung.</returns>
        public virtual string Use(Entity user, Entity? target)
        {
            if (user == null)
            {
                return "Kein Anwender für das Item angegeben.";
            }
            Entity targetEntity = target ?? user;
            bool consumed = useItem(targetEntity);
            if (consumed)
            {
                if (user is Charakter c)
                {
                    c.Inventar.Remove(this);
                }
                return $"{ItemName} wurde verwendet.";
            }
            return $"{ItemName} hatte keinen Effekt.";
        }
    }
}
