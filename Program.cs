//reference: https://docs.microsoft.com/tr-tr/dotnet/api/system.data.datatable?view=net-6.0

using System;
using System.Data;
using System.Globalization;

class Program
{

    static string startDate = "";
    static string endDate = "";
    static CultureInfo provider = CultureInfo.InvariantCulture;
    static CultureInfo currentCulture = CultureInfo.CurrentCulture;

    static void Main(string[] args)
    {
        DataTable groupTable = CreateGroupTable();
        DataSet tableDataSet = new DataSet();

        tableDataSet.Tables.Add(groupTable);

        // Insert the rows into the table
        InsertTableDetails(groupTable);

        Console.WriteLine("The initial table.");
        ShowTable(groupTable);

        //Ask Date range
        Console.WriteLine("Enter Start Date YYYY-MM-DD: (Press Enter to Save) ");
        startDate = Console.ReadLine();
        Console.WriteLine("Enter End Date YYYY-MM-DD: (Press Enter to Save) ");
        endDate = Console.ReadLine();

        Console.WriteLine("Date Range :" + startDate + "  --- " + endDate);
        Console.WriteLine();


        TimeSpan difference = DateTime.Parse(endDate) - DateTime.Parse(startDate);
        int number = (int)difference.TotalDays;

        Console.WriteLine(number.ToString() + " --- Days");

        switch (number)
        {
            case int n when (n <= 15):
                ModifyTable(1, groupTable);
                break;
            case int n when (n > 15 && n <= 30):
                ModifyTable(2, groupTable);
                break;
            case int n when (n > 30):
                ModifyTable(3, groupTable);
                break;

        }
        Console.WriteLine("Press any key to exit.....");
        Console.ReadKey();
    }

    private static DataTable CreateGroupTable()
    {
        DataTable grouppTable = new DataTable("GroupTable");

        // Define all the columns once.
        DataColumn[] cols ={
                                  new DataColumn("Id",typeof(Int32)),
                                  new DataColumn("Names",typeof(String)),
                                  new DataColumn("Date",typeof(String)),
                                  new DataColumn("Values",typeof(Int32)),
                              };

        grouppTable.Columns.AddRange(cols);
        grouppTable.PrimaryKey = new DataColumn[] { grouppTable.Columns["Id"] };
        return grouppTable;
    }

    private static void InsertTableDetails(DataTable Table1)
    {
        Random rnd = new Random();
        // Use an Object array to insert all the rows .
        // Values in the array are matched sequentially to the columns, based on the order in which they appear in the table.
        Object[] rows = {
                                 new Object[]{1,"Anna","2022-01-01",rnd.Next(1, 100)},
                                 new Object[]{2,"Barbara","2022-01-01",rnd.Next(1, 100)},
                                 new Object[]{3,"Claire","2022-02-01",rnd.Next(1, 100)},
                                 new Object[]{4,"Dev","2022-02-01",rnd.Next(1, 100)},
                                 new Object[]{5,"Enya","2022-01-15",rnd.Next(1, 100)},
                                 new Object[]{6,"Freya","2022-01-15",rnd.Next(1, 100)},
                                 new Object[]{7,"Gillman","2022-03-01",rnd.Next(1, 100)},
                                 new Object[]{8,"Henry","2022-03-01",rnd.Next(1, 100)},
                                 new Object[]{9,"Inna","2022-01-29",rnd.Next(1, 100)},
                             };

        foreach (Object[] row in rows)
        {
            Table1.Rows.Add(row);
        }
    }

