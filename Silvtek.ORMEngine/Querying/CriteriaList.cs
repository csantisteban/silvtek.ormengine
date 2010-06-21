//#region Copyright
/// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
/// See LICENSE in the repository root.
//#endregion

//using System;
//using System.Collections;
//using System.Diagnostics;
//using System.Reflection;

//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    /// Summary description for Class.
//    /// </summary>
//    public class CriteriaList : CollectionBase
//    {
//        #region -- Member variables --

//        private Type maintype;
//        private bool isCaseSensitive;

//        #endregion

//        ///<summary>
//        /// Gets a criteria instance by its index.
//        /// </summary>
//        internal Criteria this[int index]
//        {
//            get {return (Criteria)InnerList[index];}
//        }
		
//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        /// <param name="type">Principal type for the criteria</param>
//        /// <param name="isCaseSensitive">Whether the criteria is case sensitive.</param>
//        internal CriteriaList(Type type, bool isCaseSensitive)
//        {
//          this.maintype = type;
//          this.isCaseSensitive = isCaseSensitive;
//        }

//        #region --  AddCriteria using Between comparition LogicalOperator. --


//        ///<summary>
//        /// Adds a criteria that compares between two values.
//        ///</summary>
//        ///<param name="prefixOp">
//        /// The <see cref="LogicalOperator" /> that will prefix the condition.
//        ///</param>
//        ///<param name="cond">
//        /// <para>The <see cref="SQLCondition" />.</para>
//        /// This method will only accept <see cref="SQLCondition.In" />
//        /// or <see cref="SQLCondition.NotIn" /> conditions due that it compare against
//        /// multiple values.
//        ///</param>
//        ///<param name="type">
//        /// The <see cref="ORMObject" /> inhered <see cref="Type" /> that
//        /// <paramref name="property" /> belongs.
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="value1">
//        /// The first value to compare.
//        ///</param>
//        ///<param name="value2">
//        /// The second value to compare.
//        ///</param>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name between 1 and 3 </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name between 1 and 3 </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name between 1 and 3 </c>
//        /// </para>
//        ///</example>
//        ///<exception cref="QueryException">
//        /// If <paramref name="cond" /> is not <see cref="SQLCondition.Between" /> or
//        /// <see cref="SQLCondition.NotBetween" />.
//        ///</exception>
//        ///<remarks>
//        /// This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</remarks>
//        public void AddRange(LogicalOperator prefixOp, Type type, string property, Comparison comparison, object value1, object value2)
//        {
//            // Use main type when type is null;
//            if (type == null)type = maintype;
			
//            if (List.Count == 0 && prefixOp != LogicalOperator.None)
//            {
//                prefixOp = LogicalOperator.None;
//            }
//            else if(List.Count > 0 && prefixOp == LogicalOperator.None)
//            {
//                prefixOp = LogicalOperator.And;
//            }

//            Criteria tmpCriteria = new Criteria(prefixOp, type, property, comparison, value1, value2, isCaseSensitive);
//            List.Add(tmpCriteria);
//        }


//        ///<summary>
//        /// Add a criteria that compare a between two values.
//        ///</summary>
//        ///<param name="cond">
//        /// <para>The <see cref="SQLCondition" />.</para>
//        /// This method will only accept <see cref="SQLCondition.In" />
//        /// or <see cref="SQLCondition.NotIn" /> conditions due that it compare against
//        /// multiple values.
//        ///</param>
//        ///<param name="type">
//        /// The <see cref="ORMObject" /> inhered <see cref="Type" /> that
//        /// <paramref name="property" /> belongs.
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="value1">
//        /// The first value to compare.
//        ///</param>
//        ///<param name="value2">
//        /// The second value to compare.
//        ///</param>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name between 1 and 3 </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name between 1 and 3 </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name between 1 and 3 </c>
//        /// </para>
//        ///</example>
//        ///<exception cref="QueryException">
//        /// If <paramref name="cond" /> is not <see cref="SQLCondition.Between" /> or
//        /// <see cref="SQLCondition.NotBetween" />.
//        ///</exception>
//        ///<remarks>
//        /// This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</remarks>
//        public void AddRange(Type type, string property, Comparison comparison,  object value1, object value2)
//        {
//          AddRange(LogicalOperator.None, type, property, comparison,  value1, value2);
//        }



