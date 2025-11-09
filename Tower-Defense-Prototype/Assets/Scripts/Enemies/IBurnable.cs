// Interface for enemies that can receive a burn (damage-over-time) effect.

namespace TowerDefense.Enemies
{
    public interface IBurnable : IEnemy
    {
        void ApplyBurn(int damagePerTick, float duration, float tickInterval);
    }
}

