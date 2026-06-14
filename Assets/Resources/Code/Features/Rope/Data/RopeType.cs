namespace SwingingPaintBucket.Features.Rope.Data
{
    /// <summary>
    /// Defines the elasticity model for the pendulum rope.
    /// Rigid: Fixed length, pure angular physics.
    /// Nylon: Moderate elasticity, spring-damper model.
    /// Bungee: High elasticity, bouncy spring-damper model.
    /// </summary>
    public enum RopeType
    {
        Rigid = 0,
        Nylon = 1,
        Bungee = 2
    }
}
