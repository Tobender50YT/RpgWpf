namespace RpgWpf.GameCore
{
    public class Werwolf : Entity
    {
        public Werwolf(string name = "Werwolf") : base(name, startHP: 600)
        {
        }

        public override double GetAttackDamage() => BaseAttackDamage * 3;
    }
}
