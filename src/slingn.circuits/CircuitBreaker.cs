using System;
using System.Collections.Generic;

namespace slingn.circuits
{
    public class CircuitBreaker
    {

        private static readonly Dictionary<string, Circuit> Circuits = new Dictionary<string, Circuit>();
        private static readonly object Locker = new object();

        
        public static Circuit Execute(string name, Action action, int breakLimit, TimeSpan breakDuration, bool throwOnError)
        {

            lock (Locker)
            {
                //get the existing circuit or create a new one
                Circuit circuit = Get(name) ?? Create(name, breakLimit, breakDuration);

                //check the circuit
                circuit.Check();

                //if the circuit is broken - return it
                if (circuit.IsBroken())
                    return circuit;


                //execute the circuit
                try
                {
                    circuit.Execute(action);
                    return circuit;
                }
                catch (Exception)
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                    return circuit;
                }
            }

        }

        public static Circuit Execute(string name, Action action, int breakLimit, TimeSpan breakDuration)
        {
            return Execute(name, action, breakLimit, breakDuration, false);
        }

        public static Circuit Execute(string name, Action action, int breakLimit)
        {
            return Execute(name, action, breakLimit, TimeSpan.FromSeconds(30), false);

        }

        private static Circuit Create(string name, int breakLimit, TimeSpan breakDuration)
        {
                
            lock (Locker)
            {
                var circuit = new Circuit(name, breakLimit, breakDuration);
                Circuits[name] = circuit;
                return circuit;
            }

        }

        public static Circuit Get(string name)
        {
            lock (Locker)
            {
                return Circuits.ContainsKey(name) ? Circuits[name] : null;
            }

        }


        public static void Reset()
        {
            lock (Locker)
            {
                Circuits.Clear();
            }
        }


    }
}