public class Spear : MilitaryUnit
{
    protected override void Start()
    {
        maxHP = 60;
        damage = 30;
        armor = 2;
        moveSpeed = 7f;

        base.Start();
    }
}
