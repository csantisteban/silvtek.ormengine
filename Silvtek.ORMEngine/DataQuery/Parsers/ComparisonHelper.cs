#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace  Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Summary description for ComparisonHelper.
	/// </summary>
	internal sealed class ComparisonHelper
	{

		private ComparisonHelper()
		{
		}

		internal static bool IsCollections(Comparison comparison)
		{
			 int comparisonValue = (int)comparison;
			 return (comparisonValue > 2100);
		}

		internal static bool IsBetween(Comparison comparison)
		{
			int comparisonValue = (int)comparison;
			return (comparisonValue > 2000 && comparisonValue < 2100);
		}

		internal static bool IsNull(Comparison comparison)
		{
			int comparisonValue = (int)comparison;
			return (comparisonValue > 1000 && comparisonValue < 2000);
		}

		internal static bool IsSingle(Comparison comparison)
		{
			int comparisonValue = (int)comparison;
			return (comparisonValue < 2000);
		}

		
	}
}
