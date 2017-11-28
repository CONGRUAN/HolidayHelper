using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Holiday
{
    public class HolidayHelper
    {
        private static object _syncObj = new object();
        private static HolidayHelper _instance { get; set; }
        private static List<DateModel> cacheDateList { get; set; }
        private HolidayHelper() { }

        /// <summary>
        /// 获得单例对象,使用懒汉式（双重锁定）
        /// </summary>
        /// <returns></returns>
        public static HolidayHelper GetInstance()
        {
            if (_instance == null)
            {
                lock (_syncObj)
                {
                    if (_instance == null)
                    {
                        _instance = new HolidayHelper();
                    }
                }
            }
            return _instance;
        }

        private List<DateModel> GetConfigList()
        {
            //该字符串从数据库中读取
            string jsonStr = "[{\"Year\": \"2015\",\"Work\": [ \"0104\", \"0215\", \"0228\", \"0906\", \"1010\" ],\"Holiday\": [ \"0101\", \"0102\", \"0103\", \"0218\",\"0219\", \"0220\", \"0221\", \"0222\", \"0223\", \"0224\", \"0404\", \"0405\", \"0406\", \"0501\", \"0502\", \"0503\", \"0620\", \"0621\", \"0622\", \"090\", \"0904\", \"0905\", \"0927\", \"1001\", \"1002\", \"1003\", \"1004\", \"1005\", \"1006\", \"1007\" ]},{\"Year\": \"2016\",\"Work\": [ \"0206\", \"0214\", \"0612\", \"0918\", \"1008\", \"1009\" ],\"Holiday\": [ \"0101\", \"0207\", \"0208\", \"0209\", \"0210\", \"0211\", \"0212\", \"0213\", \"0404\", \"0501\",\"0502\", \"0609\", \"0610\", \"0611\", \"0915\", \"0916\", \"0917\", \"1001\", \"1002\", \"1003\", \"1004\", \"1005\", \"1006\", \"1007\" ]},{\"Year\": \"2017\",\"Work\": [ \"0122\", \"0204\", \"0401\", \"0527\", \"0930\" ],\"Holiday\": [ \"0101\", \"0102\", \"0127\", \"0128\", \"0129\", \"0130\", \"0201\", \"0202\", \"0501\", \"0529\", \"0530\", \"1001\", \"1002\", \"1003\", \"1004\", \"1005\", \"1006\" ]}]";
            cacheDateList = JsonConvert.DeserializeObject<List<DateModel>>(jsonStr);
            return cacheDateList;
        }

        /// <summary>
        /// 获取指定年份的数据
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        private DateModel GetConfigDataByYear(int year)
        {
            if (cacheDateList == null)//取配置数据
                GetConfigList();
            DateModel result = cacheDateList.FirstOrDefault(m => m.Year == year);
            return result;
        }

        /// <summary>
        /// 判断是否为工作日
        /// </summary>
        /// <param name="currDate">要判断的时间</param>
        /// <param name="thisYearData">当前的数据</param>
        /// <returns></returns>
        private bool IsWorkDay(DateTime currDate, DateModel thisYearData)
        {
            if (currDate.Year != thisYearData.Year)//跨年重新读取数据
            {
                thisYearData = GetConfigDataByYear(currDate.Year);
            }
            if (thisYearData.Year > 0)
            {
                string date = currDate.ToString("MMdd");
                int week = (int)currDate.DayOfWeek;

                if (thisYearData.Work.IndexOf(date) >= 0)
                {
                    return true;
                }

                if (thisYearData.Holiday.IndexOf(date) >= 0)
                {
                    return false;
                }

                if (week != 0 && week != 6)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据传入的工作日天数，获得计算后的日期,可传负数
        /// </summary>
        /// <param name="day">天数</param>
        /// <param name="isContainToday">当天是否算工作日（默认：true）</param>
        /// <returns></returns>
        public DateTime GetReckonDate(int day, bool isContainToday = true)
        {
            DateTime currDate = DateTime.Now;
            int addDay = day >= 0 ? 1 : -1;

            if (isContainToday)
                currDate = currDate.AddDays(-addDay);

            DateModel thisYearData = GetConfigDataByYear(currDate.Year);
            if (thisYearData.Year > 0)
            {
                int sumDay = Math.Abs(day);
                int workDayNum = 0;
                while (workDayNum < sumDay)
                {
                    currDate = currDate.AddDays(addDay);
                    if (IsWorkDay(currDate, thisYearData))
                        workDayNum++;
                }
            }
            return currDate;
        }

        /// <summary>
        /// 根据传入的时间，计算工作日天数
        /// </summary>
        /// <param name="date">带计算的时间</param>
        /// <param name="isContainToday">当天是否算工作日（默认：true）</param>
        /// <returns></returns>
        public int GetWorkDayNum(DateTime date, bool isContainToday = true)
        {
            var currDate = DateTime.Now;

            int workDayNum = 0;
            int addDay = date.Date > currDate.Date ? 1 : -1;

            if (isContainToday)
            {
                currDate = currDate.AddDays(-addDay);
            }

            DateModel thisYearData = GetConfigDataByYear(currDate.Year);
            if (thisYearData.Year > 0)
            {
                bool isEnd = false;
                do
                {
                    currDate = currDate.AddDays(addDay);
                    if (IsWorkDay(currDate, thisYearData))
                        workDayNum += addDay;
                    isEnd = addDay > 0 ? (date.Date > currDate.Date) : (date.Date < currDate.Date);
                } while (isEnd);
            }
            return workDayNum;
        }
    }
}
