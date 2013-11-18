using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeasonService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var start = SeasonService.Calculate(2013, Season.Autumn);
            var end = GetSeasonEndDate(2013, Season.Autumn);

            Console.WriteLine("Fall 2013: " + start.ToLocalTime() + " - " + end.ToLocalTime());
            Console.ReadLine();
        }

        private static DateTime GetSeasonEndDate(int year, Season season)
        {
            DateTime end;
            switch (season)
            {
                case Season.Spring:
                    end = SeasonService.Calculate(year, Season.Summer);
                    break;
                case Season.Summer:
                    end = SeasonService.Calculate(year, Season.Autumn);
                    break;
                case Season.Autumn:
                    end = SeasonService.Calculate(year, Season.Winter);
                    break;
                case Season.Winter:
                    end = SeasonService.Calculate(year + 1, Season.Spring);
                    break;
                default:
                    throw new ArgumentException("Season");
            }

            // Set end date to 1 millisecond before next season's start date.
            return end - new TimeSpan(0, 0, 0, 0, 1);
        }
    }
}
