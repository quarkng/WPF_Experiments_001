using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A001
{
    class BindExample
    {
        public int BindInt { get; set; }
        public string BindStr { get; set; }
        public double BindDouble { get; set; }
        public DateTime BindDatetime { get; set; }


        public static BindExample GetExample()
        {
            BindExample bindExample = new BindExample();

            bindExample.BindDatetime = DateTime.Now;
            bindExample.BindDouble = Math.PI;
            bindExample.BindInt = 42;
            bindExample.BindStr = "Hello Bind Example World!";

            return bindExample;
        }

    }
}
