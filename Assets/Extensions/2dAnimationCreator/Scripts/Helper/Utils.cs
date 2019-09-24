using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Line
{
    public static class Utils
    {
        #region General
        public static int ToInt(this object value)
        {
            int result = 0;
            if (value == null)
                return result;
            int.TryParse(value.ToString(), out result);
            return result;
        }

        public static float ToFloat(this object value)
        {
            float result = 0;
            if (value == null)
                return result;
            float.TryParse(value.ToString(), out result);
            return result;
        }

        public static long ToLong(this object value)
        {
            long result = 0;
            if (value == null)
                return result;
            long.TryParse(value.ToString(), out result);
            return result;
        }

        public static bool ToBool(this object value)
        {
            bool result = false;
            if (value == null)
                return result;
            bool.TryParse(value.ToString(), out result);
            return result;
        }

        public static bool ToBool(this int value)
        {
            return value == 1;
        }


        public static bool Swap<T>(this List<T> list, int index1, int index2)
        {
            if (index1 >= list.Count || index2 >= list.Count)
                return false;
            var temp = list[index1];
            list[index2] = list[index1];
            list[index1] = temp;
            return true;
        }

        public static string GetFullTimeFormat(this float time)
        {
            string result = "";
            if (time <= 0)
                return "00:00:00";
            var hour = time / 3600;
            var minute = time / 60;
            var second = time % 60;
            result = hour.GetDigit() + ":" + minute.GetDigit() + ":" + second.GetDigit();
            return result;
        }

        public static string GetMinuteTimeFormat(this float time)
        {
            string result = "";
            if (time <= 0)
                return "00:00";
            var minute = time / 60;
            var second = time % 60;
            result = minute.GetDigit() + ":" + second.GetDigit();
            return result;
        }

        public static string GetHourTimeFormat(this float time)
        {
            string result = "";
            if (time <= 0)
                return "00:00";
            var hour = time / 3600;
            var minute = time / 60;
            result = hour.GetDigit() + ":" + minute.GetDigit();
            return result;
        }

        public static string GetDigit(this int value)
        {
            if (value < 10)
                return "0" + value;
            return value.ToString();
        }

        public static string GetDigit(this float value)
        {
            if (value < 10)
                return "0" + (int)value;
            return ((int)value).ToString();
        }



        public static string ToArrayString(this List<object> list)
        {
            string result = "[";
            for (int i = 0; i < list.Count; i++)
            {
                result += list[i];
                if (i < list.Count - 1)
                    result += ",";
            }
            result += "]";
            return result;
        }
        #endregion
    }
}

