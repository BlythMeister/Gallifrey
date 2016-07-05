using System;

namespace Gallifrey.UI.Modern.Helpers
{
    public static class HourMinuteHelper
    {
        public static void UpdateHours(ref int hourValue, int newValue, int maxHours)
        {
            if (hourValue == newValue)
            {
                return;
            }

            if (newValue < 0)
            {
                hourValue = 0;
            }
            else if (newValue > maxHours)
            {
                hourValue = 9;
            }
            else
            {
                hourValue = newValue;
            }
        }

        public static void UpdateMinutes(ref int hourValue, ref int minuteValue, int newValue, int maxHours, out bool hoursChanged)
        {
            hoursChanged = false;

           if (newValue < 0)
            {
                if (hourValue == 0)
                {
                    hourValue = 0;
                }
                else
                {
                    {
                        minuteValue = 60 + newValue;
                        hourValue--;
                    }
                    hoursChanged = true;
                }
            }
            else if (newValue >= 100)
            {
                minuteValue = 59;
            }
            else if (newValue >= 60)
            {
                if (hourValue == maxHours)
                {
                    minuteValue = 59;
                }
                else
                {
                    hourValue++;
                    minuteValue = newValue - 60;
                }
                hoursChanged = true;
            }
            else
            {
                minuteValue = newValue;
            }
        }
    }
}