//        ///<summary>
//        /// Add a criteria that compare a between two values.
//        ///</summary>
//        ///<param name="cond">
//        /// <para>The <see cref="SQLCondition" />.</para>
//        /// This method will only accept <see cref="SQLCondition.In" />
//        /// or <see cref="SQLCondition.NotIn" /> conditions due that it compare against
//        /// multiple values.
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="value1">
//        /// The first value to compare.
//        ///</param>
//        ///<param name="value2">
//        /// The second value to compare.
//        ///</param>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name between 1 and 3 </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name between 1 and 3 </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name between 1 and 3 </c>
//        /// </para>
//        ///</example>
//        ///<exception cref="QueryException">
//        /// If <paramref name="cond" /> is not <see cref="SQLCondition.Between" /> or
//        /// <see cref="SQLCondition.NotBetween" />.
//        ///</exception>
//        ///<remarks>
//        ///<para>
//        /// 1. This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</para>
//        ///<para>
//        /// 2. This method will use the <see cref="MainObject" /> to location
//        /// the <paramref name="property" />.
//        ///</para>
//        ///</remarks>
//        public void AddRange(string property, Comparison comparison, object value1, object value2)
//        {
//          AddRange(LogicalOperator.None,  null, property, comparison, value1, value2);
//        }


//        ///<summary>
//        /// Add a criteria that compare a between two values.
//        ///</summary>
//        ///<param name="prefixOp">
//        ///<para>
//        /// The <see cref="SQLOperator" /> that will prefix the condition.
//        ///</para>
//        ///</param>
//        ///<param name="cond">
//        /// <para>The <see cref="SQLCondition" />.</para>
//        /// This method will only accept <see cref="SQLCondition.In" />
//        /// or <see cref="SQLCondition.NotIn" /> conditions due that it compare against
//        /// multiple values.
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="value1">
//        /// The first value to compare.
//        ///</param>
//        ///<param name="value2">
//        /// The second value to compare.
//        ///</param>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name between 1 and 3 </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name between 1 and 3 </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name between 1 and 3 </c>
//        /// </para>
//        ///</example>
//        ///<exception cref="QueryException">
//        /// If <paramref name="cond" /> is not <see cref="SQLCondition.Between" /> or
//        /// <see cref="SQLCondition.NotBetween" />.
//        ///</exception>
//        ///<remarks>
//        ///<para>
//        /// 1. This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</para>
//        ///<para>
//        /// 2. This method will use the <see cref="MainObject" /> to location
//        /// the <paramref name="property" />.
//        ///</para>
//        ///</remarks>
//        public void AddRange(LogicalOperator op, string property, Comparison comparison, object value1, object value2)
//        {
//          AddRange(op,  null, property, comparison, value1, value2);
//        }


//        public void AddRange(string property, object value1, object value2)
//        {
//          AddRange(LogicalOperator.None, maintype, property, Comparison.Between, value1, value2);
//        }
//    #endregion

//        #region --  AddCriteria Method using multiple values (ICollection)  --
//        ///<summary>
//        /// Add a criteria that compare a collection of values.
//        ///</summary>
//        ///<param name="prefixOp">
//        ///<para>
//        /// The <see cref="SQLOperator" /> that will prefix the condition.
//        ///</para>
//        ///</param>
//        ///<param name="cond">
//        /// <para>The <see cref="SQLCondition" />.</para>
//        /// This method will only accept <see cref="SQLCondition.In" />
//        /// or <see cref="SQLCondition.NotIn" /> conditions due that it compare against
//        /// multiple values.
//        ///</param>
//        ///<param name="type">
//        /// The <see cref="ORMObject" /> inhered <see cref="Type" /> that
//        /// <paramref name="property" /> belongs.
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="col">
//        /// The collection of values wrapped in a <see cref="ICollection" />
//        /// instance.
//        ///</param>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        ///</example>
//        ///<exception cref="QueryException">
//        /// If <paramref name="cond" /> is not <see cref="SQLCondition.In" /> or
//        /// <see cref="SQLCondition.NotIn" />.
//        ///</exception>
//        ///<remarks>
//        /// This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</remarks>
//        public void AddCollection(LogicalOperator op, Type type, string property, Comparison comparison, ICollection col)
//        {
//          if (type == null)
//          {
//             type = maintype;
//          }

//          if (List.Count == 0 && op != LogicalOperator.None)
//          {
//            op = LogicalOperator.None;
//          }
//          else if(List.Count > 0 && op == LogicalOperator.None)
//          {
//            op = LogicalOperator.And;
//          }

