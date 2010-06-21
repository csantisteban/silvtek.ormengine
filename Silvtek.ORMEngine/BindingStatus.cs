using System;

namespace Silvtek.ORMEngine
{

   ///<summary>
   /// Defines the <see cref="ORMObject" /> state.
   ///</summary>
   public enum BindingStatus : int
   {
      ///<summary>
      /// Defines that is bounded (belongs) to a record in the database.
      ///</summary>
      Bound = 1,

      ///<summary>
      /// Defines that is not bounded (do not belong) to any record in
      /// the database.
      ///</summary>
      Unbind = 0,

      ///<summary>
      /// Defines that the record has been removed from the database.
      ///</summary>
      Removed  = 9
   }

}
