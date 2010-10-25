namespace Silvtek.ORMEngine
{
    /// <summary>
    /// 
    /// </summary>
    internal class NameValueItem
    {
        private string name;
        private object value;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="NameValueItem"/> class.
        /// </summary>
        public NameValueItem()
        {
            
        }
    }
}
