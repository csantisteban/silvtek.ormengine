#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion
using System.Collections;
using System.Data;

namespace Silvtek.ORMEngine.Data
{
	/// <summary>
	/// Summary description for DataParameterCollection.
	/// </summary>
	public class DataParameterCollection : CollectionBase, IDataParameterCollection
	{
		
		public DataParameterCollection()
		{
		
		}

		#region IDataParameterCollection Members

		///<summary>
		/// Gets the <see cref="pIDataParameter" /> object by parameter Name.
		/// </summary>
		object IDataParameterCollection.this[string parameterName]
		{
			get	{ return GetParameter(parameterName);}
			set { Add((DataParameter)value);}
		}

		///<summary>
		/// Gets the <see cref="IDataParamenter" /> object by its index in the collection.
		///</summary>
		public IDataParameter this[int index]
		{
			get {return (IDataParameter)InnerList[index];}
		}
		
		public IDataParameter this[string parameterName]
		{
			get	{ return GetParameter(parameterName);}
			set { Add(value);}
		}


		public void RemoveAt(string parameterName)
		{
			for(int i = 0; i < InnerList.Count; i++)
			{
				IDataParameter tempParam = (IDataParameter)InnerList[i];
				if (tempParam.ParameterName == parameterName)
				{
					InnerList.RemoveAt(i);
					break;
				}
			}
		}

		public bool Contains(string parameterName)
		{
			for(int i = 0; i < InnerList.Count; i++)
			{
				IDataParameter tempParam = (IDataParameter)InnerList[i];
				if (tempParam.ParameterName == parameterName)
				{
					return true;
				}
			}

			return false;
		}

		public int IndexOf(string parameterName)
		{
			for(int i = 0; i < InnerList.Count; i++)
			{
				IDataParameter tempParam = (IDataParameter)InnerList[i];
				if (tempParam.ParameterName == parameterName)
				{
					return i;
				}
			}

			return -1;
		}



		#endregion

		public void Add(IDataParameter param)
		{
			bool paramFound = false;
			for(int i = 0; i < InnerList.Count; i++)
			{
				DataParameter tempParam = (DataParameter)InnerList[i];
				if (tempParam.ParameterName == param.ParameterName)
				{
					InnerList[i] = param;
					paramFound = true;
					break;
				}
			}

			if (!paramFound)InnerList.Add(param);
		}



		public DataParameter GetParameter(string name)
		{
			for(int i = 0; i < InnerList.Count; i++)
			{
				DataParameter tempParam = (DataParameter)InnerList[i];
				if (tempParam.ParameterName == name)
				{
					return tempParam;
				}
			}

			// returns null if the parameter name is not found in the colleciton.
			return null;
		}

	}
}
