namespace PalaceOfFantasy.FlockForge.Core
{
    public interface IBoid
    {
        int Id { get; }
        string Tag { get; set; }
        bool IsActive { get; set; }

        FVector3 Position { get; set; }
        FVector3 Velocity { get; set; }
        FVector3 Forward { get; }

        IFlock Flock { get; set; }
        IBoidSettings Settings { get; }

        void AddForce(FVector3 force);
        void ApplyForces(float deltaTime);
        FVector3 AccumulatedForce { get; }
    }
}
