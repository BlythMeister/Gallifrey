using System;

namespace Gallifrey.JiraTimers
{
    public static class TimeSpanExtensionMethods
    {
        public static string FormatAsString(this TimeSpan value)
        {
            var txt = FormatAsStringWithoutSeconds(value);
            txt += ":";

            if (value.Seconds > 0)
            {
                if (value.Seconds.ToString().Length == 1)
                {
                    txt += "0";
                }
                txt += value.Seconds.ToString();
            }
            else
            {
                txt += "00";
            }

            return txt;
        }

        public static string FormatAsStringWithoutSeconds(this TimeSpan value)
        {
            var txt = "";

            var hours = 0;

            if (value.Days > 0)
            {
                hours = value.Days * 24;
            }

            if (value.Hours > 0)
            {
                hours = hours + value.Hours;
            }

            if (hours > 0)
            {
                if (hours.ToString().Length == 1)
                {
                    txt += "0";
                }
                txt += hours.ToString();
                value = value.Subtract(new TimeSpan(0, hours, 0, 0));
            }
            else
            {
                txt += "00";
            }

            txt += ":";

            if (value.Minutes > 0)
            {
                if (value.Minutes.ToString().Length == 1)
                {
                    txt += "0";
                }
                txt += value.Minutes.ToString();
                value = value.Subtract(new TimeSpan(0, 0, value.Minutes, 0));
            }
            else
            {
                txt += "00";
            }

            return txt;
        }
    }
}
