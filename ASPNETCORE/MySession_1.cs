using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace MyWeb.lib
{
    //Reference https://www.c-sharpcorner.com/article/a-better-approach-to-access-httpcontext-outside-a-controller-in-net-core-2-1/

    public class MySession
    {
        private static ISession _session
        {
            get
            {
                return _accessor.HttpContext.Session;
            }
        }

        private static IHttpContextAccessor _accessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _accessor = httpContextAccessor;
        }

        // Gets the current session.
        public static MySession Current
        {
            get
            {
                MySession session = _session.GetObject<MySession>("__MySession__");
                if (session == null)
                {
                    session = new MySession();
                    _session.SetObject("__MySession__", session);
                }
                return session;
            }
        }

        public void Clear()
        {
            _session.Clear();
        }

        public UserModel User { get; set; }
    }

    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static void SetBoolean(this ISession session, string key, bool value)
        {
            session.Set(key, BitConverter.GetBytes(value));
        }

        public static bool? GetBoolean(this ISession session, string key)
        {
            var data = session.Get(key);
            if (data == null)
            {
                return null;
            }
            return BitConverter.ToBoolean(data, 0);
        }

        public static void SetDouble(this ISession session, string key, double value)
        {
            session.Set(key, BitConverter.GetBytes(value));
        }

        public static double? GetDouble(this ISession session, string key)
        {
            var data = session.Get(key);
            if (data == null)
            {
                return null;
            }
            return BitConverter.ToDouble(data, 0);
        }
    }
}
