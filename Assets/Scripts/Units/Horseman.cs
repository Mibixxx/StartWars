public class Horseman : MilitaryUnit
{
    protected override void Start()
    {
        maxHP = 100;
        damage = 30;
        armor = 2;
        moveSpeed = 7f;

        base.Start();
    }
}
