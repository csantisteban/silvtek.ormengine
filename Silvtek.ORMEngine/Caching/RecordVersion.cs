#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Silvtek.ORMEngine.Configs;
using System.IO;

namespace Silvtek.ORMEngine.Caching
{
    [Serializable]
    internal class RecordVersion
    {

        internal enum ModificationType : byte
        {
            Create,
            Update,
            Delete,
            None,
        }

        private readonly string recordIdentifier;
        private int currentVersion;
        private ModificationType modificationType;


        internal int VersionNumber
        {
            get { return currentVersion; }
        }

        internal string RecordIdentifier
        {
            get { return recordIdentifier; }
        }

        internal ModificationType Type
        {
            get { return modificationType; }
        }

        internal RecordVersion(string identifier, int version)
        {
            recordIdentifier = identifier;
            currentVersion = version;
            modificationType = ModificationType.None;
        }

       
        private void IncrementVersion()
        {
            currentVersion++;
            modificationType = ModificationType.Update;
        }

        private void DeletedVersion()
        {
            currentVersion = -99;
            modificationType = ModificationType.Delete;
        }

        private void CreatedVersion()
        {
            currentVersion = 0;
            modificationType = ModificationType.Create;
        }

        internal static void Create(ORMObject obj, RecordVersion version)
        {
            if (version == null) return;
            if (ORMConfigurator.BalancerShare.Length > 0)
            {
                version.CreatedVersion();
                SaveRecordVersionToFile(obj, version);
            }
        }

        internal static void Update(ORMObject obj, RecordVersion version)
        {
            if (version == null)return;
            version.IncrementVersion();
            if (ORMConfigurator.BalancerShare.Length > 0)
            {
                SaveRecordVersionToFile(obj, version);
            }
        }

        internal static void Delete(ORMObject obj, RecordVersion version)
        {
            if (version == null) return;
            if (ORMConfigurator.BalancerShare.Length > 0)
            {
                version.DeletedVersion();
                SaveRecordVersionToFile(obj, version);
            }
        }


        private static void SaveRecordVersionToFile(ORMObject obj, RecordVersion version)
        {
            if (obj == null) return;

            string file = GetFileName(obj);
            Stream serializationStream = null;

            try
            {
                serializationStream = new FileStream(file, FileMode.Create, FileAccess.Write);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(serializationStream, version);
                serializationStream.Flush();
            }
            catch (ArgumentNullException exception)
            {
                throw new SerializationException("Failed to create an stream handle for the file '" + file + "'", exception);
            }

            finally
            {
                if (serializationStream != null)
                {
                    serializationStream.Close();
                }
            }
        }
      
        internal static RecordVersion GetCurrentVersionFromSharedCachedFile(object obj)
        {
            if (obj == null) return null;
            string file = GetFileName(obj);
            
            if (!File.Exists(file))
            {
                return null;
            }

            Stream serializationStream = null;
            object versionObj = null;
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                serializationStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                versionObj = formatter.Deserialize(serializationStream);
            }
            catch (ArgumentNullException exception)
            {
                throw new SerializationException("Failed to create an stream from the file '" + file + "'", exception);
            }
            finally
            {
                if (serializationStream != null)
                {
                    serializationStream.Close();
                }
            }

            return (RecordVersion)versionObj;
  
        }

        private static RecordVersion GetCurrentVersionFromDatabase(ORMObject obj)
        {
            if (obj == null) return null;
            ORMObject clone = (ORMObject) obj.Clone();
            clone.Refresh();
            return clone.GetRecordVersion();
        }

        
        internal static RecordVersion GetCurrentVersion(ORMObject obj)
        {
            if (obj == null) return null;
            if (ORMConfigurator.BalancerShare.Length == 0 && !obj.ObjectInfo.MappedMembers.HasDigestRecordMember) return null;

            if (ORMConfigurator.BalancerShare.Length > 0 )
            {
                return GetCurrentVersionFromSharedCachedFile(obj);
            }
            
            if (obj.ObjectInfo.MappedMembers.HasDigestRecordMember)
            {
                return GetCurrentVersionFromDatabase(obj);
            }

            return null;
        }

        private static string GetFileName(object obj)
        {
            if (obj == null) return String.Empty;

            ORMObject orm = null;
            if (obj is ORMObject)
            {
                orm = (ORMObject)obj;
            }
            else
            {
                orm = ORM.Instanciate(obj);
            }

            string filename = Convert.ToString(orm.GetBagValue("RecordVersionFile"));
            if (filename != null  && filename.Length > 0) return filename;


            string identifier = orm.GetRecordIdentifier();

            if (identifier.Length < 2) return String.Empty;

            string prefix = identifier.Substring(0, 2);
            filename = String.Format(@"{0}\{1}\{2}.{3}", ORMConfigurator.BalancerShare, prefix, identifier, GlobalConstants.ORM_VERSION_CONTROL_EXTENSION);

            if (!Directory.Exists(Path.GetDirectoryName(filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }

            orm.AddBagValue("RecordVersionFile", filename);
            return filename;
        }


    }
}
