using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Holiday
{
    public class DateModel
    {
        /// <summary>
        /// 年份
        /// </summary>
        public int Year { set; get; }
        /// <summary>
        /// 调休的工作日
        /// </summary>
        public List<string> Work { set; get; }
        /// <summary>
        /// 假期
        /// </summary>
        public List<string> Holiday { set; get; }
    }
}
