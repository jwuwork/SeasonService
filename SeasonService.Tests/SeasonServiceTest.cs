using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeasonService
{
    [TestClass]
    public class SeasonServiceTest
    {
        [TestMethod]
        public void Calculate()
        {
            var spring = new DateTime(2013, 3, 20, 11, 1, 36, 502);
            var summer = new DateTime(2013, 6, 21, 5, 3, 36, 502);
            var autumn = new DateTime(2013, 9, 22, 20, 43, 27, 502);
            var winter = new DateTime(2013, 12, 21, 17, 10, 56, 502);

            Assert.AreEqual(spring, SeasonService.Calculate(2013, Season.Spring));
            Assert.AreEqual(summer, SeasonService.Calculate(2013, Season.Summer));
            Assert.AreEqual(autumn, SeasonService.Calculate(2013, Season.Autumn));
            Assert.AreEqual(winter, SeasonService.Calculate(2013, Season.Winter));
        }
    }
}
