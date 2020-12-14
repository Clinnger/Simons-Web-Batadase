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

        //Denne metode får alle køretøjer.
        public List<Vehicle> getAllVehicles()
        {
            List<Vehicle> foundVehicles = null;
            Vehicle readVehicles = null;
            Vehicle emptyVehicle = new Vehicle(0, 0, DateTime.Now);

            string queryString = "SELECT Vehicle.VehicleId, Vehicle.TypeName, Vehicle.VehicleAmount, Vehicle.Feed, Vehicle.DateTimeStamp from Vehicle";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                con.Open();

                SqlDataReader vehiclesReader = readCommand.ExecuteReader();

                if (vehiclesReader.HasRows)
                {
                    foundVehicles = new List<Vehicle>();
                    foundVehicles.Add(emptyVehicle);
                    while (vehiclesReader.Read())
                    {
                        readVehicles = GetVehiclesFromReader(vehiclesReader, true);
                        foundVehicles.Add(readVehicles);
                    }
                }
            }
            return foundVehicles;
        }

        //Denne metode vælger alle køretøjer, hvor der er angivet en start time, slut time, start dato, og slut dato, og sorter derefter.
        public List<Vehicle> getSortedVehiclesDayAndHours(string StartHour, string EndHour, string StartDate, string EndDate, string VehicleType)
        {
            string start = StartDate + ' ' +  StartHour;
            string end = EndDate + ' ' + EndHour;
            List<Vehicle> foundVehicles = null;
            Vehicle readVehicles = null;
            Vehicle emptyVehicle = new Vehicle(0, 0, DateTime.Now);
            string queryString = "select Sum(VehicleAmount) as VehicleAmount, DATEPART(hour, DateTimeStamp) as Hour, FORMAT(DateTimeStamp,'yyyy-MM-dd') as DateTimeStamp from vehicle where FORMAT(DateTimeStamp,'yyyy-MM-dd') = (select(max(FORMAT(DateTimeStamp,'yyyy-MM-dd'))) from vehicle) group by DATEPART(hour, DateTimeStamp), FORMAT(DateTimeStamp,'yyyy-MM-dd')";
            //string queryString = "SELECT Vehicle.VehicleId, Vehicle.TypeName, Vehicle.VehicleAmount, Vehicle.Feed, Vehicle.DateTimeStamp, DatePart (YEAR, DateTimeStamp) AS Year, DatePart(DAY, DateTimeStamp)AS Day, DatePart(MONTH, DateTimeStamp) AS Month, DatePart(WEEK, DateTimeStamp) AS Week, DatePart(DAY, DateTimeStamp)AS Day, DatePart(HOUR, DateTimeStamp) AS Hour, DatePart(MINUTE, DateTimeStamp) AS Minute, DatePart(SECOND, DateTimeStamp) AS Second FROM Vehicle WHERE DateTimeStamp BETWEEN @StartDate AND @EndDate AND Vehicle.TypeName = @VehicleType";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddStartDate = new SqlParameter("@StartDate", start);
                readCommand.Parameters.Add(AddStartDate);
                SqlParameter AddEndDate = new SqlParameter("@EndDate", end);
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
                        readVehicles = GetVehiclesWithIntFromReader(vehiclesReader, false);
                        foundVehicles.Add(readVehicles);
                    }
                }
            }
            return foundVehicles;
        }

        //Denne metode får køretøjerne ud fra en valgt start dato til en slut dato, og sorter dem derefter.
        public List<Vehicle> getSortedVehiclesDay(string StartDate, string EndDate)
        {
            List<Vehicle> foundVehicles = null;
            Vehicle readVehicles = null;
            Vehicle emptyVehicle = new Vehicle(0, 0, DateTime.Now);
            string queryString = "SELECT FORMAT(DateTimeStamp, 'yyyy-MM-dd') as DateTimeStamp, SUM(VehicleAmount) AS VehicleAmount FROM Vehicle WHERE FORMAT(DateTimeStamp,'yyyy-MM-dd') BETWEEN @StartDate and @EndDate GROUP BY FORMAT(DateTimeStamp,'yyyy-MM-dd')";
            //string queryString = "SELECT Vehicle.DateTimeStamp AS DateTimeStamp, SUM(VehicleAmount) AS VehicleAmount FROM Vehicle WHERE Vehicle.DateTimeStamp BETWEEN convert(datetime, @StartDate, 5) AND convert(datetime, @EndDate, 5) GROUP BY DateTimeStamp";
            //string queryString = "SELECT Vehicle.DateTimeStamp AS DateTimeStamp, SUM(VehicleAmount) AS VehicleAmount FROM Vehicle WHERE Vehicle.DateTimeStamp BETWEEN @StartDate AND @EndDate GROUP BY Vehicle.DateTimeStamp";
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddStartDate = new SqlParameter("@StartDate", StartDate);
                readCommand.Parameters.Add(AddStartDate);
                SqlParameter AddEndDate = new SqlParameter("@EndDate", EndDate);
                readCommand.Parameters.Add(AddEndDate);
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

        public int FindVehicleType(string vehicleType)
        {
            int count = 0;
            DateTime now = DateTime.Now;
            string sDate = DateTime.Today.ToString("yyyy MM dd");
            string oDate = sDate.Replace(sDate.Split(' ')[1][0], char.ToUpper(sDate.Split(' ')[1][0]));
            string sHour = now.ToString("hh");
            string queryString = "SELECT COUNT(*) AS TOTAL FROM Vehicle WHERE TypeName = @vehicleType AND DateTimeStamp= @latestHour AND DateTimeStamp = @latestDate";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddLatestDate = new SqlParameter("@latestDate", oDate);
                readCommand.Parameters.Add(AddLatestDate);
                SqlParameter AddLatestHour = new SqlParameter("@latestHour", sHour);
                readCommand.Parameters.Add(AddLatestHour);
                SqlParameter AddVehicleType = new SqlParameter("@vehicleType", vehicleType);
                readCommand.Parameters.Add(AddVehicleType);
                con.Open();

                count = (int)readCommand.ExecuteScalar();
            }

            return count;
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

        public void GenerateTestDataVehicles()
        {
            int car = FindVehicleType("Car");
            int truck = FindVehicleType("Truck");
            int motorcycle = FindVehicleType("Motorcycle");
            
            int ranNumber = GetARandomAmountNumber(0, 3);
            List<String> vehicleTypes = CreateVehicleTypeList();
            string selectedVehicleType = vehicleTypes[ranNumber];
            DateTime now = DateTime.Now;
            string sDate = DateTime.Today.ToString("yyyy MM dd");
            string oDate = sDate.Replace(sDate.Split(' ')[1][0], char.ToUpper(sDate.Split(' ')[1][0]));
            string sHour = now.ToString("hh");
            if (car == 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION INSERT INTO Vehicle(TypeName, VehicleAmount, Feed, DateTimeStamp) VALUES('Car', @amount, @feed, @latestDate); COMMIT";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand readCommand = new SqlCommand(queryString, con))
                    {
                        SqlParameter AddAmount = new SqlParameter("@amount", amount);
                        readCommand.Parameters.Add(AddAmount);
                        SqlParameter AddFeed = new SqlParameter("@feed", feed);
                        readCommand.Parameters.Add(AddFeed);
                        SqlParameter AddLatestDate = new SqlParameter("@latestDate", oDate);
                        readCommand.Parameters.Add(AddLatestDate);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
            else if (car != 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION UPDATE Vehicle SET VehicleAmount = @amount, Feed = @feed WHERE DateTimeStamp = @latestDate AND TypeName = 'Car'; COMMIT";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand readCommand = new SqlCommand(queryString, con))
                    {
                        SqlParameter AddAmount = new SqlParameter("@amount", amount);
                        readCommand.Parameters.Add(AddAmount);
                        SqlParameter AddFeed = new SqlParameter("@feed", feed);
                        readCommand.Parameters.Add(AddFeed);
                        SqlParameter AddLatestDate = new SqlParameter("@latestDate", oDate);
                        readCommand.Parameters.Add(AddLatestDate);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
            if (truck == 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION INSERT INTO Vehicle(TypeName, VehicleAmount, Feed, DateTimeStamp) VALUES('Truck', @amount, @feed, @latestDate); COMMIT";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand readCommand = new SqlCommand(queryString, con))
                    {
                        SqlParameter AddAmount = new SqlParameter("@amount", amount);
                        readCommand.Parameters.Add(AddAmount);
                        SqlParameter AddFeed = new SqlParameter("@feed", feed);
                        readCommand.Parameters.Add(AddFeed);
                        SqlParameter AddLatestDate = new SqlParameter("@latestDate", oDate);
                        readCommand.Parameters.Add(AddLatestDate);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
            else if (truck != 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION UPDATE Vehicle SET VehicleAmount = @amount, Feed = @feed WHERE DateTimeStamp = @latestDate AND TypeName = 'Truck'; COMMIT";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand readCommand = new SqlCommand(queryString, con))
                    {
                        SqlParameter AddAmount = new SqlParameter("@amount", amount);
                        readCommand.Parameters.Add(AddAmount);
                        SqlParameter AddFeed = new SqlParameter("@feed", feed);
                        readCommand.Parameters.Add(AddFeed);
                        SqlParameter AddLatestDate = new SqlParameter("@latestDate", oDate);
                        readCommand.Parameters.Add(AddLatestDate);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
            if (motorcycle == 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION INSERT INTO Vehicle(TypeName, VehicleAmount, Feed, DateTimeStamp) VALUES('Motorcycle', @amount, @feed, @latestDate); COMMIT";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand readCommand = new SqlCommand(queryString, con))
                    {
                        SqlParameter AddAmount = new SqlParameter("@amount", amount);
                        readCommand.Parameters.Add(AddAmount);
                        SqlParameter AddFeed = new SqlParameter("@feed", feed);
                        readCommand.Parameters.Add(AddFeed);
                        SqlParameter AddLatestDate = new SqlParameter("@latestDate", oDate);
                        readCommand.Parameters.Add(AddLatestDate);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
            else if (motorcycle != 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION UPDATE Vehicle SET VehicleAmount = @amount, Feed = @feed WHERE DateTimeStamp = @latestDate AND TypeName = 'Motorcycle'; COMMIT";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand readCommand = new SqlCommand(queryString, con))
                    {
                        SqlParameter AddAmount = new SqlParameter("@amount", amount);
                        readCommand.Parameters.Add(AddAmount);
                        SqlParameter AddFeed = new SqlParameter("@feed", feed);
                        readCommand.Parameters.Add(AddFeed);
                        SqlParameter AddLatestDate = new SqlParameter("@latestDate", oDate);
                        readCommand.Parameters.Add(AddLatestDate);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        //Denne metode henter de 7 seneste timer med køretøjer.
        public List<Vehicle> GetAllLatestVehiclesFromLatestHour()
        {
            List<Vehicle> foundVehicles = new List<Vehicle>();
            string queryString = "select Sum(VehicleAmount) as VehicleAmount, DATEPART(hour, DateTimeStamp) as Hour, FORMAT(DateTimeStamp,'yyyy-MM-dd') as DateTimeStamp from vehicle where FORMAT(DateTimeStamp,'yyyy-MM-dd') = (select(max(FORMAT(DateTimeStamp,'yyyy-MM-dd'))) from vehicle) and DATEPART(hour, DateTimeStamp) >= '08' and DATEPART(hour, DateTimeStamp) <= '20' group by DATEPART(hour, DateTimeStamp), FORMAT(DateTimeStamp,'yyyy-MM-dd')";
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
                        Vehicle readVehicles = GetVehiclesFromReader(vehiclesReader, false);
                        foundVehicles.Add(readVehicles);
                    }
                }
            }
            return foundVehicles;
        }

        /*
        //En loop igennem uger.
        public List<Vehicle> LoopThroughWeeks()
        {
            List<Vehicle> foundVehicles = new List<Vehicle>();
            Vehicle emptyVehicle = new Vehicle(0, 0, DateTime.Now);
            foundVehicles.Add(emptyVehicle);
            //List<string> Weeks = GetAllWeekNumbers();
            int index = 0;

            while (index < Weeks.Count())
            {
                int Index = Int32.Parse(Weeks[index]);
                Vehicle vehicle = SortByWeeks(Index);
                foundVehicles.Add(vehicle);
                index++;
            }

            return foundVehicles;

        }
        */

        //Sorter efter uger.
        public List<Vehicle> SortByWeeks(string StartDate, string EndDate)
        {
            List<Vehicle> readVehicles = new List<Vehicle>();
            string queryString = "SELECT SUM(VehicleAmount) AS VehicleAmount, DATEPART(ww, DateTimeStamp) AS DateTimeStamp FROM Vehicle WHERE DATEPART(ww, DateTimeStamp) BETWEEN DATEPART(WEEK, @StartDate) AND DATEPART(WEEK, @EndDate) group by DATEPART(ww, DateTimeStamp)";
            //string queryString = "SELECT SUM(VehicleAmount) AS VehicleAmount FROM Vehicle WHERE TypeName = @VehicleType and DatePart(WEEK, DateTimeStamp) = @index";
            //string queryString = "SELECT SUM(VehicleAmount) AS VehicleAmount, WeekNumber as WeekNumber FROM Vehicle WHERE WeekNumber = @index Group By WeekNumber";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddStartDate = new SqlParameter("@StartDate", StartDate);
                readCommand.Parameters.Add(AddStartDate);
                SqlParameter AddEndDate = new SqlParameter("@EndDate", EndDate);
                readCommand.Parameters.Add(AddEndDate);
                //SqlParameter AddWeek = new SqlParameter("@index", index);
                //readCommand.Parameters.Add(AddWeek);
                con.Open();

                SqlDataReader vehiclesReader = readCommand.ExecuteReader();

                if (vehiclesReader.HasRows)
                {
                    while (vehiclesReader.Read())
                    {
                        Vehicle aVehicle =  GetVehiclesWithIntFromReader(vehiclesReader, true);
                        readVehicles.Add(aVehicle);
                    }
                }
            }
            return readVehicles;
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
    }
}