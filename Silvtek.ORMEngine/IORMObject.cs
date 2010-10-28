using System.Xml.Serialization;

namespace Silvtek.ORMEngine
{
    public interface IORMObject
    {
        /// <summary>
        /// Tells whether this object has been loaded 
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        bool IsLoaded { get; set;}
    }
}
