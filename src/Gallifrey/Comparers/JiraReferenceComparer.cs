using System;
using System.Collections.Generic;

namespace Gallifrey.Comparers
{
    public class JiraReferenceComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var xParts = x?.Split('-');
            var yParts = y?.Split('-');

            int.TryParse(xParts?[1], out var xNumber);
            int.TryParse(yParts?[1], out var yNumber);

            return xParts?[0] == yParts?[0] ? xNumber.CompareTo(yNumber) : string.Compare(x, y, StringComparison.InvariantCulture);
        }
    }
}
