public class Infantry : MilitaryUnit
{
    protected override void Start()
    {
        maxHP = 100;
        damage = 20;
        armor = 5;
        moveSpeed = 7f;
        attackRange = 4f;

        base.Start();
    }
}
