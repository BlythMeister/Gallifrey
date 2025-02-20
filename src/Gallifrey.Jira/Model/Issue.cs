using System;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Issue : IEquatable<Issue>
    {
        public long id { get; set; }
        public string key { get; set; }
        public Fields fields { get; set; }

        public bool Equals(Issue other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return id == other.id && key == other.key;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Issue)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (id.GetHashCode() * 397) ^ (key != null ? key.GetHashCode() : 0);
            }
        }
    }
}
