namespace PalaceOfFantasy.FlockForge.Core
{
    public interface IBehaviour
    {
        string Name { get; }
        bool IsEnabled { get; set; }
        float Weight { get; set; }

        FVector3 CalculateForce(IBoidContext context);
    }
}
