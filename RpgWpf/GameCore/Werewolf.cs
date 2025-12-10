namespace RpgWpf.GameCore
{
    public class Werewolf : Entity
    {
        public Werewolf(string name = "Werewolf") : base(name, startHP: 600)
        {
        }

        public override double GetAttackDamage() => BaseAttackDamage * 3;
    }
}
