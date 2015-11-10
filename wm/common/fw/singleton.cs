/** fw - singleton
 * Author: Zex <top_zlynch AT yahoo.com>
 * 
 */
using System;

namespace fw
{
    public class SingletonBase
    {
        private static SingletonBase _instance;
        private static readonly object _mutex = new object();

        protected SingletonBase()
        {
        }

        public static SingletonBase Instance
        {
            get
            {
                lock (_mutex)
                {
                    if (null == _instance)
                    {
                        _instance = new SingletonBase();
                    }
                    return _instance;
                }
            }
        }
    }

    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object _mutex = new object();

        protected Singleton()
        {
        }

        public static T Instance
        {
            get
            {
                lock (_mutex)
                {
                    if (null == _instance)
                    {
                        _instance = new T();
                    }
                    return _instance;
                }
            }
        }
    }
}
