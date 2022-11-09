using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM_View
{
    internal class countTime
    {
        public int Count { get; set; }
        public countTime()
        {
            Count = 0;
            run();
        }
        public void reset()
        {
            Count = 0;
        }
        public void run()
        {
            Thread st = new Thread(() =>
            {
                runTime();
            });
            st.Start();
        }
        void runTime()
        {
            while (true)
            {
                Count++;
                Thread.Sleep(1000);
            }
        }
    }
}
