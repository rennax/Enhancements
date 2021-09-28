using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enhancements
{
    public class Messenger
    {
        private static Messenger _instance;
        private Dictionary<Type, Delegate> simpleActions = new Dictionary<Type, Delegate>();
        private Dictionary<Type, Delegate> actions = new Dictionary<Type, Delegate>();

        public static Messenger Default
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Messenger();
                }
                return _instance;
            }
        }

        public void Send<T>(T message)
        {
            Delegate del;
            if (actions.TryGetValue(typeof(T), out del) == true)
            {
                del.DynamicInvoke(message);
            }
        }
        
        public void Send<T>()
        {
            Delegate del;
            if (simpleActions.TryGetValue(typeof(T), out del) == true)
            {
                del.DynamicInvoke();
            }
        }

        public void Register<T>(Action<T> action)
        {
            Delegate del;
            if (actions.TryGetValue(typeof(T), out del) == true)
            {
                actions.Add(typeof(T), action);
            }
            else
            {
                actions[typeof(T)] = Delegate.Combine(del, action);
            }

        }

        public void Register<T>(Action action)
        {
            Delegate del;
            if (simpleActions.TryGetValue(typeof(T), out del) == true)
            {
                simpleActions.Add(typeof(T), action);
            }
            else
            {
                simpleActions[typeof(T)] = Delegate.Combine(del, action);
            }
        }

        public void UnRegister<T>(Action<T> action)
        {
            Delegate del;
            if (actions.TryGetValue(typeof(T), out del) == true)
            {
                actions[typeof(T)] = Delegate.Remove(del, action);
            }
        }

        public void UnRegister<T>(Action action)
        {
            Delegate del;
            if (simpleActions.TryGetValue(typeof(T), out del) == true)
            {
                simpleActions[typeof(T)] = Delegate.Remove(del, action);
            }
        }
    }
}
