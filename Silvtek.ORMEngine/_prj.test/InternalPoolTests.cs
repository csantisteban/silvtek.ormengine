using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using NUnit.Framework;
using Silvtek.ORMEngine.Data;
using Silvtek.ORMEngine.ObjectPooling;

namespace Silvtek.ORMEngine._prj.test
{
    [TestFixture]
    public class InternalPoolTests
    {
        private static readonly Pool<Connection> ConnectionPool = new Pool<Connection>(45, 2500);
        private delegate void RunTest();
        private object mutex = new object();
        private int finishCount = 0;

        [Test]
        public void GenerateObjectsMultipleThreads()
        {
             for(int i = 0; i < 6; i++)
             {
                 RunTest test = new RunTest(Generate10000Objects);
                 test.BeginInvoke(EndGenerateObjectsMultipleThreads, null);
             }


            do
            {
                Thread.Sleep(250);
            } while (finishCount < 5);
        }

        public void EndGenerateObjectsMultipleThreads(IAsyncResult result)
        {
            if (result == null) return;
            AsyncResult asyncResult = (AsyncResult)result;
            RunTest asyncDelegate = (RunTest)asyncResult.AsyncDelegate;
            asyncDelegate.EndInvoke(result);
            
            lock(mutex)
            {
                finishCount++;
                Console.WriteLine("Thread count {0} finished", finishCount);
            }
        }

        [Test]
        public void Generate10000Objects()
        {
            for (int i = 0; i <= 10000; i++)
            {
               Connection cnn = ConnectionPool.GetObject();
               Assert.IsNotNull(cnn);
            }
        }

        [Test]
        public void Generate20000000Objects()
        {
            for (int i = 0; i <= 20000000; i++)
            {
                Connection cnn = ConnectionPool.GetObject();
                Assert.IsNotNull(cnn);
            }
        }
    }
}
