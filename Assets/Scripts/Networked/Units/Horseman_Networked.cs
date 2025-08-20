public class Horseman_Networked : MilitaryUnit_Networked
{
    public override void Spawned()
    {
        maxHP = 100;
        damage = 30;
        armor = 2;
        moveSpeed = 7f;
        attackRange = 4f;

        base.Spawned();
    }
}
