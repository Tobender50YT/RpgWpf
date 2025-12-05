namespace RpgWpf.GameCore
{
    public class PoisonPotion : Potion
    {
        private readonly double _damage;

        public PoisonPotion(int ItemGroesse = 1, string Name = "PoisonPotion", double HP = 10)
            : base(ItemGroesse, Name)
        {
            _damage = HP;
        }

        public override bool useItem(Entity entity)
        {
            if (entity == null) return false;
            if (entity.IsDead) return false; // Ziel schon tot → nicht verbrauchen

            double dmg = _damage * InventarGroesse;
            entity.TakeDamage(dmg);
            return true; // Trank wurde eingesetzt
        }
    }
}
