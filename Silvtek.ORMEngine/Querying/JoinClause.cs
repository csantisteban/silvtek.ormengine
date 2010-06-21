//#region Copyright
/// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
/// See LICENSE in the repository root.
//#endregion

//using System;
//using Silvtek.ORMEngine.Schema;

//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    ///  Represent a SQL Join.
//    /// </summary>
//    public class JoinClause
//    {

//       //private const string TEMPLATE_2 =
//        //      " {join} join {table} on {table_col} {condition} {table_col2} ";

//       private const string TEMPLATE =
//              " {join} join {table} on {criteria} ";


//       // Hold the string buffer template.
//       private StringBuffer buffer;

//       // Hold whether the join is case sensitive.
//       private bool iscasesensitive;

//       // Hold the join unique signature.
//       private string unique_signature;

//       // Hold the join type.
//       private JoinType jointype;
//       private Type type1;
//       private Type type2;


//       // Hold the join criteria.
//       private CriteriaList criteria;


//       ///<summary>
//       /// Adds an extra key to the join.
//       ///</summary>
//       ///<param name="op">
//       /// The <see cref="SQLOperator" /> that will join the first
//       /// criteria key with this one.
//       ///</param>
//       ///<param name="type1_property">
//       /// The property name of the first class <see cref="Type" />.
//       ///</param>
//       ///<param name="comparison">
//       /// The <see cref=SQLComparison" /> that will compare
//       /// <paramref name="type1_property" /> and <paramref name="type2_property" />
//       ///</param>
//       ///<param name="type2_property">
//       /// The property name of the second class <see cref="Type" /> in
//       /// the Join..
//       ///</param>
//       public void AddKey(LogicalOperator op, string type1_property,
//                          Comparison comparison, string type2_property)
//       {
//         criteria.InternalAdd(op, type1, type1_property, comparison, type2,
//                              type2_property);
//       }
       
//       ///<summary>
//       /// Clears all SQL Join keys.
//       ///</summary>
//       public void ClearKeys()
//       {
//         criteria.Clear();
//       }


//       public override int GetHashCode()
//       {
//          return unique_signature.GetHashCode();
//       }

//       public override bool Equals(object obj)
//       {
//           if (obj.GetType() != GetType())return false;
//           JoinClause tmp = (JoinClause)obj;
//           return (tmp.unique_signature == this.unique_signature);
//       }

//       public override string ToString()
//       {
//           buffer.SetTokenValue("criteria", criteria.ToString());
//           return buffer.ToString();
//       }

//       internal bool CaseSensitive
//       {
//           get {return iscasesensitive;}
//       }

//       ///<summary>
//       /// Constructor.
//       ///</summary>
//       ///<param name="jointype">
//       /// The <see cref="JoinType" /> for this instance.
//       ///</param>
//       ///<param name="type" >
//       /// The <see cref="Type" /> of the <see cref="ORMObject" />
//       /// class that is joining to <paramref name="type2" /> class.
//       ///</param>
//       ///<param name="property">
//       /// The property name of the class <paramref name="type" />
//       /// that is used to join <paramref name="type2" /> class.
//       ///</param>
//       ///<param name="type2">
//       /// The <see cref="Type" /> of the <see cref="ORMObject" />
//       /// that is joining to <paramref name="type" /> class.
//       ///</param>
//       ///<param name="property2">
//       /// The property name of the class <paramref name="type" />
//       /// that is used to join <paramref name="type" /> class.
//       ///<param>
//       ///<param name="isCaseSenstive" >
//       /// Whether the <see cref="Query" /> where the joins is being used
//       /// is case sensitive.
//       ///</param>
//       internal JoinClause(JoinType jointype, Type type, string property,
//                          Comparison comparison, Type type2, string property2,
//                          bool iscasesensitive)
//       {
//          this.iscasesensitive = iscasesensitive;
//          this.type1 = type;
//          this.type2 = type2;

//          buffer = new StringBuffer(TEMPLATE);
//          SetJoin(jointype);
//          SetTableName(type2);
//          criteria = new CriteriaList(type, iscasesensitive);
//          criteria.InternalAdd(LogicalOperator.None, type, property, comparison, type2, property2);


//          unique_signature = String.Format("{0}{1}.{2}{3}{4}.{5}",
//                                           jointype, type, property, comparison, type2,
//                                           property2);
//       }


//       private void SetTableName(Type type)
//       {
//          ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(type);
//          buffer.SetTokenValue("table", info.TableName);
//       }

//       private void SetJoin(JoinType type)
//       {
//           this.jointype = type;
//           string val = "";
//           switch(type)
//           {
//             case JoinType.Inner:
//              val = "inner";
//              break;
             
//              case JoinType.Outer:
//              val = "outer";
//              break;
             
//             case JoinType.Left:
//              val = "left";
//              break;
             
//             case JoinType.Right:
//              val = "right";
//              break;
//             default:
//              break;
//          }

//           buffer.SetTokenValue("join", val);
//       }


//    }
//}
