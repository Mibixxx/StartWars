public class PikeUnit : MilitaryUnit
{
    protected override void Start()
    {
        maxHP = 60;
        damage = 30;
        armor = 2;
        moveSpeed = 1.2f;

        base.Start();
    }
}
