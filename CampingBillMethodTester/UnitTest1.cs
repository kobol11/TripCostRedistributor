using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CampingBillMethodTester
{
    [TestClass]
    public class UnitTest1
    {

        TripCostRedistributor.CampingBill campingBill = new TripCostRedistributor.CampingBill();
        [TestMethod]
        public void TestMethod1()
        {

            int result = campingBill.IntegerParser("2");
            Assert.AreEqual(2, result);

        }
        
        [TestMethod]
        public void TestMethod2()
        {
            double result = campingBill.DoubleParser("2");
            Assert.AreEqual((double)2, result);
        }
        [TestMethod]
        public void TestMethod3()
        {
            TripCostRedistributor.Participant participant = new TripCostRedistributor.Participant();
            participant.Expenses.Add(2);
            participant.Expenses.Add(3.3);
            double result = campingBill.TotalCost(participant);
            Assert.AreEqual(5.3, result);
        }
        [TestMethod]
        public void TestMethod4()
        {
            TripCostRedistributor.Participant participant = new TripCostRedistributor.Participant();
            participant.TotalMoneySpent = 11;

            double result = campingBill.Refund(participant, 10.0);
            Assert.AreEqual(-1.0, result);
        }

        [TestMethod]
        public void TestMethod5()
        {

            int result = campingBill.IntegerParser("2.0");
            Assert.AreEqual(-1, result);

        }
    }
}
