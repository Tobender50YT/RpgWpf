namespace RpgWpf.GameCore
{
    /// <summary>
    /// Vertrag für Inventar Items. Rückgabe von useItem: true, wenn das Item benutzt/verbraucht wurde.
    /// </summary>
    public interface IInventarItem
    {
        int ItemGroesse { get; }
        string ItemName { get; }
        bool useItem(Entity entity);
    }
}
