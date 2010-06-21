using System;

namespace Silvtek.ORMEngine
{
    [Flags]
    public enum ColumnModeType
    {
        IgnoreChanges = 2,
        ExcludeFromDigest = 4,
        Default = 0
    }
}