    private static void ShowTable(DataTable table)
    {
        foreach (DataColumn col in table.Columns)
        {
            Console.Write("{0,-14}", col.ColumnName);
        }
        Console.WriteLine();

        foreach (DataRow row in table.Rows)
        {
            foreach (DataColumn col in table.Columns)
            {
                if (col.DataType.Equals(typeof(DateTime)))
                    Console.Write("{0,-14:d}", row[col]);
                else if (col.DataType.Equals(typeof(Decimal)))
                    Console.Write("{0,-14:C}", row[col]);
                else
                    Console.Write("{0,-14}", row[col]);
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    private static void ModifyTable(int option, DataTable table) //days
    {

        DataTable modifiedTable = new DataTable();

        string[] uniqueDates = table.AsEnumerable().Select(x => x.Field<string>("Date")).Distinct().ToArray();

        //first Column
        modifiedTable.Columns.Add("Names", typeof(string));


        switch (option)
        {
            case 1: // <=15 days
                    //add days column
                foreach (DateTime day in EachDay(startDate, endDate))
                    modifiedTable.Columns.Add(day.ToString("yyyy-MM-dd"), typeof(string));
                break;
            case 2: // between 16-30 days


                // first cahnge datetime str to week numbers to match
                for (int index = 0; index < table.Rows.Count; index++)
                {
                    table.Rows[index]["Date"] = "Week " + currentCulture.Calendar.GetWeekOfYear(DateTime.ParseExact(table.Rows[index]["Date"].ToString(), "yyyy-MM-dd", provider), currentCulture.DateTimeFormat.CalendarWeekRule, currentCulture.DateTimeFormat.FirstDayOfWeek);

                }

                //now add new week columns to modifiedTable
                foreach (var week in WeeksBetween(startDate, endDate))
                {
                    modifiedTable.Columns.Add(week.ToString(), typeof(string));
                }

                break;

            case 3: // +30 days

                // first cahnge datetime str to Months to match
                for (int index = 0; index < table.Rows.Count; index++)
                {
                    table.Rows[index]["Date"] = DateTime.ParseExact(table.Rows[index]["Date"].ToString(), "yyyy-MM-dd", provider).ToString("MMMM", CultureInfo.InvariantCulture);

                }

                //now add new week columns to modifiedTable
                foreach (var month in MonthsBetween(startDate, endDate))
                {
                    modifiedTable.Columns.Add(month.ToString(), typeof(string));
                }

                break;
        }


        var groups = table.AsEnumerable().GroupBy(x => x.Field<string>("Names"));


        foreach (var group in groups)
        {
            DataRow newRow = modifiedTable.Rows.Add();
            foreach (DataRow row in group)
            {
                newRow["Names"] = group.Key;
                try
                {
                    newRow[row.Field<string>("Date")] = row.Field<int>("Values");
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        Console.WriteLine("After.....");
        ShowTable(modifiedTable);

    }

    public static IEnumerable<DateTime> EachDay(string startD, string endD)
    {

        DateTime from = DateTime.ParseExact(startD, "yyyy-MM-dd", provider);
        DateTime thru = DateTime.ParseExact(endD, "yyyy-MM-dd", provider);

        for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            yield return day;
    }

    // This presumes that weeks start with Monday.
    // reference: https://stackoverflow.com/questions/11154673/get-the-correct-week-number-of-a-given-date
    public static int GetIso8601WeekOfYear(string timeStr)
    {
        DateTime time = DateTime.ParseExact(timeStr, "yyyy-MM-dd", provider);
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Sunday)
        {
            time = time.AddDays(3);
        }

        // Return the week of our adjusted day
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    //reference: https://stackoverflow.com/questions/11930565/list-the-months-between-two-dates/11930768
    public static IEnumerable<string> MonthsBetween(string startD, string endD)
    {
        DateTime startDate = DateTime.ParseExact(startD, "yyyy-MM-dd", provider);
        DateTime endDate = DateTime.ParseExact(endD, "yyyy-MM-dd", provider);

        DateTime iterator;
        DateTime limit;


        iterator = new DateTime(startDate.Year, startDate.Month, 1);
        limit = endDate;

        var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        while (iterator <= limit)
        {
            yield return (dateTimeFormat.GetMonthName(iterator.Month));
            iterator = iterator.AddMonths(1);
        }
    }

    //reference: https://stackoverflow.com/questions/25247696/how-do-i-find-week-numbers-of-a-given-date-range-in-c-sharp
    public static IEnumerable<string> WeeksBetween(string startD, string endD)
    {
        DateTime d1 = DateTime.ParseExact(startD, "yyyy-MM-dd", provider);
        DateTime d2 = DateTime.ParseExact(endD, "yyyy-MM-dd", provider);


        var weeks = new List<string>();

        for (var dt = d1; dt < d2; dt = dt.AddDays(1))
        {
            var weekNo = currentCulture.Calendar.GetWeekOfYear(
                                  dt,
                                  currentCulture.DateTimeFormat.CalendarWeekRule,
                                  currentCulture.DateTimeFormat.FirstDayOfWeek);
            if (!weeks.Contains("Week " + weekNo))
                weeks.Add("Week " + weekNo);
        }

        return weeks;
    }



}
