public class SoldierUnit : MilitaryUnit
{
    protected override void Start()
    {
        maxHP = 100;
        damage = 20;
        armor = 5;
        moveSpeed = 1f;

        base.Start();
    }
}
