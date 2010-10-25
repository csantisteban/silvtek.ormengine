#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Silvtek.ORMEngine.Schema
{
    /// <summary>
    ///  Represent a single data object field.
    /// </summary>
    [Serializable]
    public class MemberField : ISerializable, IXmlSerializable
    {

        private readonly MapMember mapMember;

        // The original value.
        //private object beforeCommitValue;

        // this is the origiinal value before comitting to the database. T
        private object originalValue;

        // The field name (property)
        private readonly string name;

        // The new value.
        private object currentValue;

        // Whether this field had been changed.
        private bool changed;

        // Whether this field is a primary key.
        private readonly bool isPrimaryKey;
       
        internal event UpdateChangeDelegate Changed;
        private readonly object sourceObject;


        //internal event EventHandler OriginalValueChanged;

        /// <summary>
        /// Get the original value of this object. this value is the value set to this field when the ORM object is instanciated. 
        /// </summary>
        internal object OriginalValue
        {
            get { return originalValue; }
        }

        ///<summary>
        /// Gets the actual value.
        ///</summary>
        internal object ActualValue
        {
            get { return GetValue(); }
        }

        /// <summary>
        /// Gets a value indicating whether [include in updates].
        /// </summary>
        /// <value><c>true</c> if [include in updates]; otherwise, <c>false</c>.</value>
        internal bool IncludeInUpdates
        {
            get
            {
                if (mapMember.Properties.HasRecordDigestProvider) return true;
                if (mapMember.Properties.ModeType == ColumnModeType.IgnoreChanges || mapMember.Properties.ModeType == (ColumnModeType.IgnoreChanges | ColumnModeType.ExcludeFromDigest)) return true;
                return false;
            }
        }



        /// <summary>
        /// Refreshes the value from source.
        /// </summary>
        internal void RefreshValueFromSource()
        {
            object value = GetValue();

            if((mapMember.Properties.DataType == DataType.Binary || mapMember.Properties.DataType == DataType.Object))
            {
                currentValue = value;
                changed = true;
                if (Changed != null) Changed(Name);
                return;
            }


            if ((currentValue == null && value != null) || (currentValue != null && !currentValue.Equals(value)) || (value == null && currentValue != null))
            {
#if DEBUG
                string dbgMessage0 = String.Format("Field {0} is being set as changed for value: {1}", name, value);
                Debug.WriteLine(dbgMessage0);
#endif
                currentValue = value;

                if (mapMember.Properties.IsPrimaryKey) return;
                if (mapMember.Properties.HasRecordDigestProvider) return;
                if (mapMember.Properties.ModeType == ColumnModeType.IgnoreChanges ||
                    mapMember.Properties.ModeType == (ColumnModeType.IgnoreChanges | ColumnModeType.ExcludeFromDigest)) return;


                changed = true;
                try
                {
                    if (Changed != null) Changed(Name);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Changed event raised an error" + ex.Message);
                }
            }

        }

        ///<summary>
        /// Gets this fields is a primary key.
        ///</summary>
        internal bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
        }

        ///<summary>
        /// Gets the field name.
        ///</summary>
        internal string Name
        {
            get { return name; }
        }

        ///<summary>
        /// Gets whether this field had been changed.
        ///</summary>
        public bool HasChanged
        {
            get 
            {
                return changed;
            }
        }

        internal MapMember MapMember
        {
            get { return mapMember; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MemberField"/> class.
        /// </summary>
        /// <param name="mapMember">The map member.</param>
        /// <param name="sourceObject">The source object.</param>
        internal MemberField(MapMember mapMember, object sourceObject)
        {
            this.mapMember = mapMember;
            this.sourceObject = sourceObject;
            object tmpValue = GetValue();
            //beforeCommitValue = tmpValue;
            currentValue = tmpValue;
            originalValue = tmpValue;
            name = mapMember.MemberName;
            isPrimaryKey = mapMember.Properties.IsPrimaryKey;
            changed = false;
        }

        private MemberField(MemberField source, object value)
        {
            this.mapMember = source.mapMember;
            this.sourceObject = null;

            object tmpValue = value;
            //beforeCommitValue = tmpValue;
            currentValue = tmpValue;
            originalValue = tmpValue;
            name = source.name;
            isPrimaryKey = source.IsPrimaryKey;
            changed = false;
        }

        /// <summary>
        /// Creates the copy.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        internal static MemberField CreateCopy(MemberField source)
         {
             MemberField field = new MemberField(source, source.currentValue);
             field.changed = false;

             return field;

         }


        private object GetValue()
        {
            if (sourceObject == null) return null;
            return mapMember.GetMemberValue(sourceObject);
        }




        /// <summary>
        /// Commit changes.
        /// </summary>
        internal void Commit()
        {

            changed = false;
            originalValue = currentValue;
            //alreadyUpdated = false;
            //beforeCommitValue = currentValue;

            Debug.WriteLine("Commit value: "+currentValue);
        }

        //internal void RegisterUpdateNotify(UpdateChangeDelegate updateNotifier)
        //{
        //    updateChange = updateNotifier;
        //}

        #region	 -- Serialization --

        /// <summary>
        /// Serialize this object.
        /// </summary>
        /// <param name="si"></param>
        /// <param name="cxt"></param>
        public virtual void GetObjectData(SerializationInfo si, StreamingContext cxt)
        {
            si.AddValue("OriginalValue", originalValue);
            si.AddValue("NewValue", currentValue);
            si.AddValue("Name", name);
            si.AddValue("IsPK", isPrimaryKey);

        }

        /// <summary>
        /// Deserialize this object.
        /// </summary>
        /// <param name="si"></param>
        /// <param name="ctx"></param>
        protected MemberField(SerializationInfo si, StreamingContext ctx)
        {
            this.originalValue = si.GetValue("OriginalValue", typeof(Object));
            this.currentValue = si.GetValue("NewValue", typeof(Object));
            this.name = si.GetString("Name");
            this.isPrimaryKey = si.GetBoolean("IsPK");
        }

        #endregion

        /// <summary>
        ///	  Determines whether two Object instances are equal.
        /// </summary>
        /// <param name="obj">The Object to compare with the current Object. </param>
        /// <returns>true if the specified Object is equal to the current Object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (GetType() != obj.GetType()) return false;

            //cast
            MemberField field = (MemberField)obj;
            return (field.name == name);
        }

        /// <summary>
        /// Serves as a hash function for a particular type, suitable for use
        /// in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>A hash code for the current Object.</returns>
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        /// <returns>A String that represents the current Object.</returns>
        public override string ToString()
        {
            return name;
        }


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
            writer.WriteStartElement("Field");
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("isPrimaryKey", isPrimaryKey.ToString());
            
            string tmpOriginalValue = Convert.ToString(OriginalValue);
            string tmpValue = Convert.ToString(currentValue);
            bool isDifferent = (String.Compare(tmpOriginalValue, tmpValue, false) != 0);
            writer.WriteAttributeString("hasChanged", isDifferent.ToString());

            writer.WriteStartElement("OriginalValue");
            writer.WriteString(Convert.ToString(OriginalValue));
            writer.WriteEndElement();

            writer.WriteStartElement("ActualValue");
            writer.WriteString(Convert.ToString(currentValue));
            writer.WriteEndElement();


            writer.WriteEndElement();


        }
    }
}
