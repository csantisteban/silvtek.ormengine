using System;

namespace Silvtek.ORMEngine
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class PreserveAttribute : Attribute
    {
        private readonly bool preserveNull;

        //private bool skipDefaultValue;

        //public bool SkipDefaultValue
        //{
        //    get { return skipDefaultValue; }
        //    set { skipDefaultValue = value; }
        //}

        public PreserveAttribute()
        {
            preserveNull = false;
        }

        public PreserveAttribute(bool preserveNulls)
        {
            this.preserveNull = preserveNulls;
        }

        public bool PreserveNull
        {
            get { return preserveNull; }
        }
    }
}
