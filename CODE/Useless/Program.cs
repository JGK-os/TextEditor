using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Code.Core
{
    public class Kernel
    {
        public static string pwd = "";
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            External.Program.TextEditor.TextEditor.Start( "" );
        }
    }
}