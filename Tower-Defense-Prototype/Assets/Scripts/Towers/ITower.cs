// Interface for tower behaviours, used as an integration point
// so other systems (placement, upgrades, etc.) don't depend on
// oncrete tower implementations.

namespace TowerDefense.Towers
{
    public interface ITower
    {
        float Range { get; }

        bool IsActive { get; }

        void SetActive(bool active);
    }
}