//          Criteria criteria_obj = new Criteria(op, type, property, comparison, col, isCaseSensitive);
//          List.Add(criteria_obj);

//        }


//        ///<summary>
//        /// Add a criteria that compare a collection of values.
//        ///</summary>
//        ///<param name="cond">
//        /// <para>The <see cref="Condition" />.</para>
//        /// This method will only accept <see cref="Condition.In" />
//        /// or <see cref="Condition.NotIn" /> conditions due that it compare against
//        /// multiple values.
//        ///</param>
//        ///<param name="type">
//        /// The <see cref="ORMObject" /> inhered <see cref="Type" /> that
//        /// <paramref name="property" /> belongs.
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="col">
//        /// The collection of values wrapped in a <see cref="ICollection" />
//        /// instance.
//        ///</param>
//        ///<remarks>
//        /// This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</remarks>
//        ///<exception cref="QueryException">
//        /// If <paramref name="cond" /> is not <see cref="SQLCondition.In" /> or
//        /// <see cref="SQLCondition.NotIn" />.
//        ///</exception>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        ///</example>
//        public void AddCollection(Type type, string property,  Comparison comparison, ICollection col)
//        {
//            AddCollection(LogicalOperator.None, type, property, comparison, col);
//        }

//        ///<summary>
//        /// Add a criteria that compare a collection of values.
//        ///</summary>
//        ///<param name="cond">
//        /// <para>The <see cref="SQLCondition" />.</para>
//        /// This method will only accept <see cref="SQLCondition.In" />
//        /// or <see cref="SQLCondition.NotIn" /> conditions due that it compare against
//        /// multiple values.
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="col">
//        /// The collection of values wrapped in a <see cref="ICollection" />
//        /// instance.
//        ///</param>
//        ///<remarks>
//        ///<para>
//        /// 1. This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</para>
//        ///<para>
//        /// 2. This method will use the <see cref="MainObject" /> to location
//        /// the <paramref name="property" />.
//        ///</para>
//        ///</remarks>
//        ///<exception cref="QueryException">
//        /// If <paramref name="cond" /> is not <see cref="SQLCondition.In" /> or
//        /// <see cref="SQLCondition.NotIn" />.
//        ///</exception>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        ///</example>
//        public void AddCollection(string property, Comparison comparison, ICollection col)
//        {
//          AddCollection(LogicalOperator.None, null, property, comparison, col);
//        }

//        ///<summary>
//        /// Add a criteria that compare a collection of values.
//        ///</summary>
//        ///<param name="prefixOp">
//        ///<para>
//        /// The <see cref="SQLOperator" /> that will prefix the condition.
//        ///</para>
//        ///</param>
//        ///<param name="cond">
//        /// <para>The <see cref="SQLCondition" />.</para>
//        /// This method will only accept <see cref="SQLCondition.In" />
//        /// or <see cref="SQLCondition.NotIn" /> conditions due that it compare against
//        /// multiple values.
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="col">
//        /// The collection of values wrapped in a <see cref="ICollection" />
//        /// instance.
//        ///</param>
//        ///<remarks>
//        ///<para>
//        /// 1. This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</para>
//        ///<para>
//        /// 2. This method will use the <see cref="MainObject" /> to location
//        /// the <paramref name="property" />.
//        ///</para>
//        ///</remarks>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name in ('a', 'b', 'c') </c>
//        /// </para>
//        ///</example>
//        ///<exception cref="QueryException">
//        /// If <paramref name="cond" /> is not <see cref="SQLCondition.In" /> or
//        /// <see cref="SQLCondition.NotIn" />.
//        ///</exception>
//        public void AddCollection(LogicalOperator op, string property,  Comparison comparison, ICollection col)
//        {
//          AddCollection(op, maintype, property, comparison, col);
//        }

//        public void AddCollection(string property, ICollection col)
//        {
//          AddCollection(LogicalOperator.None,  property, Comparison.In, col);
//        }

//     #endregion

//        #region --  Regular criteria --

