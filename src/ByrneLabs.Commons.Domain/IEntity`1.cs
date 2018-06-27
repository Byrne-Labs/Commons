namespace ByrneLabs.Commons.Domain
{
    public interface IEntity<out T> : IEntity, ICloneable<T> where T : IEntity<T>
    {
    }
}
