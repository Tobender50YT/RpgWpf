namespace RpgWpf.GameCore
{
    /// <summary>
    /// Vertrag für Inventar Items. Rückgabe von useItem: true, wenn das Item benutzt/verbraucht wurde.
    /// </summary>
    public interface IInventarItem
    {
        int InventarGroesse { get; }
        string ItemName { get; }
        bool useItem(Entity entity);

        /// <summary>
        /// Wendet das Item an (inklusive aller Effekte).
        /// </summary>
        /// <param name="user">Die Entity, die das Item benutzt.</param>
        /// <param name="target">Optionales Ziel der Item-Wirkung (falls benötigt).</param>
        /// <returns>Eine Meldung, die den Ausgang der Verwendung beschreibt.</returns>
        string Use(Entity user, Entity? target);
    }
}
