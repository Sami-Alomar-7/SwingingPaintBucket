namespace SwingingPaintBucket.Features.Rope.Data
{

    /// Rigid: Fixed length, pure angular physics.
    /// Nylon: Moderate elasticity, spring-damper model.
    /// Bungee: High elasticity, bouncy spring-damper model.
 
    public enum RopeType
    {
        Rigid = 0,
        Nylon = 1,
        Bungee = 2
    }
}