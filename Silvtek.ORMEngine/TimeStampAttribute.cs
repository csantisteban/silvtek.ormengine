using System;

namespace Silvtek.ORMEngine
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public sealed class TimeStampAttribute  : ColumnAttribute
    {
        private readonly bool syncfield;
     
        public TimeStampAttribute(string name) : this(name, true)
        {
        }

        public TimeStampAttribute(string name, bool syncfield) : base(name, DataType.DateTime)
	   {
           this.syncfield = syncfield;
           ModeType = ColumnModeType.IgnoreChanges | ColumnModeType.ExcludeFromDigest;
	   }

        public bool Syncfield
        {
            get { return syncfield; }
        }
    }
}
