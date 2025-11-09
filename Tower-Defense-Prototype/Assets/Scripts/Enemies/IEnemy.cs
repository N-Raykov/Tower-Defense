// Interface for enemy behaviours, used as an integration point
// so other systems (towers, spawners) don't depend on concrete enemy implementations.

namespace TowerDefense.Enemies
{

    public interface IEnemy
    {
        bool IsAlive { get; }

        void TakeDamage(int amount);
    }
}

