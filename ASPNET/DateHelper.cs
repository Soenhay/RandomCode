using System;
using System.Collections.Generic;

namespace Common
{
    public static class DateHelper
    {
        /// <summary>
        /// Object that contains a FromDate and a ToDate. The constructor ensures that the dates are in the proper order.
        /// </summary>
        public class DateRange
        {
            public DateTime FromDate;
            public DateTime ToDate;

            /// <summary>
            /// Creates a DateRange object with DateTime.Now for both dates.
            /// </summary>
            public DateRange() 
                : this(DateTime.Now, DateTime.Now)
            {
            }

            public DateRange(DateTime date)
                : this(date, date)
            {
            }

            /// <summary>
            /// Creates a DateRange object. Also makes sure that the dates are in chronological order.
            /// </summary>
            /// <param name="fromDate"></param>
            /// <param name="toDate"></param>
            public DateRange(DateTime fromDate, DateTime toDate)
            {
                this.FromDate = fromDate;
                this.ToDate = toDate;

                if (FromDate > ToDate)
                {
                    DateTime temp = FromDate;
                    FromDate = ToDate;
                    ToDate = temp;
                }
            }

            /// <summary>
            /// <para>Return "[FromDate=" + FromDate.ToString() + "][ToDate=" + ToDate.ToString() + "]"</para>
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return "[FromDate=" + FromDate.ToString() + "][ToDate=" + ToDate.ToString() + "]";
            }

            public List<DateTime> ToList()
            {
                return (new List<DateTime>()).AddRangeUnique(new DateTime[] { FromDate, ToDate });
            }
        }

        /// <summary>
        /// Sets the date to a minimum date of 1/1/1901 if it is not 
        /// between the SMALLDATETIME range of 1/1/1900 12:00:00 AM and 6/6/2079 11:59:59 PM. 
        /// <para>We are setting it to 1901 so we know it was out of range.</para>
        /// </summary>
        /// <param name="theDate"></param>
        /// <returns></returns>
        public static DateTime EnsureDateIsSafeForSQL(DateTime theDate)
        {
            if (theDate.isSafeForSQL_SmallDateTime())
            {
                return theDate;
            }
            else
            {
                return new DateTime(1901, 1, 1);
            }
        }

        /// <summary>
        /// Sets the date to a minimum date of 1/1/1901 if it is not 
        /// between the SMALLDATETIME range of 1/1/1900 12:00:00 AM and 6/6/2079 11:59:59 PM. 
        /// <para>We are setting it to 1901 so we know it was out of range.</para>
        /// </summary>
        /// <param name="theDate"></param>
        /// <param name="defaultDate">Returns defaultDate if theDate is out range.</param>
        /// <returns>Returns theDate if it is in range. Returns defaultDate if theDate is out range.</returns>
        public static DateTime EnsureDateIsSafeForSQL(DateTime theDate, DateTime defaultDate)
        {
            if (theDate.isSafeForSQL_SmallDateTime())
            {
                return theDate;
            }
            else
            {
                return new DateTime(1901, 1, 1);
            }
        }

        /// <summary>
        /// Convert a single date by the offsetUTC and ensure that the date is safe for SQL.
        /// </summary>
        /// <param name="theDate"></param>
        /// <param name="offsetUTC"></param>
        /// <returns></returns>
        public static DateTime adjustDateByOffsetUTC(DateTime theDate, Int16 offsetUTC)
        {
            theDate = DateHelper.EnsureDateIsSafeForSQL(theDate);//Sometimes we get dates like 1/1/0001 so put it in range if necessary.
            theDate = theDate.AddHours(offsetUTC); //Apply the offset.
            theDate = DateHelper.EnsureDateIsSafeForSQL(theDate);//If the date was previously set to the min date now it is out of range so put it back.
            return theDate;
        }

        /// <summary>
        /// Returns the difference in minutes between the times. Checks to see if start is less than end. If not then switches them.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Double GetMinutesDiff(DateTime start, DateTime end)
        {
            return GetMinutesDiff(start, end, true);
        }

        /// <summary>
        /// Returns the difference in minutes between the times. Checks to see if start is less than end. If not then switches them.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="fixOrder">Whether or not to make sure the dates are in chronological order before getting the difference.</param>
        /// <returns></returns>
        public static Double GetMinutesDiff(DateTime start, DateTime end, bool fixOrder)
        {
            if (fixOrder)
            {
                if (start > end)
                {
                    DateTime temp = start;
                    start = end;
                    end = temp;
                }
            }

            return (end - start).TotalMinutes;
        }

        public static DateRange DateListToDateRange(List<DateTime> listDates)
        {
            DateRange dates;

            if (listDates.Count == 1)
            {
                dates = new DateHelper.DateRange(listDates[0], listDates[0]);
            }
            else if (listDates.Count == 2)
            {
                dates = new DateHelper.DateRange(listDates[0], listDates[1]);//Automaticaly orders the dates.
            }
            else
            {
                //(listDates == null || listDates.Count == 0 || listDates.Count > 2)
                dates = new DateHelper.DateRange();//Create with DateTime.Now for both dates.
            }

            return dates;
        }
    }

    public static class DateTimeExtensions
    {
        public static string Quarter_String(this DateTime obj)
        {
            switch (obj.Quarter_Int())
            {
                default:
                case 1: return "Q1";
                case 2: return "Q2";
                case 3: return "Q3";
                case 4: return "Q4";
            }
        }

        public static int Quarter_Int(this DateTime obj)
        {
            return ((obj.Month - 1) / 3) + 1;
        }

        /// <summary>
        /// Returns true if the date is within the following range, false otherwise:
        /// <para>For SMALLDATETIME</para>
        /// <para>Minimum Date is : 1 Jan 1900</para>
        /// <para>Maximum Date is : 6 Jun 2079</para>
        /// </summary>
        /// <param name="theDate"></param>
        /// <returns></returns>
        public static Boolean isSafeForSQL_SmallDateTime(this DateTime theDate)
        {
            DateTime minDate = new DateTime(1900, 1, 1);
            DateTime maxDate = new DateTime(2079, 6, 6);

            if (theDate < minDate || theDate > maxDate)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true if the date is within the following range, false otherwise:
        /// <para>For DATETIME</para>
        /// <para>Minimum Date is : i Jan 1753</para>
        /// <para>Maximum Date is : 31 Dec 9999</para>
        /// </summary>
        /// <param name="theDate"></param>
        /// <returns></returns>
        public static Boolean isSafeForSQL_DateTime(this DateTime theDate)
        {
            DateTime minDate = new DateTime(1753, 1, 1);
            DateTime maxDate = new DateTime(9999, 12, 31);

            if (theDate <= minDate || theDate >= maxDate)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

}
