using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Represents how a enumeration will be converted from the database.
	/// </summary>
	public enum EnumConversionType : byte
	{
		/// <summary>
		/// The convertion will be by enum name.
		/// </summary>
		ByName = 1,

		/// <summary>
		/// The convertion will be by value.
		/// </summary>
		ByValue = 4
	}

    /// <summary>
    /// Represents how a enumeration will be converted from the database.
    /// </summary>
    [Obsolete("use EnumConversionType instead")]
    public enum EnumConvertionType : byte
    { 
        /// <summary>
        /// The convertion will be by enum name.
        /// </summary>
        ByName = 1,

        /// <summary>
        /// The convertion will be by value.
        /// </summary>
        ByValue = 4
    }
}
