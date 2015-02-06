using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.WebTesting;


namespace slingn.circuits.loadtests
{
    public class CircuitBreakerTest : WebTest
    {

        public override IEnumerator<WebTestRequest> GetRequestEnumerator()
        {
            var request = new WebTestRequest("http://localhost/slingn.circuits.demo/api/ExampleApi");
            request.Headers.Add(new WebTestRequestHeader("X-Requested-With", "XMLHttpRequest"));
            request.QueryStringParameters.Add("action", "Get");
            request.QueryStringParameters.Add("throwException", "false");
            yield return request;
            request = null;
        }
    }
}
