public class Spear_Networked : MilitaryUnit_Networked
{
    public override void Spawned()
    {
        maxHP = 60;
        damage = 30;
        armor = 2;
        moveSpeed = 7f;
        attackRange = 4f;

        base.Spawned();
    }
}