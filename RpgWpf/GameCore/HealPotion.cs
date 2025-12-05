namespace RpgWpf.GameCore
{
    public class HealPotion : Potion
    {
        private readonly double _healAmount;

        public HealPotion(int ItemGroesse = 1, string Name = "HealPotion", double HP = 50)
            : base(ItemGroesse, Name)
        {
            _healAmount = HP;
        }

        public override bool useItem(Entity entity)
        {
            if (entity == null) return false;
            // Heilung skaliert mit Inventar Größe
            return entity.Heal(_healAmount * InventarGroesse);
        }
    }
}
