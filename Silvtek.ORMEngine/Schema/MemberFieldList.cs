using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Silvtek.ORMEngine.Schema
{
	/// <summary>
	///  List all fields for a <see cref="ORMObject" />.
	/// </summary>	
	[Serializable]
	public class MemberFieldList :  IEnumerable, IXmlSerializable
	{

	    ///Holds the column names.
        private readonly ListDictionary list;
        private bool changed;

        /// <summary>
        /// Gets a value indicating whether this instance has changes.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
        /// </value>
        public bool HasChanges
        {
            get { return changed; }
        }

		/// <summary>
		/// Gets a <see cref="MemberField" /> by its name.
		/// </summary>
        public MemberField this[string name]
        {
          get {return Get(name); }
        }


        private MemberFieldList()
        {
            list = new ListDictionary();
        }

        ///<summary>
        /// Constructor.
        ///</summary>
        internal MemberFieldList(ORMObjectInfo info, object sourceobject)
        {
            if (sourceobject == null)
            {
                throw new ArgumentNullException("sourceobject", "source object cannot be null");
            }

            list = new ListDictionary();

            for (int i = 0; i < info.MappedMembers.Count; i++)
            {
                MapMember mapMember = info.MappedMembers[i];
                MemberField memberField = new MemberField(mapMember, sourceobject);
                memberField.Changed += new UpdateChangeDelegate(MemberFieldChanged);
                Add(memberField);
            }

        }

        void MemberFieldChanged(string memberName)
        {
            this.changed = true;
        }



        ///<summary>
        /// Adds a column and its value in this collection.
        ///</summary>
        ///<param name="field">
        /// The <see cref="MemberField" /> to add.
        ///</param>
        ///<exception cref="ORMException">
        /// If there try to add a column name that already exists.
        ///</exception>
        private void Add(MemberField field)
        {
            if (field == null) return;
           //lock(mutex)
           //{
              // field.RegisterUpdateNotify(updateChange);
               list.Add(field.Name, field);
           //}
         }

        //internal void SimpleAdd(MemberField field)
        //{
        //    list.Add(field.Name, field);
        //}

        ///<summary>
        /// Returns a <see cref="MemberField" /> for a given column.
        ///</summary>
        ///<param name="name">
        /// The column name to which the <see cref="MemberField" />
        /// belongs.
        ///</param>
        ///<returns>
        /// a <see cref="MemberField" /> that belongs to
        ///<paramref name="name" />.
        ///</returns>
        internal MemberField Get(string name)
        {
           return (MemberField)list[name];
        }

        internal ICollection Values
        {
            get { return list.Values; }
        }

        //internal bool HasChanges()
        //{
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        MemberField tmp = (MemberField) list[i];
        //        if (tmp.Changed)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        //public object Clone()
        //{
        //    MemberFieldList clon = new MemberFieldList();
        //    foreach (MemberField field in list.Values)
        //    {
        //        MemberField memberField = MemberField.CreateCopy(field);
        //        clon.list.Add(field.Name, memberField);

        //    }

        //    return clon;
        //}



        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns></returns>
        public MemberFieldList Copy()
        {
            MemberFieldList clon = new MemberFieldList();
            foreach (MemberField field in list.Values)
            {
                MemberField memberField = MemberField.CreateCopy(field);
                clon.list.Add(field.Name, memberField);

            }

            return clon;
        }


        public  IEnumerator GetEnumerator()
        {
           return list.Values.GetEnumerator();
        }

        /// <summary>
        /// Comite the changes. Set the changed status as false.
        /// </summary>
        internal void CommitChanges()
        {
            Debug.WriteLine("Changes commited");
            foreach (MemberField field in list.Values)
            {
                field.Commit();
            }

            this.changed = false;
        }

        internal void RefreshObjectSource()
        {
            foreach (MemberField field in list.Values)
            {
                field.RefreshValueFromSource();

            }
        }

        #region -- XML Serialization -- 

        /// <summary>
	    /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
	    /// </summary>
	    /// <returns>
	    /// An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
	    /// </returns>
	    public XmlSchema GetSchema()
	    {
	        return null;
	    }

	    /// <summary>
	    /// Generates an object from its XML representation.
	    /// </summary>
	    /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is deserialized. </param>
	    public void ReadXml(XmlReader reader)
	    {
	        throw new NotImplementedException();
	    }

	    /// <summary>
	    /// Converts an object into its XML representation.
	    /// </summary>
	    /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is serialized. </param>
        public void WriteXml(XmlWriter writer)
	    {
	        writer.WriteStartElement("Members");
	
	        foreach (MemberField field in list.Values)
	        {
                if (field == null) continue;
	            field.WriteXml(writer);
	        }
	        writer.WriteEndElement();
        }

        #endregion
    }
}
