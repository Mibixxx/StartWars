public class Infantry_Networked : MilitaryUnit_Networked
{
    public override void Spawned()
    {
        maxHP = 100;
        damage = 20;
        armor = 5;
        moveSpeed = 7f;
        attackRange = 4f;

        base.Spawned();
    }
}
