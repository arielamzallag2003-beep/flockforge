using System.Collections.Generic;

namespace PalaceOfFantasy.FlockForge.Core
{
    public interface IFlockSettings
    {
        bool UseFixedTimestep { get; }
        float FixedTimestep { get; }
        IReadOnlyList<IBehaviour> DefaultBehaviours { get; }
    }
}
