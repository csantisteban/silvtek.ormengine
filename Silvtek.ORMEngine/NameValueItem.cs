using System;
using System.Runtime.Serialization;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    internal class NameValueItem 
    {
        private string name;
        private object value;
        private bool stub;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public bool Stub
        {
            get { return stub; }
            set { stub = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameValueItem"/> class.
        /// </summary>
        public NameValueItem()
        {
            Stub = false;
            value = null;
            name = String.Empty;
        }

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("name", name);
        //    info.AddValue("value", value);
        //}

        //public NameValueItem(SerializationInfo info, StreamingContext context)
        //{
        //    name = info.GetString("name");
        //    value = info.GetValue("value",)
        //}
    }
}
