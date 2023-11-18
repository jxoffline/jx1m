using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameServer.KiemThe.Tests
{
    [TestClass()]
    public class KTGlobalTests
    {
        [TestMethod()]
        public void AccrueSeriesSourceRequestTest()
        {
            int GetDestValue = KTGlobal.g_nAccrueSeries[1];

            Console.WriteLine(GetDestValue);
        }
    }
}