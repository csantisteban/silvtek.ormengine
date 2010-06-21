using System;
using System.IO;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.XmlMapping;

namespace Silvtek.ORMEngine.Schema
{
    internal sealed class ObjectSchemaInfoLoader
    {
 
        private ObjectSchemaInfoLoader() 
        {

        }
   
        private static string FindFile(Type source, string mapRoot)
        {
            string fullpath = String.Format(@"{0}\{1}.xml", mapRoot, source.FullName);

            if (File.Exists(fullpath))
            {
                return fullpath;
            }
            else
            {
                return String.Empty;
            }

        }

        internal static ObjectSchemaInfo GetLoader(Type source)
        {
            string mapRoot = ORMConfigurator.MapFiles;
            string filePath = FindFile(source, mapRoot);

            ObjectSchemaInfo info = null;

            if (filePath.Length > 0)
            {
                info = new ObjectXmlSchemaInfo(source, filePath);
                info.Load();

                string exMsg = String.Format("Object '{0}' is missing xml map file: {1}. ", source.FullName, filePath);
                throw new ORMException(exMsg);
            }
            else
            {
                info = new ObjectAttributesSchemaInfo(source);
                info.Load();

                if (!info.IsValid)
                {
                    string exMsg = String.Format("Object '{0}' is missing mapping attributes. Check if your class is tagged with Table attribute. e.g: [Table(\"MyTable\", \"MyConnection\")]", source.FullName);
                    throw new ORMException(exMsg);
                }
            }


            return info;
            
        }

    }
}
