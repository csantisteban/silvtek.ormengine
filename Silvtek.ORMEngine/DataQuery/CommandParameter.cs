using System;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery
{
    internal class CommandParameter
    {
        private readonly string name;
        private readonly string parameterID;
        private MapMember mapMember;
        private object value;
        private DbTypeFormatter formatter;


        internal CommandParameter(DbTypeFormatter formatter, string name)
        {
            if (name == null || name.Length == 0)
            {
                throw new ArgumentNullException("name");
            }

            if (name[0] != '@')
            {
                name = "@" + name;
            }


            this.name = name;
            this.formatter = formatter;
            parameterID = Utility.GenerateHashForText(name.ToLower());


           
        }


        internal string Name
        {
            get { return name; }
        }

        internal string ParameterID
        {
            get { return parameterID; }
        }

        public object GetValue()
        {
             return value; 
        }

        internal string GetFormattedValue()
        {
            if (mapMember != null)
            {
                return MapUtility.FormatValueAsString(this.mapMember, value, false);
            }
            else
            {
                return Convert.ToString(value);
            }
        }

        internal void SetValue(Type type, string member, object value)
        {
            if (type != null && (member != null && member.Length > 0))
            {
                ObjectSchemaInfo info = ObjectSchemaContainer.Current.Retrieve(type);
                
                if (info != null)
                {
                    mapMember = info.MappedMembers.Find(member);
                    if (mapMember == null)
                    {
                        throw new OrmMappingException("Member " + member + " not found for type " + type.FullName);
                    }
                }
                else
                {
                    throw new OrmMappingException("Type " + type.FullName + " not found. This type is not mapped as an ORM object. ");
                }
            }

            this.value = value;
        }

        internal void SetValue(string value)
        {
            this.value = String.Format("'{0}'", formatter.FormatString(value));
        }

        internal void SetValue(int value)
        {
            this.value = formatter.FormatString(Convert.ToString(value));
        }

        internal void SetValue(DateTime datetime)
        {
            this.value = String.Format("'{0}'", formatter.FormatDateTime(datetime));
        }

        internal void SetValue(bool value)
        {
            this.value = formatter.FormatBoolean(value);
        }

        

        internal  void SetValue(object source, string member)
        {
            Type type = null;
            if (source is ORMObject)
            {
                type = ((ORMObject) source).SourceObject.GetType();
            }
            else
            {
                type = source.GetType();
            }

            if (member != null && member.Length > 0)
            {
                ObjectSchemaInfo info = ObjectSchemaContainer.Current.Retrieve(type);
                if (info != null)
                {
                    mapMember = info.MappedMembers.Find(member);
                    if (mapMember == null)
                    {
                        throw new OrmMappingException("Member " + member + " not found for type " + type.FullName);
                    }
                }
                else
                {
                    throw new OrmMappingException("Type " + type.FullName + " not found. This type is not mapped as an ORM object. ");
                }
            }

            this.value = mapMember.GetMemberValue(source);

        }



        
        internal static string GetParameterID(string name)
        {
            if (name[0] != '@')
            {
                name = "@" + name;
            }

            return Utility.GenerateHashForText(name.ToLower());
        }
    }
}
