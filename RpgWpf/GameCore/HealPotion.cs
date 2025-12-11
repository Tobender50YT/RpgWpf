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

        /// <summary>
        /// Wendet den Heiltrank auf den Anwender an und stellt Lebenspunkte wieder her.
        /// </summary>
        /// <param name="user">Die Entity, die den Trank einsetzt (typischerweise der Charakter).</param>
        /// <param name="target">Ein optionales Ziel (nicht benötigt für HealPotion).</param>
        /// <returns>Eine Meldung über den Erfolg oder Misserfolg der Heilung.</returns>
        public override string Use(Entity user, Entity? target)
        {
            if (user == null)
            {
                return "Kein Anwender für den Heiltrank angegeben.";
            }
            bool used = this.useItem(user);
            if (!used)
            {
                return $"{ItemName} hatte keinen Effekt (HP vermutlich bereits voll).";
            }
            if (user is Charakter c)
            {
                c.Inventar.Remove(this);
            }
            return $"{ItemName} wurde auf den Charakter angewendet.";
        }
    }
}
