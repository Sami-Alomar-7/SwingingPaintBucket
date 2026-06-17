namespace SwingingPaintBucket.Features.Paint.Interfaces
{
    public interface IMassLossNotifier
    {
        /// <summary>
        /// يتم استدعاؤها لتنبيه نظام البندول بنقصان كتلة السطل عند خروج كل جسيم
        /// </summary>
        void NotifyParticleEmitted(float particleMass);
    }
}