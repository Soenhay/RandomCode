using System;
using System.Collections.Generic;
using System.Web;
using System.Data;

namespace SoenhaysLibrary
{
    /// <summary>
    /// Put all session variables in here.
    /// 
    ///<para>-it saves you from a lot of type-casting</para>
    ///<para>-you don't have to use hard-coded session keys throughout your application (e.g. Session["loginId"]</para>
    ///<para>-you can document your session items by adding XML doc comments on the properties of MySession</para>
    ///<para>-you can initialize your session variables with default values (e.g. assuring they are not null)</para>
    /// </summary>
    /// <remarks> http://stackoverflow.com/questions/621549/how-to-access-session-variables-from-any-class-in-asp-net </remarks>
    public class MySession
    {
        // private constructor
        private MySession()
        {
            //Put default property values here.
            BannerStyle = "";
            User = new UserInfo();
        }

        /// <summary>
        /// Gets the current session. Also creates MySession if it does not exist.
        /// </summary>
        public static MySession Current
        {
            get
            {
                MySession session = (MySession)HttpContext.Current.Session["__MySession__"];
                if (session == null)
                {
                    session = new MySession();
                    HttpContext.Current.Session["__MySession__"] = session;
                }
                return session;
            }
        }

        public static void ClearSession()
        {
            HttpContext.Current.Session.Clear();
        }

        /// <summary>
        /// Clear user info.
        /// </summary>
        public void ClearUser()
        {
            User = new UserInfo();
        }

        // **** add your session properties here

        /// <summary>
        /// Contains information about a user.
        /// </summary>
        public UserInfo User { get; set; }
    }
}
