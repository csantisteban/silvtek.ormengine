namespace Silvtek.ORMEngine
{
        ///<summary>
        /// Represent an <see cref="ORMObject"/> action with the database.
        ///</summary>
        public enum Action
        {
           ///<summary>Create</summary>
           Create = 1,
           ///<summary>Store</summary>
           Save = 2,
           ///<summary>Remove</summary>
           Remove = 3 ,
           ///<summary>Load</summary>
           Load	= 4,

          // RunMethod = 5,

           //Query = 5,

          // None = 99,

           Undefined = 0
        }
}
