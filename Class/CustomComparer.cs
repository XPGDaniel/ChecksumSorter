using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ChecksumSorter.Class
{
    public class CustomComparer : IComparer<string>
    {

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern int StrCmpLogicalW(String x, String y);

        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }

    }
}
