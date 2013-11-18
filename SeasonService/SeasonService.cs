using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace SeasonService
{
    /// <summary>
    /// http://stellafane.org/misc/equinox.html
    /// http://en.wikipedia.org/wiki/Julian_day
    /// http://en.wikipedia.org/wiki/Ephemeris_Time
    /// http://www.epochconverter.com/
    /// </summary>
    public class SeasonService
    {
        /// <summary>
        /// Calculate and display a single event for a single year (Either a Equiniox or Solstice).
        /// Meeus Astronmical Algorithms Chapter 27
        /// </summary>
        /// <param name="year"></param>
        /// <param name="season"></param>
        /// <returns></returns>
        public static DateTime Calculate(int year, Season season)
        {
            if (year < 1000 || year > 3000)
            {
                throw new ArgumentOutOfRangeException("Year must be between 1000 and 3000");
            }

            var jde0 = CalculateInitial(year, season);  // Initial estimate of date of event
            var t = (jde0 - 2451545.0) / 36525;
            var w = 35999.373 * t - 2.47;
            var dl = 1 + 0.0334 * Math.Cos(w * Math.PI / 180) + 0.0007 * Math.Cos(2 * w * Math.PI / 180);
            var s = Periodic24(t);
            var jde = jde0 + 0.00001 * s / dl;          // This is the answer in Julian Emphemeris Days

            var tdt = FromJdToTdt(jde);                 // Convert Julian Days to TDT in a Date Object
            var utc = FromTdtToUtc(tdt);                // Correct TDT to UTC, both as Date Objects
            return utc;
        }

        /// <summary>
        /// Calcualte an initial guess as the JD of the Equinox or Solstice of a Given Year
        /// Meeus Astronmical Algorithms Chapter 27
        /// </summary>
        /// <param name="year"></param>
        /// <param name="season"></param>
        /// <returns></returns>
        protected static double CalculateInitial(int year, Season season)
        {
            // http://stackoverflow.com/questions/6318252/c-sharp-double-decimal-problems
            double y = (year - 2000.0) / 1000.0;

            switch (season)
            {
                case Season.Spring:
                    return 2451623.80984 + 365242.37404 * y + 0.05169 * Math.Pow(y, 2) - 0.00411 * Math.Pow(y, 3) - 0.00057 * Math.Pow(y, 4);
                case Season.Summer:
                    return 2451716.56767 + 365241.62603 * y + 0.00325 * Math.Pow(y, 2) + 0.00888 * Math.Pow(y, 3) - 0.00030 * Math.Pow(y, 4);
                case Season.Autumn:
                    return 2451810.21715 + 365242.01767 * y - 0.11575 * Math.Pow(y, 2) + 0.00337 * Math.Pow(y, 3) + 0.00078 * Math.Pow(y, 4);
                case Season.Winter:
                    return 2451900.05952 + 365242.74049 * y - 0.06223 * Math.Pow(y, 2) - 0.00823 * Math.Pow(y, 3) + 0.00032 * Math.Pow(y, 4);
                default:
                    throw new ArgumentOutOfRangeException("Season");
            }
        }

        /// <summary>
        /// Calculate 24 Periodic Terms
        /// Meeus Astronmical Algorithms Chapter 27
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected static double Periodic24(double t)
        {
            double[] a = { 485, 203, 199, 182, 156, 136, 77, 74, 70, 58, 52, 50, 45, 44, 29, 18, 17, 16, 14, 12, 12, 12, 9, 8 };
            double[] b = { 324.96, 337.23, 342.08, 27.85, 73.14, 171.52, 222.54, 296.72, 243.58, 119.81, 297.17, 21.02, 247.54, 325.15, 60.93, 155.12, 288.79, 198.04, 199.76, 95.39, 287.11, 320.81, 227.73, 15.45 };
            double[] c = { 1934.136, 32964.467, 20.186, 445267.112, 45036.886, 22518.443, 65928.934, 3034.906, 9037.513, 33718.147, 150.678, 2281.226, 29929.562, 31555.956, 4443.417, 67555.328, 4562.452, 62894.029, 31436.921, 14577.848, 31931.756, 34777.259, 1222.114, 16859.074 };

            double s = 0;
            for (var i = 0; i < 24; i++)
            {
                s += a[i] * Math.Cos((b[i] + c[i] * t) * Math.PI / 180);
            }
            return s;
        }

        /// <summary>
        /// Julian Date (JD) to UTC Date Object
        /// Meeus Astronmical Algorithms Chapter 7
        /// Possible with fractional days
        /// </summary>
        /// <param name="jd"></param>
        /// <returns></returns>
        protected static DateTime FromJdToTdt(double jd)
        {
            double a;
            double alpha;
            double z = Math.Floor(jd + 0.5);                // Integer JD's
            double f = jd + 0.5 - z;                        // Fractional JD's
            if (z < 2299161)
            {
                a = z;
            }
            else
            {
                alpha = Math.Floor((z - 1867216.25) / 36524.25);
                a = z + 1 + alpha - Math.Floor(alpha / 4);
            }
            var b = a + 1524;
            var c = Math.Floor((b - 122.1) / 365.25);
            var d = Math.Floor(365.25 * c);
            var e = Math.Floor((b - d) / 30.6001);
            var dt = b - d - Math.Floor(30.6001 * e) + f;   // Day of month with decimals for time
            var mon = e - (e < 13.5 ? 1 : 13);              // Month number
            var yr = c - (mon > 2.5 ? 4716 : 4715);         // Year
            var day = Math.Floor(dt);                       // Day of month without decimals for time
            var h = 24 * (dt - day);                        // Hours and fractional hours
            var hr = Math.Floor(h);                         // Integer hours
            var m = 60 * (h - hr);                          // Minutes and fractional minutes
            var min = Math.Floor(m);                        // Integer minutes
            var sec = Math.Floor(60 * (m - min));           // Integer seconds (milliseconds discarded)

            // Create and set a date Object and return it
            return new DateTime((int)yr, (int)mon, (int)day, (int)hr, (int)min, (int)sec);
        }

        /// <summary>
        /// Correct TDT to UTC
        /// from Meeus Astronmical Algroithms Chapter 10
        /// Correction lookup table has entry for every even year between TBLfirst and TBLlast
        /// </summary>
        /// <param name="tdt"></param>
        /// <returns></returns>
        protected static DateTime FromTdtToUtc(DateTime tdt)
        {
            var tblFirst = 1620;
            var tblLast = 2002;
            double[] tbl = { /*1620*/ 121,112,103, 95, 88,  82, 77, 72, 68, 63,  60, 56, 53, 51, 48,  46, 44, 42, 40, 38,
                             /*1660*/  35, 33, 31, 29, 26,  24, 22, 20, 18, 16,  14, 12, 11, 10,  9,   8,  7,  7,  7,  7,
                             /*1700*/   7,  7,  8,  8,  9,   9,  9,  9,  9, 10,  10, 10, 10, 10, 10,  10, 10, 11, 11, 11,
                             /*1740*/  11, 11, 12, 12, 12,  12, 13, 13, 13, 14,  14, 14, 14, 15, 15,  15, 15, 15, 16, 16,
                             /*1780*/  16, 16, 16, 16, 16,  16, 15, 15, 14, 13,  
                             /*1800*/ 13.1, 12.5, 12.2, 12.0, 12.0,  12.0, 12.0, 12.0, 12.0, 11.9,  11.6, 11.0, 10.2,  9.2,  8.2,
                             /*1830*/  7.1,  6.2,  5.6,  5.4,  5.3,   5.4,  5.6,  5.9,  6.2,  6.5,   6.8,  7.1,  7.3,  7.5,  7.6,
                             /*1860*/  7.7,  7.3,  6.2,  5.2,  2.7,   1.4, -1.2, -2.8, -3.8, -4.8,  -5.5, -5.3, -5.6, -5.7, -5.9,
                             /*1890*/ -6.0, -6.3, -6.5, -6.2, -4.7,  -2.8, -0.1,  2.6,  5.3,  7.7,  10.4, 13.3, 16.0, 18.2, 20.2,
                             /*1920*/ 21.1, 22.4, 23.5, 23.8, 24.3,  24.0, 23.9, 23.9, 23.7, 24.0,  24.3, 25.3, 26.2, 27.3, 28.2,
                             /*1950*/ 29.1, 30.0, 30.7, 31.4, 32.2,  33.1, 34.0, 35.0, 36.5, 38.3,  40.2, 42.2, 44.5, 46.5, 48.5,
                             /*1980*/ 50.5, 52.5, 53.8, 54.9, 55.8,  56.9, 58.3, 60.0, 61.6, 63.0,  63.8, 64.3 }; /*2002 last entry*/
            // Values for deltaT for 2000 thru 2002 from NASA
            double deltaT = 0;                  // deltaT = TDT - UTC (in seconds)
            var year = tdt.Year;
            var t = (year - 2000.0) / 100.0;    // Centuries from the epoch 2000.0

            if (year >= tblFirst && year <= tblLast)
            {
                // Find correction in table
                if (year % 2 != 0)
                {
                    // Odd year - interpolate
                    deltaT = (tbl[(year - tblFirst - 1) / 2] + tbl[(year - tblFirst + 1) / 2]) / 2;
                }
                else
                {
                    // Even year - direct table lookup
                    deltaT = tbl[(year - tblFirst) / 2];
                }
            }
            else if (year < 948)
            {
                deltaT = 2177 + 497 * t + 44.1 * Math.Pow(t, 2);
            }
            else if (year >= 948)
            {
                deltaT = 102 + 102 * t + 25.3 * Math.Pow(t, 2);
                if (year >= 2000 && year <= 2100)
                {
                    // Special correction to avoid discontinurity in 2000
                    deltaT += 0.37 * (year - 2100);
                }
            }
            else
            {
                throw new Exception("Error: TDT to UTC correction not computed");
            }

            var epoch = new DateTime(1970, 1, 1);
            var time = (tdt - epoch).TotalMilliseconds - deltaT * 1000;
            return epoch + TimeSpan.FromMilliseconds(time);
        }
    }
}