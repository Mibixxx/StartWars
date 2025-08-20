using UnityEngine;

public class CombatAreaTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        TestEnemyUnit enemy = other.GetComponent<TestEnemyUnit>();
        if (enemy != null)
        {
            enemy.SetInCombatArea(true);
        }

        MilitaryUnit player = other.GetComponent<MilitaryUnit>();
        if (player != null)
        {
            player.SetInCombatArea(true);
            player.SwitchToAttack();
        }

        if (other.GetComponent<TestEnemyUnit>() != null)
        {
            Debug.Log(other.name + " entered CA");
        }
        if (other.GetComponent<MilitaryUnit>() != null)
        {
            Debug.Log(other.name + " (player) entered CA");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        TestEnemyUnit enemy = other.GetComponent<TestEnemyUnit>();
        if (enemy != null)
        {
            enemy.SetInCombatArea(false);
        }
    }
}
