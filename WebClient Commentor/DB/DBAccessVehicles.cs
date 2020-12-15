using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebClient_Commentor.Models;
using System.Data.SqlClient;
using System.Web.ModelBinding;
using System.Transactions;
using System.Windows.Forms;
using System.Globalization;

namespace WebClient_Commentor.DB
{
    public class DBAccessVehicles
    {
        readonly string connectionString;

        public DBAccessVehicles()
        {
            connectionString = "data Source=.; database=CommentorDB; integrated security=true";
        }

        public int getLatestHourFromVehicles()
        {
            int foundHour = 0;
            string queryString = "select DATEPART(hour, DateTimeStamp) as Hour from vehicle where vehicleid = (select (max(vehicleid)) from vehicle)";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
               
                con.Open();

                foundHour = (int)readCommand.ExecuteScalar();

            }
            return foundHour;
        }

        public List<Vehicle> GetVehiclesForMapOverview()
        {
            List<Vehicle> readVehicles = new List<Vehicle>();
            int foundHour = getLatestHourFromVehicles();
            string queryString = "select Sum(VehicleAmount) as VehicleAmount, DATEPART(hour, DateTimeStamp) as Hour, FORMAT(DateTimeStamp, 'yyyy-MM-dd') as DateTimeStamp, Feed from Vehicle where FORMAT(DateTimeStamp, 'yyyy-MM-dd') = (select(max(FORMAT(DateTimeStamp, 'yyyy-MM-dd'))) from vehicle) and DatePart(hour, DateTimeStamp) = @Hour group by DATEPART(hour, DateTimeStamp), FORMAT(DateTimeStamp, 'yyyy-MM-dd'), Feed";


            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddHour = new SqlParameter("@Hour", foundHour);
                readCommand.Parameters.Add(AddHour);
                con.Open();

                SqlDataReader vehiclesReader = readCommand.ExecuteReader();

                if (vehiclesReader.HasRows)
                {
                    while (vehiclesReader.Read())
                    {
                        Vehicle aVehicle = GetVehiclesWithAmount(vehiclesReader);
                        readVehicles.Add(aVehicle);
                    }
                }
            }
            return readVehicles;
        }

        //Sorter efter uger.
        public List<Vehicle> SortByWeeks(string StartDate, string EndDate, string VehicleType)
        {
            List<Vehicle> readVehicles = new List<Vehicle>();
            string queryString = "";
            if (string.IsNullOrEmpty(VehicleType) || VehicleType.Equals("Vehicle"))
            {
                queryString = "SELECT SUM(VehicleAmount) AS VehicleAmount, DATEPART(ww, DateTimeStamp) AS DateTimeStamp FROM Vehicle WHERE DATEPART(ww, DateTimeStamp) BETWEEN DATEPART(WEEK, @StartDate) AND DATEPART(WEEK, @EndDate) group by DATEPART(ww, DateTimeStamp)";
            }
            else
            {
                queryString = "SELECT SUM(VehicleAmount) AS VehicleAmount, DATEPART(ww, DateTimeStamp) AS DateTimeStamp FROM Vehicle WHERE TypeName = @VehicleType and DATEPART(ww, DateTimeStamp) BETWEEN DATEPART(WEEK, @StartDate) AND DATEPART(WEEK, @EndDate) group by DATEPART(ww, DateTimeStamp)";
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddStartDate = new SqlParameter("@StartDate", StartDate);
                readCommand.Parameters.Add(AddStartDate);
                SqlParameter AddEndDate = new SqlParameter("@EndDate", EndDate);
                readCommand.Parameters.Add(AddEndDate);
                SqlParameter AddVehicleType = new SqlParameter("@VehicleType", VehicleType);
                readCommand.Parameters.Add(AddVehicleType);

                con.Open();

                SqlDataReader vehiclesReader = readCommand.ExecuteReader();

                if (vehiclesReader.HasRows)
                {
                    while (vehiclesReader.Read())
                    {
                        Vehicle aVehicle = GetVehiclesWithIntFromReader(vehiclesReader, true);
                        readVehicles.Add(aVehicle);
                    }
                }
            }
            return readVehicles;
        }


        //Denne metode vælger alle køretøjer, hvor der er angivet en start time, slut time, start dato, og slut dato, og sorter derefter.
        public List<Vehicle> getSortedVehiclesDayAndHours(string StartHour, string EndHour, string StartDate, string EndDate, string VehicleType)
        {
            List<Vehicle> foundVehicles = null;
            Vehicle readVehicles = null;
            Vehicle emptyVehicle = new Vehicle(0, 0, DateTime.Now);
            string queryString = "";
            if (string.IsNullOrEmpty(VehicleType) || VehicleType.Equals("Vehicle"))
            {
                queryString = "select Sum(VehicleAmount) as VehicleAmount, DATEPART(hour, DateTimeStamp) as Hour, FORMAT(DateTimeStamp,'yyyy-MM-dd') as DateTimeStamp from vehicle where FORMAT(DateTimeStamp,'yyyy-MM-dd') >= @StartDate and FORMAT(DateTimeStamp,'yyyy-MM-dd') <= @EndDate and DATEPART(hour, DateTimeStamp) >= @StartHour and DATEPART(hour, DateTimeStamp) <= @EndHour group by DATEPART(hour, DateTimeStamp), FORMAT(DateTimeStamp,'yyyy-MM-dd')";
            }
            else
            {
                queryString = "select Sum(VehicleAmount) as VehicleAmount, DATEPART(hour, DateTimeStamp) as Hour, FORMAT(DateTimeStamp,'yyyy-MM-dd') as DateTimeStamp from vehicle where TypeName = @VehicleType and FORMAT(DateTimeStamp,'yyyy-MM-dd') >= @StartDate and FORMAT(DateTimeStamp,'yyyy-MM-dd') <= @EndDate and DATEPART(hour, DateTimeStamp) >= @StartHour and DATEPART(hour, DateTimeStamp) <= @EndHour group by DATEPART(hour, DateTimeStamp), FORMAT(DateTimeStamp,'yyyy-MM-dd')";
            }

            //string queryString = "SELECT Vehicle.VehicleId, Vehicle.TypeName, Vehicle.VehicleAmount, Vehicle.Feed, Vehicle.DateTimeStamp, DatePart (YEAR, DateTimeStamp) AS Year, DatePart(DAY, DateTimeStamp)AS Day, DatePart(MONTH, DateTimeStamp) AS Month, DatePart(WEEK, DateTimeStamp) AS Week, DatePart(DAY, DateTimeStamp)AS Day, DatePart(HOUR, DateTimeStamp) AS Hour, DatePart(MINUTE, DateTimeStamp) AS Minute, DatePart(SECOND, DateTimeStamp) AS Second FROM Vehicle WHERE DateTimeStamp BETWEEN @StartDate AND @EndDate AND Vehicle.TypeName = @VehicleType";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddStartDate = new SqlParameter("@StartDate", StartDate);
                readCommand.Parameters.Add(AddStartDate);
                SqlParameter AddEndDate = new SqlParameter("@EndDate", EndDate);
                readCommand.Parameters.Add(AddEndDate);

                SqlParameter AddStartHour = new SqlParameter("@StartHour", StartHour);
                readCommand.Parameters.Add(AddStartHour);
                SqlParameter AddEndHour = new SqlParameter("@EndHour", EndHour);
                readCommand.Parameters.Add(AddEndHour);

                SqlParameter AddVehicleType = new SqlParameter("@VehicleType", VehicleType);
                readCommand.Parameters.Add(AddVehicleType);
                con.Open();

                SqlDataReader vehiclesReader = readCommand.ExecuteReader();

                if (vehiclesReader.HasRows)
                {
                    foundVehicles = new List<Vehicle>();
                    foundVehicles.Add(emptyVehicle);
                    while (vehiclesReader.Read())
                    {
                        readVehicles = GetVehiclesWithIntFromReader(vehiclesReader, false);
                        foundVehicles.Add(readVehicles);
                    }
                }
            }
            return foundVehicles;
        }

        //Denne metode får køretøjerne ud fra en valgt start dato til en slut dato, og sorter dem derefter.
        public List<Vehicle> getSortedVehiclesDay(string StartDate, string EndDate, string VehicleType)
        {
            List<Vehicle> foundVehicles = null;
            Vehicle readVehicles = null;
            Vehicle emptyVehicle = new Vehicle(0, 0, DateTime.Now);

            string queryString = "";
            if (string.IsNullOrEmpty(VehicleType) || VehicleType.Equals("Vehicle"))
            {
                queryString = "SELECT FORMAT(DateTimeStamp, 'yyyy-MM-dd') as DateTimeStamp, SUM(VehicleAmount) AS VehicleAmount FROM Vehicle WHERE FORMAT(DateTimeStamp,'yyyy-MM-dd') BETWEEN @StartDate and @EndDate GROUP BY FORMAT(DateTimeStamp,'yyyy-MM-dd')";
            }
            else
            {
                queryString = "SELECT FORMAT(DateTimeStamp, 'yyyy-MM-dd') as DateTimeStamp, SUM(VehicleAmount) AS VehicleAmount FROM Vehicle WHERE TypeName = @VehicleType and FORMAT(DateTimeStamp,'yyyy-MM-dd') BETWEEN @StartDate and @EndDate GROUP BY FORMAT(DateTimeStamp,'yyyy-MM-dd')";
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddStartDate = new SqlParameter("@StartDate", StartDate);
                readCommand.Parameters.Add(AddStartDate);
                SqlParameter AddEndDate = new SqlParameter("@EndDate", EndDate);
                readCommand.Parameters.Add(AddEndDate);
                SqlParameter AddVehicleType = new SqlParameter("@VehicleType", VehicleType);
                readCommand.Parameters.Add(AddVehicleType);
                con.Open();

                SqlDataReader vehiclesReader = readCommand.ExecuteReader();

                if (vehiclesReader.HasRows)
                {
                    foundVehicles = new List<Vehicle>();
                    foundVehicles.Add(emptyVehicle);
                    while (vehiclesReader.Read())
                    {
                        readVehicles = GetVehiclesFromReader(vehiclesReader, false);
                        foundVehicles.Add(readVehicles);
                    }
                }
            }
            return foundVehicles;
        }

        //Denne metode benyttes kun til test, da den tester om noget kan slettes.
        public int InsertToDBToDelete()
        {
            int vehiclesIdMade = 0;
            string queryString = "BEGIN TRANSACTION INSERT INTO Vehicle(TypeName, VehicleAmount, Feed, DateTimeStamp) VALUES('Vehicle', 150, 1, '2020/12/10'); COMMIT";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand readCommand = new SqlCommand(queryString, con))
                {
                    con.Open();
                    vehiclesIdMade = readCommand.ExecuteNonQuery();
                }
                return vehiclesIdMade;
            }
        }

        //Dog er denne metode benyttet til at slettes normalt, og ikke ligesom den ovenfor.
        public void DeleteFromDB(int toDelete)
        {
            System.IO.StringWriter writer = new System.IO.StringWriter();
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        using (SqlCommand cmdDeleteVehicles = con.CreateCommand())
                        {
                            cmdDeleteVehicles.CommandText = "DELETE FROM Vehicle WHERE Vehicle.VehicleId=VehicleId";
                            cmdDeleteVehicles.Parameters.AddWithValue("VehicleId", toDelete);
                            cmdDeleteVehicles.ExecuteNonQuery();
                        }
                    }
                    scope.Complete();
                }
            }
            catch (TransactionAbortedException ex)
            {
                writer.WriteLine("TransactionAbortedException Message: {0}", ex.Message);
            }
        }

        public List<String> CreateVehicleTypeList()
        {
            List<String> vehicleType = new List<String>();
            vehicleType.Add("Car");
            vehicleType.Add("Truck");
            vehicleType.Add("Motorcycle");
            return vehicleType;
        }

        public int GetARandomAmountNumber(int min, int max)
        {
            Random number = new Random(Guid.NewGuid().GetHashCode());
            int amount = number.Next(min, max);
            return amount;
        }

        public void CreateVehicleForTestData(DateTime now, string typeName)
        {
            int feed = GetARandomAmountNumber(1, 11);
            string queryString = "BEGIN TRANSACTION INSERT INTO Vehicle(TypeName, VehicleAmount, Feed, DateTimeStamp) VALUES(@typeName, @amount, @feed, @date); COMMIT";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand readCommand = new SqlCommand(queryString, con))
                {
                    SqlParameter TypeName = new SqlParameter("@typeName", typeName);
                    readCommand.Parameters.Add(TypeName);
                    SqlParameter AddAmount = new SqlParameter("@amount", 1);
                    readCommand.Parameters.Add(AddAmount);
                    SqlParameter AddFeed = new SqlParameter("@feed", feed);
                    readCommand.Parameters.Add(AddFeed);
                    SqlParameter AddLatestDate = new SqlParameter("@date", now);
                    readCommand.Parameters.Add(AddLatestDate);
                    con.Open();
                    readCommand.ExecuteNonQuery();
                }
            }

        }

        public void GenerateTestDataVehicles()
        {
            List<String> vehicleTypes = CreateVehicleTypeList();
            int randomIndex = GetARandomAmountNumber(10, 500);
            for (int i = 0; i < randomIndex; i++)
            {
                int ranNumber = GetARandomAmountNumber(0, 3);
                DateTime now = DateTime.Now;
                string selectedVehicleType = vehicleTypes[ranNumber];
                CreateVehicleForTestData(now, selectedVehicleType);
            }

        }

        //Denne metode henter de 7 seneste timer med køretøjer.
        public List<Vehicle> GetAllLatestVehiclesFromLatestDate()
        {
            List<Vehicle> foundVehicles = new List<Vehicle>();
            string queryString = "select Sum(VehicleAmount) as VehicleAmount, DATEPART(hour, DateTimeStamp) as Hour, FORMAT(DateTimeStamp,'yyyy-MM-dd') as DateTimeStamp from vehicle where FORMAT(DateTimeStamp,'yyyy-MM-dd') = (select(max(FORMAT(DateTimeStamp,'yyyy-MM-dd'))) from vehicle) and DATEPART(hour, DateTimeStamp) >= '00' and DATEPART(hour, DateTimeStamp) <= '23' group by DATEPART(hour, DateTimeStamp), FORMAT(DateTimeStamp,'yyyy-MM-dd')";
            //string queryString = "WITH SortedSeven AS (SELECT TOP 7 Vehicle.VehicleId, Vehicle.TypeName, Vehicle.VehicleAmount, Vehicle.Feed, Vehicle.DateTimeStamp FROM Vehicle ORDER BY VehicleId DESC) SELECT * FROM SortedSeven ORDER BY VehicleId";
            Vehicle emptyVehicle = new Vehicle(0, 0, DateTime.Now);

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                con.Open();

                SqlDataReader vehiclesReader = readCommand.ExecuteReader();

                if (vehiclesReader.HasRows)
                {
                    foundVehicles.Add(emptyVehicle);
                    while (vehiclesReader.Read())
                    {
                        Vehicle readVehicles = GetVehiclesWithIntFromReader(vehiclesReader, false);
                        foundVehicles.Add(readVehicles);
                    }
                }
            }
            return foundVehicles;
        }

        public Vehicle GetVehiclesFromReader(SqlDataReader vehiclesReader, bool check)
        {
            Vehicle foundVehicles;
            int tempvehicleid;
            int tempVehicleAmount;
            DateTime tempdatestamp;
            string dateTime;

            if (check)
            {

                tempvehicleid = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleId"));
                tempVehicleAmount = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleAmount"));
                tempdatestamp = (DateTime)vehiclesReader["DateTimeStamp"];


                foundVehicles = new Vehicle(tempvehicleid, tempVehicleAmount, tempdatestamp);
            }
            else
            {
                tempVehicleAmount = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleAmount"));
                dateTime = vehiclesReader["DateTimeStamp"].ToString();
                //tempdatestamp = vehiclesReader["DateTimeStamp"];

                foundVehicles = new Vehicle(tempVehicleAmount, dateTime);
            }
            return foundVehicles;
        }

        public Vehicle GetVehiclesWithIntFromReader(SqlDataReader vehiclesReader, bool check)
        {
            Vehicle foundVehicles = null;
            string dateToName;
            int tempVehicleAmount;
            int tempDateStamp;
            string tempGetHourFromClass;
            if (check)
            {
                tempVehicleAmount = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleAmount"));
                tempDateStamp = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("DateTimeStamp"));
                foundVehicles = new Vehicle(tempVehicleAmount, tempDateStamp.ToString());
            }
            else
            {
                tempVehicleAmount = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleAmount"));
                //dateToName = (DateTime)vehiclesReader["DateTimeStamp"];
                dateToName = vehiclesReader["DateTimeStamp"].ToString();
                tempGetHourFromClass = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("Hour")).ToString();
                //tempdatestamp = vehiclesReader["DateTimeStamp"];

                foundVehicles = new Vehicle(tempVehicleAmount, dateToName, tempGetHourFromClass);
            }

            return foundVehicles;
        }

        public Vehicle GetVehiclesWithAmount(SqlDataReader vehiclesReader)
        {
            Vehicle foundVehicles = null;
            int tempVehicleAmount = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleAmount"));
            foundVehicles = new Vehicle(tempVehicleAmount);


            return foundVehicles;
        }
    }
}