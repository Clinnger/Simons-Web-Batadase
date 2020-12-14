using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebClient_Commentor.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebClient_Commentor.Models;
using System.Data.SqlClient;

namespace WebClient_Commentor.DB.Tests
{
    [TestClass()]
    public class DBAccessVehiclesTests
    {
        string connectionString;
        Vehicle vehiclesToFind;
        DBAccessVehicles findVehicles;

        [TestInitialize]
        public void InitializeBeforeEachMethod()
        {
            vehiclesToFind = new Vehicle();
            findVehicles = new DBAccessVehicles();
        }
        [TestMethod()]
        public void DBAccessVehiclesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getAllVehiclesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getSortedVehiclesHourTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getSortedVehiclesDayAndHoursTest()
        {
            //Arrange Vehicles
            vehiclesToFind.VehicleId = 6;
            vehiclesToFind.DateTimeStamp = "10 Dec 2020";
            vehiclesToFind.HourStamp = "09";
            vehiclesToFind.VehicleAmount = 15;

            //Act
            IEnumerable<Vehicle> foundTestVehicles = findVehicles.getSortedVehiclesDayAndHours("09", "09", "10 Dec 2020", "10 Dec 2020");

            //Assert
            Assert.IsNotNull(foundTestVehicles);
        }

        [TestMethod()]
        public void getSortedVehiclesDayTest()
        {
            //Arrange Vehicles
            vehiclesToFind.DateTimeStamp = "10 Dec 2020";
            vehiclesToFind.VehicleAmount = 50;

            //Act
            IEnumerable<Vehicle> foundTestVehicles = findVehicles.getSortedVehiclesDay("10 Dec 2020","10 Dec 2020");

            //Assert
            Assert.IsNotNull(foundTestVehicles);
        }

        [TestMethod()]
        public void DeleteFromDBTest()
        {
            //Arrange
            findVehicles.InsertToDBToDelete();
            int testDeletion = findVehicles.FindLatestVehicleId();

            //Act
            findVehicles.DeleteFromDB(testDeletion);
                
            //Assert
            Assert.AreNotEqual(testDeletion, findVehicles.FindLatestVehicleId());
        }

        [TestMethod()]
        public void GetAllVehiclesByLatestDateTest()
        {
            //Arrange
            vehiclesToFind.VehicleId = 8;
            vehiclesToFind.DateTimeStamp = "11 Dec 2020";
            vehiclesToFind.HourStamp = "10";
            vehiclesToFind.VehicleAmount = 65;

            //Act
            IEnumerable<Vehicle> foundTestVehicles = findVehicles.GetAllVehiclesByLatestDate();

            //Assert
            Assert.IsNotNull(foundTestVehicles);
        }

        [TestMethod()]
        public void Get7LatestVehiclesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetVehiclesFromReaderTest()
        {
            Assert.Fail();
        }
    }
}