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

        /// <summary>
        /// Wendet den Gifttrank auf ein Ziel an und fügt diesem Schaden zu.
        /// </summary>
        /// <param name="user">Die Entity, die den Trank einsetzt (typischerweise der Charakter).</param>
        /// <param name="target">Das Ziel, das durch den Gifttrank verletzt werden soll.</param>
        /// <returns>Eine Meldung über den Erfolg oder Misserfolg der Anwendung.</returns>
        public override string Use(Entity user, Entity? target)
        {
            if (user == null)
            {
                return "Kein Anwender für den Gifttrank angegeben.";
            }
            if (target == null)
            {
                return "Für PoisonPotion muss ein Ziel ausgewählt werden.";
            }
            bool used = this.useItem(target);
            if (!used)
            {
                return "PoisonPotion konnte nicht angewendet werden.";
            }
            if (user is Charakter c)
            {
                c.Inventar.Remove(this);
            }
            return $"{target.Name} wurde durch eine PoisonPotion verletzt.";
        }
    }
}
