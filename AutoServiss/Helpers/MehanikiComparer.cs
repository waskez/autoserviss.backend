using System.Collections.Generic;
using AutoServiss.Database;

namespace AutoServiss.Helpers
{
    public class MehanikiComparer : IEqualityComparer<Mehanikis>
    {
        public bool Equals(Mehanikis m1, Mehanikis m2)
        {
            return m1.Id == m2.Id;
        }

        public int GetHashCode(Mehanikis m)
        {
            return m.Id;
        }
    }
}
