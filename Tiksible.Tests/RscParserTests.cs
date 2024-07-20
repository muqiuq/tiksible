using Tiksible.Services;

namespace Tiksible.Tests
{
    [TestClass]
    public class RscParserTests
    {
        [TestMethod]
        public void PathWithOrWithoutSlash()
        {
            string left = "/ip address add address=192.168.88.1/24 comment=defconf interface=bridge network=192.168.88.0";
            string right = "/ip/address add address=192.168.88.1/24 comment=defconf interface=bridge network=192.168.88.0";

            var statementLeft = RosRscParser.Parse(left);
            var statementRight = RosRscParser.Parse(right);

            Assert.AreEqual(statementLeft[0], statementRight[0]);
        }

        [TestMethod]
        public void PropertyOrdering1()
        {
            string left = "/ip address add address=192.168.88.1/24 comment=defconf interface=bridge network=192.168.88.0";
            string right = "/ip/address add comment=defconf address=192.168.88.1/24 interface=bridge network=192.168.88.0";

            var statementLeft = RosRscParser.Parse(left);
            var statementRight = RosRscParser.Parse(right);

            Assert.AreEqual(statementLeft[0], statementRight[0]);
        }

        [TestMethod]
        public void PropertyOrdering2()
        {
            string left = "/ip address add  network=192.168.88.0 address=192.168.88.1/24 comment=defconf interface=bridge  ";
            string right = "/ip/address add comment=defconf address=192.168.88.1/24 interface=bridge network=192.168.88.0";

            var statementLeft = RosRscParser.Parse(left);
            var statementRight = RosRscParser.Parse(right);

            Assert.AreEqual(statementLeft[0], statementRight[0]);
        }

        [TestMethod]
        public void SingleLineParseTest()
        {
            var inLine =
                "/ip firewall filter add action=accept chain=input comment=\"defconf: accept established,related,untracked\" connection-state=established,related,untracked";
            var parseResult = RosRscParser.Parse(inLine);
            var result = parseResult.Export();
            Assert.AreEqual(inLine, result.FirstOrDefault());
        }

        [TestMethod]
        public void ParseTest1()
        {
            var statements = RosRscParser.Parse(TestData.RscFile1Variant2);
            var expectedLines = TestData.ExpectedParseResultOutput.Replace("\r", "").Split("\n");

            var exportedLines = statements.Export();

            Assert.AreEqual(expectedLines.Length, exportedLines.Length);

            for (int a = 0; a < expectedLines.Length; a++)
            {
                Assert.AreEqual(expectedLines[a], exportedLines[a]);
            }
        }

        [TestMethod]
        public void ComparisonTest1()
        {
            var statementLeft = RosRscParser.Parse(TestData.RscFile1Variant1);
            var statementRight = RosRscParser.Parse(TestData.RscFile1Variant2);

            var result = statementLeft.Compare(statementRight);

            Assert.AreEqual(1, result.MissingStatemenetsOwn.Count);
            Assert.AreEqual(0, result.MissingStatemenetsOther.Count);

            Assert.AreEqual(TestData.MissingLineTest1, result.MissingStatemenetsOwn.FirstOrDefault()?.Export());
        }

        [TestMethod]
        public void ComparisonTest2()
        {
            var statementLeft = RosRscParser.Parse(TestData.RscFile2Variant1);
            var statementRight = RosRscParser.Parse(TestData.RscFile2Variant2);

            var result = statementLeft.Compare(statementRight);

            Assert.AreEqual(1, result.MissingStatemenetsOwn.Count);
            Assert.AreEqual(0, result.MissingStatemenetsOther.Count);

            Assert.AreEqual(TestData.MissingLineTest2, result.MissingStatemenetsOwn.FirstOrDefault()?.Export());
        }
    }
}