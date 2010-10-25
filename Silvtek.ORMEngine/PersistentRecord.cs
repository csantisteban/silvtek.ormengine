using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine
{
    [Serializable]
    internal class PersistentRecord :  ISerializable
    {
        private readonly Type type;
        private readonly string recordIdentifier;
        private readonly string hash;
       
        private readonly ORMObject obj;
        private Hashtable values;

        private string digestValue;

        internal object this[string name]
        {
            get { return values[name]; }
        }

        internal PersistentRecord(ORMObject obj)
        {
            values = new Hashtable();
            this.obj = obj;
    
            recordIdentifier = obj.GetRecordIdentifier();

            hash = obj.GetDigest();
            type = obj.SourceObject.GetType();

            if (obj.ObjectInfo.MappedMembers.HasDigestRecordMember)
            {
                foreach (MapMember member in obj.ObjectInfo.MappedMembers)
                {
                    if (member.Properties.HasRecordDigestProvider)
                    {
                        digestValue = Convert.ToString(member.GetMemberValue(obj.SourceObject));
                        break;
                    }
                }
            }
     

        }

        public Type Type
        {
            get { return type; }
        }

        public string RecordIdentifier
        {
            get { return recordIdentifier; }
        }

        public string Hash
        {
            get { return hash; }
        }

        public string DigestValue
        {
            get { return digestValue; }
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArrayList tmpList = new ArrayList();
            foreach (MemberField field in obj.FieldList)
            {
                if (field == null) continue;

                tmpList.Add(field);

            }

            for(int i = 0; i < tmpList.Count; i++)
            {

                NameValueItem item = new NameValueItem();
                MemberField field = (MemberField)tmpList[i];

                //if (field.MapMember.Properties.DataType == DataType.Binary || field.MapMember.Properties.DataType == DataType.Object)
                //{
                  
                //}

                item.Name = field.Name;
                item.Value = field.ActualValue;
                
                info.AddValue("record:" + i, item);

                
            }

            info.AddValue("fieldcount", tmpList.Count);
            info.AddValue("recordhash", Hash);
            info.AddValue("recordIdentifier", RecordIdentifier);
            info.AddValue("type", Type);
            info.AddValue("digestValue", DigestValue);

        }

        public  PersistentRecord(SerializationInfo info, StreamingContext context)
        {
            int fieldCount = info.GetInt32("fieldcount");
            ArrayList tmplist = new ArrayList(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                NameValueItem item = (NameValueItem)info.GetValue("record:" + i, typeof(NameValueItem));
                if (item == null) continue;
                tmplist.Add(item);

            }

            hash = info.GetString("recordhash");
            recordIdentifier = info.GetString("recordIdentifier");
            type = (Type)info.GetValue("type", typeof(Type));
            digestValue = info.GetString("digestValue");
            values = new Hashtable();

            foreach (NameValueItem item in tmplist)
            {
                values.Add(item.Name, item.Value);
            }
        }
    }
}
