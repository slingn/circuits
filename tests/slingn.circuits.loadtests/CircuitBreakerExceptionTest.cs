using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.WebTesting;


namespace slingn.circuits.loadtests
{
    public class CircuitBreakerExceptionTest : WebTest
    {

        public override IEnumerator<WebTestRequest> GetRequestEnumerator()
        {
            var randomized = new Random();
            var randomNumber = randomized.Next(1000);
            var shouldThrowException = randomNumber == 1;

            var request = new WebTestRequest("http://localhost/slingn.circuits.demo/api/ExampleApi");
            request.Headers.Add(new WebTestRequestHeader("X-Requested-With", "XMLHttpRequest"));
            request.QueryStringParameters.Add("action", "Get");
            request.QueryStringParameters.Add("throwException", shouldThrowException.ToString());

            request.ExtractValues += (sender, args) =>
            {
                bool isBroken = args.Response.BodyString.Contains("broken");
                args.Success = !isBroken;
            };


            yield return request;
            request = null;
        }
    }
}
