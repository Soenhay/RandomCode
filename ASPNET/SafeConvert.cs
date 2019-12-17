using System;

namespace Common
{
    //ATTENTION! each of these functions does a try/catch and returns a default value if the try fails. 
    //When parsing large amounts of data that consistently fails to be converted, the catch will slow down your program.
    //To avoid the 100s or 1000s of catches slowing down the program make this check before calling SafeConvert: String.IsNullOrEmpty(value.toString()) and do not call SafeConvert if it is true.
    //
    //TODO: think about modifying SafeConvert to do this check automatically.

    /// <summary>
    /// A set of functions that will try to convert the passed in obj to the appropriate 
    /// <para>type and return a default value upon failure.</para>
    /// <para>Includes overloads for each method where the default value can be specified.</para>
    /// </summary>
    /// <remarks>Be careful when modifying this class since it might be used in multiple locations.</remarks>
    public static class SafeConvert
    {
        /// <summary>
        /// Try to conver to an 8 bit unsigned integer (byte). If it fails 0 is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Byte SafeByte(Object obj)
        {
            return SafeByte(obj, 0);
        }

        /// <summary>
        /// Try to conver to an 8 bit unsigned integer (byte). If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Byte SafeByte(Object obj, Byte DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            try { return Convert.ToByte(obj); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to a Boolean. If it fails DefaultValue is returned. 
        /// The default value should be explicitly defined in all cases.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Boolean SafeBoolean(Object obj, Boolean DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            switch (obj.ToString().ToUpper())
            {
                case "T":
                case "Y":
                case "YES":
                    return true;
                case "F":
                case "N":
                case "NO":
                    return false;
            }
            try { return Convert.ToBoolean(obj); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to a single(also known as a float). If it fails 0.0f is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Single SafeSingle(Object obj)
        {
            return SafeSingle(obj, 0.0f);
        }

        /// <summary>
        /// Tries to convert the object to a single(also known as a float). If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Single SafeSingle(Object obj, Single DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            try { return Convert.ToSingle(obj, System.Globalization.CultureInfo.InvariantCulture); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to a double. If it fails 0.0 is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Double SafeDouble(Object obj)
        {
            return SafeDouble(obj, 0.0);
        }

        /// <summary>
        /// Tries to convert the object to a double. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Double SafeDouble(Object obj, Double DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            try { return Convert.ToDouble(obj, System.Globalization.CultureInfo.InvariantCulture); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to a Decimal. If it fails 0.0m is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Decimal SafeDecimal(Object obj)
        {
            return SafeDecimal(obj, 0.0m);
        }

        /// <summary>
        /// Tries to convert the object to a Decimal. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Decimal SafeDecimal(Object obj, Decimal DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            try { return Convert.ToDecimal(obj); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to an Int16 in the range 0-255 which is equivalent to an sql tiny int. 
        /// <para>If it fails 0 is returned.</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Int16 SafeTinyInt16(Object obj)
        {
            return SafeByte(obj);
        }

        /// <summary>
        /// Tries to convert the object to an Int16 in the range 0-255 which is equivalent to an sql tiny int.
        /// <para>If it fails DefaultValue is returned.</para>
        /// <para>If DefaultValue is not 0-255 then DefaultValue is set to 0.</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Int16 SafeTinyInt16(Object obj, Byte DefaultValue)
        {
            return SafeByte(obj, DefaultValue);
        }

        /// <summary>
        /// First tries to convert num to an Int16. Then to TinyInt16. If num16 is too large for an TinyInt16 then it takes the right most digits that will put it in range.
        /// If it failes 0 is returned.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static Int16 SafeTinyInt16_Right(Object obj, String logDescription)
        {
            //Notes
            //Int16.MaxValue = 32767
            //Int16.MinValue = -32768

            Int16 num16 = SafeConvert.SafeInt16(obj);
            Int16 num8 = SafeConvert.SafeTinyInt16(obj);

            if (obj != null && (!((Byte.MinValue <= num16) && (num16 <= Byte.MaxValue)) || num16 == 0))
            {
                string strNum = obj.ToString().Right(3);//Take right most 3 digits
                num8 = SafeConvert.SafeTinyInt16(strNum);
                if (!((Int16.MinValue <= num8) && (num8 <= Int16.MaxValue)) || num8 == 0)
                {
                    strNum = obj.ToString().Right(2);//If still out of range then take right most 2 digits ( guaranteed to be in range.
                    num8 = SafeConvert.SafeTinyInt16(strNum);
                }
                //This ends up making too many log entries.
                //string strMsg = MethodBase.GetCurrentMethod().ReflectedType.Name + "." + MethodInfo.GetCurrentMethod().Name + " - [" + logDescription + "] " + obj.ToString() + " converted to " + num32 + ".";
                //LogHelper.AddLog("", "", strMsg);
            }
            return num8;//num8 should now be within range for TinyInt16.
        }

        /// <summary>
        /// Tries to convert the object to an Int16. If it fails 0 is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Int16 SafeInt16(Object obj)
        {
            return SafeInt16(obj, 0);
        }

        /// <summary>
        /// Tries to convert the object to an Int16. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Int16 SafeInt16(Object obj, Int16 DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            //Convert to double first for rare case of invalid input format. ex "1.000"
            try { return Convert.ToInt16(SafeDouble(obj, DefaultValue)); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// First tries to convert num to an Int32. Then to an Int16. If num32 is too large for an Int16 then it takes the right most digits that will put it in range.
        /// If it failes 0 is returned.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static Int16 SafeInt16_Right(Object obj, String logDescription)
        {
            //Notes
            //Int16.MaxValue = 32767
            //Int16.MinValue = -32768

            Int32 num32 = SafeConvert.SafeInt32(obj);
            Int16 num16 = SafeConvert.SafeInt16(obj);

            if (obj != null && (!((Int16.MinValue <= num32) && (num32 <= Int16.MaxValue)) || num32 == 0))
            {
                string strNum = obj.ToString().Right(5);//Take right most 5 digits
                num16 = SafeConvert.SafeInt16(strNum);
                if (!((Int16.MinValue <= num16) && (num16 <= Int16.MaxValue)) || num16 == 0)
                {
                    strNum = obj.ToString().Right(4);//If still out of range then take right most 4 digits ( guaranteed to be in range.
                    num16 = SafeConvert.SafeInt16(strNum);
                }
                //This ends up making too many log entries.
                //string strMsg = MethodBase.GetCurrentMethod().ReflectedType.Name + "." + MethodInfo.GetCurrentMethod().Name + " - [" + logDescription + "] " + obj.ToString() + " converted to " + num32 + ".";
                //LogHelper.AddLog("", "", strMsg);
            }
            return num16;//num32 should now be within range for Int16.
        }

        /// <summary>
        /// Tries to convert the object to an UInt16. If it fails 0 is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static UInt16 SafeUInt16(Object obj)
        {
            return SafeUInt16(obj, 0);
        }

        /// <summary>
        /// Tries to convert the object to an UInt16. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static UInt16 SafeUInt16(Object obj, UInt16 DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            try { return Convert.ToUInt16(obj); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to an Int32. If it fails 0 is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Int32 SafeInt32(Object obj)
        {
            return SafeInt32(obj, 0);
        }

        /// <summary>
        /// Tries to convert the object to an Int32. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Int32 SafeInt32(Object obj, Int32 DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            //Convert to double first for rare case of invalid input format. ex "1.000"
            try { return Convert.ToInt32(SafeDouble(obj, DefaultValue)); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// First tries to convert num to an Int64. Then to an Int32. If num64 is too large for an Int32 then it takes the right most digits that will put it in range.
        /// If it failes 0 is returned.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static Int32 SafeInt32_Right(Object obj, String logDescription)
        {
            //Notes:
            //Int32.MaxValue = 2,147,483,647
            //Int32.MinValue = -2,147,483,648

            Int64 num64 = SafeConvert.SafeInt64(obj);
            Int32 num32 = SafeConvert.SafeInt32(obj);

            if (!((Int32.MinValue <= num64) && (num64 <= Int32.MaxValue)) || num64 == 0)
            {
                string strNum = obj.ToString().Right(10);
                num32 = SafeConvert.SafeInt32(strNum);
                if (!((Int32.MinValue <= num32) && (num32 <= Int32.MaxValue)) || num32 == 0)
                {
                    strNum = obj.ToString().Right(9);//If still out of range then take right most 9 digits ( guaranteed to be in range ).
                    num32 = SafeConvert.SafeInt32(strNum);
                }
                //This ends up making too many log entries.
                //string strMsg = MethodBase.GetCurrentMethod().ReflectedType.Name + "." + MethodInfo.GetCurrentMethod().Name + " - [" + logDescription + "] " + obj.ToString() + " converted to " + num32 + ".";
                //LogHelper.AddLog("", "", strMsg);
            }

            return num32;//num32 should now be within range for Int32.
        }

        /// <summary>
        /// Tries to convert the object to an UInt32. If it fails 0 is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static UInt32 SafeUInt32(Object obj)
        {
            return SafeUInt32(obj, 0);
        }

        /// <summary>
        /// Tries to convert the object to an UInt32. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static UInt32 SafeUInt32(Object obj, UInt32 DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            try { return Convert.ToUInt32(obj); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to an Int64. If it fails 0 is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Int64 SafeInt64(Object obj)
        {
            return SafeInt64(obj, 0);
        }

        /// <summary>
        /// Tries to convert the object to an Int64. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Int64 SafeInt64(Object obj, Int64 DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            try { return Convert.ToInt64(obj); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to an UInt64. If it fails 0 is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static UInt64 SafeUInt64(Object obj)
        {
            return SafeUInt64(obj, 0);
        }

        /// <summary>
        /// Tries to convert the object to an UInt64. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static UInt64 SafeUInt64(Object obj, UInt64 DefaultValue)
        {
            if (obj == null || obj == DBNull.Value || String.IsNullOrEmpty(obj.ToString())) { return DefaultValue; }
            try { return Convert.ToUInt64(obj); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to a string. If it fails "" is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static String SafeString(Object obj)
        {
            return SafeString(obj, String.Empty);
        }

        /// <summary>
        /// Tries to convert the object to a string. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static String SafeString(Object obj, String DefaultValue)
        {
            if (obj == null) { return DefaultValue; }
            try { return Convert.ToString(obj); }
            catch { return DefaultValue; }
        }

        /// <summary>
        /// Tries to convert the object to a character. If it fails '\0' (null character) is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Char SafeChar(Object obj)
        {
            return SafeChar(obj, '\0');
        }

        /// <summary>
        /// Tries to convert the object to a character. If it fails DefaultValue is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static Char SafeChar(Object obj, Char DefaultValue)
        {
            if (obj == null || obj == DBNull.Value) { return DefaultValue; }
            try { return Convert.ToChar(obj); }
            catch { return DefaultValue; }
        }

    }
}
