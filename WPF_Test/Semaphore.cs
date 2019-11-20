using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Test
{
    static class Semaphore
    {
        public static int iterator = 0;
        public static int[,] table = new int[5, 3]
        {
            { 1, 2, 3 },
            { 4, 2, 3 },
            { 1, 4, 3 },
            { 1, 2, 4 },
            { 1, 2, 3 }
        };  //where 1 - Read 1 file, 2 - Read 2 file, 3 - Read 3 file, 4 - Write in *i* file
        
    }
}