//             ///<summary>
//        /// Add a regular criteria
//        ///</summary>
//        ///<param name="prefixOp">
//        ///<para>
//        /// The <see cref="SQLOperator" /> that will prefix the condition.
//        ///</para>
//        ///</param>
//        ///<param name="cond">
//        /// The <see cref="SQLCondition" /> used to compare value
//        ///</param>
//        ///<param name="type">
//        /// The <see cref="ORMObject" /> inhered <see cref="Type" /> that
//        /// <paramref name="property" /> belongs.
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="value">
//        /// The value to compare.
//        ///</param>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="LogicalOperator" >LogicalOperator.Or</see> Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name = 'A' </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="LogicalOperator" >LogicalOperator.And</see> Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name = 'A' </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="LogicalOperator" >LogicalOperator.None</see> Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name = 'A' </c>
//        /// </para>
//        ///</example>
//        ///<remarks>
//        /// This method will create a criteria with <see cref="LogicalOperator" >LogicalOperator.None</see>
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="LogicalOperator" >LogicalOperator.And</see> prefix.
//        ///</remarks>
//       public void Add(LogicalOperator prefixOp, Type type, string property, Comparison condition, object value)
//       {
//          if (type == null)
//          {
//             type = maintype;
//          }

//          if (List.Count == 0 && prefixOp != LogicalOperator.None)
//          {
//            prefixOp = LogicalOperator.None;
//          }
//          else if(List.Count > 0 && prefixOp == LogicalOperator.None)
//          {
//            prefixOp = LogicalOperator.And;
//          }

//          Criteria tmpCriteria = null;
//          if (condition == Comparison.Between || condition == Comparison.NotBetween)
//          {
//             tmpCriteria = new Criteria(prefixOp, type, property, condition, value, value, isCaseSensitive);
//          }
//          else if (condition == Comparison.In || condition == Comparison.NotIn)
//          {
//            ArrayList tmpList = new ArrayList();
//            tmpList.Add(value);
//            tmpCriteria = new Criteria(prefixOp, type, property, condition, tmpList, isCaseSensitive);
//          }
//          else
//          {
//             tmpCriteria = new Criteria(prefixOp, type, property, value, condition, isCaseSensitive);
//          }

//           List.Add(tmpCriteria);
//        }



//        ///<summary>
//        /// Add a regular criteria
//        ///</summary>
//        ///<param name="cond">
//        /// The <see cref="SQLCondition" /> used to compare value
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="value">
//        /// The value to compare.
//        ///</param>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="LogicalOperator" >LogicalOperator.Or</see> Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name = 'A' </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="LogicalOperator" >LogicalOperator.And</see> Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name = 'A' </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="LogicalOperator" >LogicalOperator.None</see> Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name = 'A' </c>
//        /// </para>
//        ///</example>
//        ///<remarks>
//        ///<para>
//        /// This method will create a criteria with <see cref="LogicalOperator" >LogicalOperator.None</see>
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the  <see cref="LogicalOperator" >LogicalOperator.And</see> prefix.
//        ///</para>
//        ///<para>
//        /// 2. This method will use the MainObject  to location
//        /// the <paramref name="property" />.
//        ///</para>
//        ///</remarks>
//      public void Add(string property, Comparison condition, object value)
//       {
//          Add(LogicalOperator.None, null, property, condition, value);
//       }


//        ///<summary>
//        /// Add a regular criteria
//        ///</summary>
//        ///<param name="prefixOp">
//        ///<para>
//        /// The <see cref="SQLOperator" /> that will prefix the condition.
//        ///</para>
//        ///</param>
//        ///<param name="cond">
//        /// The <see cref="SQLCondition" /> used to compare value
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="value">
//        /// The value to compare.
//        ///</param>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name = 'A' </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name = 'A' </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name = 'A' </c>
//        /// </para>
//        ///</example>
//        ///<remarks>
//        ///<para>
//        /// This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</para>
//        ///<para>
//        /// 2. This method will use the <see cref="MainObject" /> to location
//        /// the <paramref name="property" />.
//        ///</para>
//        ///</remarks>
//       public void Add(LogicalOperator prefixOp, string property, Comparison condition, object value)
//       {
//           Add(prefixOp, null, property, condition, value);
//       }



