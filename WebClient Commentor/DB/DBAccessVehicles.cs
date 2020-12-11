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
            Vehicle emptyVehicle = new Vehicle(0, 0, "Start", "");

            string queryString = "SELECT Vehicle.VehicleId, Vehicle.TypeName, Vehicle.VehicleAmount, Vehicle.Feed, Vehicle.DateStamp, Vehicle.WeekNumber, Vehicle.HourStamp from Vehicle";

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

        //Gammel metode som ikke bruges, men sorteres efter valgt start time og slut time.
        public List<Vehicle> getSortedVehiclesHour(string StartHour, string EndHour)
        {
            List<Vehicle> foundVehicles = null;
            Vehicle readVehicles = null;
            Vehicle emptyVehicle = new Vehicle(0, 0, "Start", "");
            string queryString = "SELECT Vehicle.VehicleId, Vehicle.TypeName, Vehicle.VehicleAmount, Vehicle.Feed, Vehicle.DateStamp, Vehicle.WeekNumber Vehicle.HourStamp from Vehicle WHERE Vehicle.HourStamp BETWEEN @StartHour AND @EndHour";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                try
                {
                    SqlParameter AddStartHour = new SqlParameter("@StartHour", StartHour);
                    readCommand.Parameters.Add(AddStartHour);
                    SqlParameter AddEndHour = new SqlParameter("@EndHour", EndHour);
                    readCommand.Parameters.Add(AddEndHour);
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
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                return foundVehicles;
            }
        }

        //Denne metode vælger alle køretøjer, hvor der er angivet en start time, slut time, start dato, og slut dato, og sorter derefter.
        public List<Vehicle> getSortedVehiclesDayAndHours(string StartHour, string EndHour, string StartDate, string EndDate)
        {
            List<Vehicle> foundVehicles = null;
            Vehicle readVehicles = null;
            Vehicle emptyVehicle = new Vehicle(0, 0, "Start", "");
            string queryString = "SELECT Vehicle.VehicleId, Vehicle.TypeName, Vehicle.VehicleAmount, Vehicle.Feed, Vehicle.DateStamp, Vehicle.WeekNumber, Vehicle.HourStamp from Vehicle WHERE Vehicle.HourStamp BETWEEN @StartHour AND @EndHour AND Vehicle.DateStamp BETWEEN @StartDate AND @EndDate";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddStartHour = new SqlParameter("@StartHour", StartHour);
                readCommand.Parameters.Add(AddStartHour);
                SqlParameter AddEndHour = new SqlParameter("@EndHour", EndHour);
                readCommand.Parameters.Add(AddEndHour);
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
                        readVehicles = GetVehiclesFromReader(vehiclesReader, true);
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
            Vehicle emptyVehicle = new Vehicle(0, 0, "Start", "");
            string queryString = "SELECT Vehicle.DateStamp AS DateStamp, SUM(VehicleAmount) AS VehicleAmount FROM Vehicle WHERE Vehicle.DateStamp BETWEEN @StartDate AND @EndDate GROUP BY Vehicle.DateStamp";
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

        public int FindLatestVehicleId()
        {
            int vehiclesIdToDelete = 0;
            string queryString = "SELECT MAX(VehicleId) AS VehicleId from Vehicle ";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                con.Open();

                vehiclesIdToDelete = (int)readCommand.ExecuteScalar();

            }
            return vehiclesIdToDelete;
        }

        //Denne metode benyttes kun til test, da den tester om noget kan slettes.
        public int InsertToDBToDelete()
        {
            int vehiclesIdMade = 0;
            string queryString = "BEGIN TRANSACTION INSERT INTO Vehicle(TypeName, VehicleAmount, Feed, DateStamp, WeekNumber, HourStamp) VALUES('Vehicle', 150, 1, '10 Dec 2020', 50, '10'); COMMIT";
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
        public String GetLatestDateFromLatestVehicleId()
        {
            String vehicleId = "";
            String queryString = "SELECT Vehicle.DateStamp from Vehicle WHERE Vehicle.VehicleId=(SELECT max(Vehicle.VehicleId) from Vehicle)";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                con.Open();

                vehicleId = (String)readCommand.ExecuteScalar();

            }
            return vehicleId;

        }
        public String GetLatestHourFromLatestVehicleId()
        {
            String vehicleId = "";
            String queryString = "SELECT Vehicle.HourStamp from Vehicle WHERE Vehicle.VehicleId=(SELECT max(Vehicle.VehicleId) from Vehicle)";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                con.Open();

                vehicleId = (String)readCommand.ExecuteScalar();

            }
            return vehicleId;
        }

        public int FindVehicleType(String vehicleType)
        {
            int count = 0;
            DateTime now = DateTime.Now;
            string sDate = DateTime.Today.ToString("dd MMM yyyy");
            string oDate = sDate.Replace(sDate.Split(' ')[1][0], char.ToUpper(sDate.Split(' ')[1][0]));
            string sHour = now.ToString("hh");
            string queryString = "SELECT count(*) as total from Vehicle where TypeName = @vehicleType and HourStamp = @latestHour and DateStamp = @latestDate";

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
        public static int GetWeekNumber(DateTime now)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            int weekNumber = ci.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNumber;
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
            string sDate = DateTime.Today.ToString("dd MMM yyyy");
            string oDate = sDate.Replace(sDate.Split(' ')[1][0], char.ToUpper(sDate.Split(' ')[1][0]));
            string sHour = now.ToString("hh");
            var weekNumber = GetWeekNumber(now);
            if (car == 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION INSERT INTO Vehicle(TypeName, VehicleAmount, Feed, DateStamp, WeekNumber, HourStamp) VALUES('Car', @amount, @feed, @latestDate, @weekNumber, @latestHour); COMMIT";
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
                        SqlParameter AddWeekNumber = new SqlParameter("@weekNumber", weekNumber);
                        readCommand.Parameters.Add(AddWeekNumber);
                        SqlParameter AddLatestHour = new SqlParameter("@latestHour", sHour);
                        readCommand.Parameters.Add(AddLatestHour);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
            else if (car != 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION UPDATE Vehicle SET VehicleAmount = @amount, Feed = @feed WHERE DateStamp = @latestDate AND HourStamp = @latestHour AND TypeName = 'Car'; COMMIT";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand readCommand = new SqlCommand(queryString, con))
                    {
                        SqlParameter AddAmount = new SqlParameter("@amount", amount);
                        readCommand.Parameters.Add(AddAmount);
                        SqlParameter AddFeed = new SqlParameter("@feed", feed);
                        readCommand.Parameters.Add(AddFeed);
                        SqlParameter AddLatestHour = new SqlParameter("@latestHour", sHour);
                        readCommand.Parameters.Add(AddLatestHour);
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
                string queryString = "BEGIN TRANSACTION INSERT INTO Vehicle(TypeName, VehicleAmount, Feed, DateStamp, WeekNumber, HourStamp) VALUES('Truck', @amount, @feed, @latestDate, @weekNumber, @latestHour); COMMIT";
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
                        SqlParameter AddWeekNumber = new SqlParameter("@weekNumber", weekNumber);
                        readCommand.Parameters.Add(AddWeekNumber);
                        SqlParameter AddLatestHour = new SqlParameter("@latestHour", sHour);
                        readCommand.Parameters.Add(AddLatestHour);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
            else if (truck != 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION UPDATE Vehicle SET VehicleAmount = @amount, Feed = @feed WHERE DateStamp = @latestDate AND HourStamp = @latestHour AND TypeName = 'Truck'; COMMIT";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand readCommand = new SqlCommand(queryString, con))
                    {
                        SqlParameter AddAmount = new SqlParameter("@amount", amount);
                        readCommand.Parameters.Add(AddAmount);
                        SqlParameter AddFeed = new SqlParameter("@feed", feed);
                        readCommand.Parameters.Add(AddFeed);
                        SqlParameter AddLatestHour = new SqlParameter("@latestHour", sHour);
                        readCommand.Parameters.Add(AddLatestHour);
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
                string queryString = "BEGIN TRANSACTION INSERT INTO Vehicle(TypeName, VehicleAmount, Feed, DateStamp, WeekNumber, HourStamp) VALUES('Motorcycle', @amount, @feed, @latestDate, @weekNumber, @latestHour); COMMIT";
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
                        SqlParameter AddWeekNumber = new SqlParameter("@weekNumber", weekNumber);
                        readCommand.Parameters.Add(AddWeekNumber);
                        SqlParameter AddLatestHour = new SqlParameter("@latestHour", sHour);
                        readCommand.Parameters.Add(AddLatestHour);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
            else if (motorcycle != 0)
            {
                int amount = GetARandomAmountNumber(1, 2001);
                int feed = GetARandomAmountNumber(1, 11);
                string queryString = "BEGIN TRANSACTION UPDATE Vehicle SET VehicleAmount = @amount, Feed = @feed WHERE DateStamp = @latestDate AND HourStamp = @latestHour AND TypeName = 'Motorcycle'; COMMIT";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand readCommand = new SqlCommand(queryString, con))
                    {
                        SqlParameter AddAmount = new SqlParameter("@amount", amount);
                        readCommand.Parameters.Add(AddAmount);
                        SqlParameter AddFeed = new SqlParameter("@feed", feed);
                        readCommand.Parameters.Add(AddFeed);
                        SqlParameter AddLatestHour = new SqlParameter("@latestHour", sHour);
                        readCommand.Parameters.Add(AddLatestHour);
                        SqlParameter AddLatestDate = new SqlParameter("@latestDate", oDate);
                        readCommand.Parameters.Add(AddLatestDate);
                        con.Open();
                        readCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        //Denne metode får alle biler, som sådan set nu er alle køretøjer som den henter fra seneste dato.
        public List<Vehicle> GetAllVehiclesByLatestDate()
        {
            List<Vehicle> foundVehicles = null;
            Vehicle readVehicles = null;
            string queryString = "SELECT Vehicle.VehicleId, Vehicle.TypeName, Vehicle.VehicleAmount, Vehicle.Feed, Vehicle.DateStamp, Vehicle.WeekNumber, Vehicle.HourStamp from Vehicle WHERE Vehicle.DateStamp = @latestDate";
            Vehicle emptyVehicle = new Vehicle(0, 0, "Start", "");

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddLatestDate = new SqlParameter("@latestDate", GetLatestDateFromLatestVehicleId());
                readCommand.Parameters.Add(AddLatestDate);
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

        //Denne metode henter de 7 seneste timer med biler.
        public List<Vehicle> Get7LatestVehicles()
        {
            List<Vehicle> foundVehicles = null;
            Vehicle readVehicles = null;
            string queryString = "WITH SortedSeven AS (SELECT TOP 7 Vehicle.VehicleId, Vehicle.TypeName, Vehicle.VehicleAmount, Vehicle.Feed, Vehicle.DateStamp, Vehicle.WeekNumber, Vehicle.HourStamp from Vehicle ORDER BY VehicleId DESC) SELECT * FROM SortedSeven ORDER BY VehicleId";
            Vehicle emptyVehicle = new Vehicle(0, 0, "Start", "");

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

        //En loop igennem uger.
        public List<Vehicle> LoopThroughWeeks()
        {
            List<Vehicle> foundVehicles = new List<Vehicle>();
            Vehicle emptyVehicle = new Vehicle(0, 0, "Start", "");
            foundVehicles.Add(emptyVehicle);
            List<string> Weeks = GetAllWeekNumbers();
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

        //Sorter efter uger.
        public Vehicle SortByWeeks(int index)
        {
            Vehicle readVehicles = null;
            string queryString = "SELECT SUM(VehicleAmount) AS VehicleAmount, WeekNumber as WeekNumber FROM Vehicle WHERE WeekNumber = @index Group By WeekNumber";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                SqlParameter AddWeek = new SqlParameter("@index", index);
                readCommand.Parameters.Add(AddWeek);
                con.Open();

                SqlDataReader vehiclesReader = readCommand.ExecuteReader();

                if (vehiclesReader.HasRows)
                {
                    while (vehiclesReader.Read())
                    {
                        readVehicles = GetVehiclesWithIntFromReader(vehiclesReader);
                    }
                }
            }
            return readVehicles;
        }

        //Får alle ugens numre.
        public List<string> GetAllWeekNumbers()
        {
            List<string> WeekNumbers = new List<string>();
            int number = 0;
            string queryString = "SELECT DISTINCT WeekNumber FROM Vehicle";
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand readCommand = new SqlCommand(queryString, con))
            {
                con.Open();

                SqlDataReader vehiclesReader = readCommand.ExecuteReader();

                if (vehiclesReader.HasRows)
                {
                    while (vehiclesReader.Read())
                    {
                        number = GetWeekNumberFromTable(vehiclesReader);
                        string myDate = number.ToString();
                        WeekNumbers.Add(myDate);
                    }
                }
            }
            return WeekNumbers;
        }

        public int GetWeekNumberFromTable(SqlDataReader dataReader)
        {
            int weekNumber;

            weekNumber = dataReader.GetInt32(dataReader.GetOrdinal("WeekNumber"));

            return weekNumber;
        }

        public Vehicle GetVehiclesFromReader(SqlDataReader vehiclesReader, bool check)
        {
            Vehicle foundVehicles;
            int tempvehicleid;
            int tempVehicleAmount;
            string tempdatestamp;
            string temphourstamp;

            if (check)
            {

                tempvehicleid = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleId"));
                tempVehicleAmount = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleAmount"));
                tempdatestamp = vehiclesReader.GetString(vehiclesReader.GetOrdinal("DateStamp"));
                temphourstamp = vehiclesReader.GetString(vehiclesReader.GetOrdinal("HourStamp"));

                foundVehicles = new Vehicle(tempvehicleid, tempVehicleAmount, tempdatestamp, temphourstamp);
            }
            else
            {
                tempVehicleAmount = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleAmount"));
                tempdatestamp = vehiclesReader.GetString(vehiclesReader.GetOrdinal("DateStamp"));
                temphourstamp = "";

                foundVehicles = new Vehicle(tempVehicleAmount, tempdatestamp, temphourstamp);
            }
            return foundVehicles;
        }

        public Vehicle GetVehiclesWithIntFromReader(SqlDataReader vehiclesReader)
        {
            Vehicle foundVehicles;
            int tempVehicleAmount;
            string tempdatestamp;

            tempVehicleAmount = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("VehicleAmount"));
            int test = vehiclesReader.GetInt32(vehiclesReader.GetOrdinal("WeekNumber"));
            string myDate = test.ToString();
            tempdatestamp = myDate;

            foundVehicles = new Vehicle(tempVehicleAmount, tempdatestamp, "");
            return foundVehicles;
        }

    }
}