namespace Silvtek.ORMEngine
{
   ///<summary>
   /// Defines the result of the dataobject after a operation.
   ///</summary>
   public enum ActionResult : int
   {
      ///<summary>
      /// Defines as the operation is a success.
      ///</summary>
      Succeed = 1,

      ///<summary>
      /// Defines as the operation failed.
      ///</summary>
      Failed = 2,

      ///<summary>
      /// Defines as the operation, usually <see cref="ORMObject.Load" />
      /// did not found any record with same primary key.
      ///</summary>
      RecordNotFound = 3,

      //DuplicateKey,

      ///<summary>
      /// Defines as no transaction (operation) performed.
      ///</summary>
      TransactionNotPerformed = 4,

      ///<summary>
      /// Defines as the operation has not be submitted due that
      /// the record is intact.
      ///</summary>
      NotModified = 5,

       Undefined = 99

   }

}
