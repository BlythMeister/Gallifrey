using System.Collections.Generic;

namespace Gallifrey.Comparers
{
    public class JiraReferenceComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var xParts = x.Split('-');
            var yParts = y.Split('-');

            return xParts[0] == yParts[0]
                       ? int.Parse(xParts[1]).CompareTo(int.Parse(yParts[1]))
                       : x.CompareTo(y);
        }
    }
}