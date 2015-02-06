using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;

namespace slingn.circuits.demo.Controllers
{
    public class ExampleApiController : ApiController
    {
        private DataRepository _repository = new DataRepository();

        public JsonResult Get(bool? throwException)
        {
            var list = new List<string>();

            //i provide my circuit with a unique name
            const string circuitName = "MyCircuit";
            
            //i will allow 2 exceptions before the Circuit Breaks (Off)
            const int breakLimit = 2;
            
            //when a Circuit Breaks (Off), i will not allow the Circuit to Execute (application logic) for 30 seconds
            TimeSpan breakDuration = TimeSpan.FromSeconds(10);

            //next: I define the application logic which I want to protect with a Circuit 
            
            //in my case, i want to allow the caller of my web service to simulate an exception being thrown during execution of
            //my application logic
            Action applicationLogic = () =>
            {
                if (throwException.HasValue && throwException.Value)
                {
                    throw new Exception();
                }
                list = _repository.List();
            };
            try
            {
                //next: i pass the parameters i've defined into the Execute method of the Circuit breaker
                //the Circuit Breaker will always return an instance of the Circuit you have implictly created/retrieved
                var circuit = CircuitBreaker.Execute(circuitName, applicationLogic, breakLimit, breakDuration, true);
                
                //for illustrative purposes, if the circuit is broken, i am displaying a message to the screen
                //indicating the Circuit's Break Expiration Date/Time
                
                //NOTE: this is only for illustrative purposes, you do not have to use the Circuit instance returned by this 
                //method and will probably never do so unless you are looking to handle Circuit specific activity 
                //(as I do in this example)
                if (circuit.IsBroken())
                {
                    list.Add(
                       string.Format("Circuit is broken and will not be retried until {0:MM/dd/yyyy HH:mm:ss.fff}", circuit.ExpirationDate)
                    );
                }
            }
            catch(Exception ex)
            {
                list.Add(ex.Message);                
            }


            return new JsonResult()
            {
                Data = list,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }


    internal class DataRepository
    {
        public List<string> List()
        {
            return new List<string>
            {
                string.Format("Retrieved Data from server at - {0}", DateTime.Now),
            };
        }
    }
}