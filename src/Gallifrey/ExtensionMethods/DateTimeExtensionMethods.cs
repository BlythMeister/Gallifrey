using System;

namespace Gallifrey.ExtensionMethods
{
    public static class DateTimeExtensionMethods
    {
        public static bool Between(this DateTime input, DateTime date1, DateTime date2)
        {
            return (input > date1 && input < date2);
        }
    }
}
