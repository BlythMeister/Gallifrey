using System.Collections.Generic;

namespace Gallifrey.Comparers
{
    public class JiraReferenceComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var xParts = x.Split('-');
            var yParts = y.Split('-');

            int xNumber, yNumber;

            int.TryParse(xParts[1], out xNumber);
            int.TryParse(yParts[1], out yNumber);

            return xParts[0] == yParts[0]
                       ? xNumber.CompareTo(yNumber)
                       : x.CompareTo(y);
        }
    }
}