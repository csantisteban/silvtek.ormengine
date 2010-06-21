namespace Silvtek.ORMEngine
{
    public interface IORMObject
    {
        /// <summary>
        /// Tells whether this object has been loaded 
        /// </summary>
        bool IsLoaded { get; set;}
    }
}
