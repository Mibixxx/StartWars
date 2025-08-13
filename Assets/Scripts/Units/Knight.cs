public class Knight : MilitaryUnit
{
    public override void Spawned()
    {
        maxHP = 100;
        damage = 30;
        armor = 2;
        moveSpeed = 7f;

        base.Spawned();
    }
}
