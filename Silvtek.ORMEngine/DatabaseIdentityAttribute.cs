using System;

namespace Silvtek.ORMEngine
{

    /// <summary>
    /// Marks a member that its values will be get from the database identity.
    /// </summary>
    /// <remarks>
    /// Using this attribute with tied your application to Microsoft Sql Server.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public sealed class DatabaseIdentityAttribute : Attribute
    {
        public DatabaseIdentityAttribute()
        {
        }
    }
}