//             ///<summary>
//        /// Add a regular criteria
//        ///</summary>
//        ///<param name="prefixOp">
//        ///<para>
//        /// The <see cref="SQLOperator" /> that will prefix the condition.
//        ///</para>
//        ///</param>
//        ///<param name="cond">
//        /// The <see cref="SQLCondition" /> used to compare value
//        ///</param>
//        ///<param name="property">
//        /// The property of <see cref="ORMObject" /> that the query
//        /// will compare
//        ///</param>
//        ///<param name="value">
//        /// The value to compare.
//        ///</param>
//        ///<example>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.And" />Condition.
//        /// </para>
//         ///<para> The result will be <c> and column_name = 'A' </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.Or" />Condition.
//        /// </para>
//        ///<para> The result will be <c> or column_name = 'A' </c>
//        /// </para>
//        /// <para>
//        /// The parameter <paramref name="prefixOp" /> value is <see cref="SQLOperator.None" />Condition.
//        /// </para>
//        ///<para> The result will be <c> column_name = 'A' </c>
//        /// </para>
//        ///</example>
//        ///<remarks>
//        /// This method will create a criteria with <see cref="SQLOperator.None" />
//        /// prefix. If this is not the first criteria in the query
//        /// it will be added the <see cref="SQLOperator.And" /> prefix.
//        ///</remarks>
//       public void Add(Type type, string property, Comparison condition, object value)
//       {
//           Add(LogicalOperator.None, type, property, condition, value);
//       }

//       ///<summary>
//       /// Add an <see cref="ISQLCriteria" /> instance.
//       ///</summary>
//       internal void Add(ICriteria criteria)
//       {

//          if (List.Count == 0 && criteria.LogicalOperator != LogicalOperator.None)
//          {
//              criteria.SetOperator(LogicalOperator.None);
//          }
//          else if(List.Count > 0 && criteria.LogicalOperator == LogicalOperator.None)
//          {
//              criteria.SetOperator(LogicalOperator.And);
//          }

//          List.Add(criteria);
//       }

//      #endregion

//        public void AddIsNull(LogicalOperator op, Type type, string property)
//        {
//            Add(op, type, property, Comparison.IsNull, null);
//        }

//        public void AddIsNull(LogicalOperator op, string property)
//        {
//            AddIsNull(op, maintype, property);
//        }

//        public void AddIsNull(Type type, string property)
//        {
//            AddIsNull(LogicalOperator.None, type, property);
//        }

//        public void AddIsNotNull(LogicalOperator op, Type type, string property)
//        {
//            Add(op, type, property, Comparison.IsNotNull, null);
//        }

//        public void AddIsNotNull(LogicalOperator op, string property)
//        {
//            AddIsNotNull(op, maintype, property);
//        }

//        public void AddIsNotNull(Type type, string property)
//        {
//            AddIsNotNull(LogicalOperator.None, type, property);
//        }


//      internal void InternalAdd(LogicalOperator op, Type type, string property, Comparison comparison, Type type2, string property2)
//      {
//         if (List.Count == 0 && op != LogicalOperator.None)
//         {
//           op =  LogicalOperator.None;
//         }
//         else if(List.Count > 0 && op == LogicalOperator.None)
//         {
//            op = LogicalOperator.And;
//         }

//        Criteria criteria_obj = null;
          
//        if (  comparison == Comparison.Between    ||
//              comparison == Comparison.NotBetween ||
//              comparison == Comparison.In         ||
//              comparison == Comparison.NotIn
//            )
//        {
//            throw new QueryException("Do not support range query between properties");
//        }

//         criteria_obj = new Criteria(op, type, property, comparison, type2, property2, isCaseSensitive);

//         List.Add(criteria_obj);
//      }


//      public void Add(LogicalOperator prefixOp, Type type, string property, Comparison comparison, Type type2, string property2)
//       {
//          // Thrown an exception if type2 is null;
//          if (type2 == null)throw new ArgumentNullException("type2");
          
//          // Use the main type if type is null;
//          if (type == null) type = maintype;
         

//          if (List.Count == 0 && prefixOp != LogicalOperator.None)
//          {
//             prefixOp = LogicalOperator.None;
//          }
//          else if(List.Count > 0 && prefixOp == LogicalOperator.None)
//          {
//             prefixOp = LogicalOperator.And;
//          }

//          Criteria tmpCriteria = null;
          
//          if ( comparison == Comparison.Between     ||
//               comparison == Comparison.NotBetween  ||
//               comparison == Comparison.In          ||
//               comparison == Comparison.NotIn
//             )
//          {
//              throw new QueryException("Do not support range query between properties");
//          }

//            tmpCriteria = new Criteria(prefixOp, type, property, comparison, type2, property2, isCaseSensitive);

//            List.Add(tmpCriteria);
//        }

//        public override string ToString()
//        {
//            string criteria_str = String.Empty;
//            foreach(ICriteria criteriaObj in List)
//            {
//               criteria_str += criteriaObj.ToString();
//            }

//            return criteria_str;
//        }

//    }
//}
