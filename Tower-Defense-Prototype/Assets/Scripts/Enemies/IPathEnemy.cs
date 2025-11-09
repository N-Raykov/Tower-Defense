namespace TowerDefense.Enemies
{
    public interface IPathEnemy : IEnemy
    {
        float PathProgress { get; }
    }
}
