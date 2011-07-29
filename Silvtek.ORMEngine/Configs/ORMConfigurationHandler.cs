using System;
using System.Configuration;
using System.Xml;

#if !NET11
using NUnit.Framework;
using Silvtek.ORMEngine.Data;

#endif

namespace Silvtek.ORMEngine.Configs
{

    public class ORMConfigurationHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler Members

        public object Create(object parent, object configContext, XmlNode section)
        {
            return section.OuterXml;
        }

        #endregion

#if !NET11

        [TestFixture]
        public class ORMConfigurationHandlerTester
        {
            [Test]
            public void LoadConfigurationSection()
            {
                object obj = ConfigurationManager.GetSection("Silvtek.ORMEngine");


                Assert.IsNotNull(obj);

                string xml = Convert.ToString(obj);
                
                Console.WriteLine(xml);
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);

                if (document.DocumentElement == null || document.DocumentElement.LocalName != "Silvtek.ORMEngine")
                {
                    Assert.Fail("Silvtek.ORMEngine parent is missing");
                }

                string version = Utility.GetAttributeValue(document.DocumentElement, "version");
                Assert.IsNotNullOrEmpty(version);

                // now parse it.

                ORMConfigurationV1 version1 = new ORMConfigurationV1();
                version1.ReadConfiguration(document);

                // now, lets try to get the connection
                Connection cnn = ConnectionManager.Current.GetConnection("ORM.TESTERS");
                Assert.IsNotNull(cnn);
            }
        }

#endif
    }




}
