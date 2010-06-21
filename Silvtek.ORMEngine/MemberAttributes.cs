using System;
using System.Text;

namespace Silvtek.ORMEngine
{
    [Flags]
    internal enum MemberAttributes
    {
        None = 0,
        AllowNulls = 2,
        PrimaryKey = 4,
        Unique = 8,
        DatabaseIdentity = 16 | SyncField,
        Timestamp = 32,
        SyncField = 64
    }
}
