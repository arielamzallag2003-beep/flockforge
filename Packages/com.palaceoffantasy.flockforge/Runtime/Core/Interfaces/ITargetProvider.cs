using System.Collections.Generic;

namespace PalaceOfFantasy.FlockForge.Core
{
    public interface ITargetProvider
    {
        FVector3? GetSeekTarget(IBoid boid);
        IReadOnlyList<FVector3> GetThreats(IBoid boid);
        IReadOnlyList<IObstacle> GetNearbyObstacles(IBoid boid);
    }
}
