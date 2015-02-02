using System;
using System.Threading;
using System.Threading.Tasks;
using Machine.Fakes;
using Machine.Specifications;

namespace slingn.circuits.specs.Integration.CircuitBreakerSpecs
{
    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_breaker_has_met_its_break_limit_and_is_executed : WithFakes
    {
        private static Circuit _circuit;
        private static int _breaklimit = 1;
        private static string _circuitName = "test_circuit";

        Establish context = () =>
        {
            
            _sourceMethod = An<Action>();
            _sourceMethodException = new ApplicationException("this is an exception");
            _sourceMethod.WhenToldTo(method => method()).Throw(_sourceMethodException);
        };

        private Because of = () =>
        {
            CircuitBreaker.Execute(_circuitName, _sourceMethod, _breaklimit);
            CircuitBreaker.Execute(_circuitName, _sourceMethod, _breaklimit);
            
        };

        It should_not_allow_the_circuit_to_execute = () =>
        {
            _sourceMethod.WasToldTo(x => x()).OnlyOnce();
        };

        private static Action _sourceMethod;
        private static ApplicationException _sourceMethodException;
        private static Exception _result;

        Cleanup after = () => CircuitBreaker.Reset();
    }

    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_has_met_its_break_limit_and_its_break_duration_is_expired_and_is_executed : WithFakes
    {
        private static Action _sourceMethod;
        private static ApplicationException _sourceMethodException;
        private static Exception _result;
        private static string _circuitName = "test_circuit";
     
        private static int _breakLimit = 1;
        private static TimeSpan _breakDuration;

        Establish context = () =>
        {
            _sourceMethod = An<Action>();
            _sourceMethodException = new ApplicationException("this is an exception");
            _sourceMethod.WhenToldTo(method => method()).Throw(_sourceMethodException);

            _breakDuration = TimeSpan.FromSeconds(1);

            CircuitBreaker.Execute(_circuitName, _sourceMethod, _breakLimit, _breakDuration);

            CircuitBreaker.Execute(_circuitName, _sourceMethod, _breakLimit, _breakDuration);

            Thread.Sleep(2000);
        };

        private Because of = () =>
        {

            CircuitBreaker.Execute(_circuitName, _sourceMethod, _breakLimit, _breakDuration);
        };

        It should_execute = () =>
        {
            _sourceMethod.WasToldTo(method => method()).Times(2);
        };

        Cleanup after = () => CircuitBreaker.Reset();

    }

    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_is_executed_on_multiple_threads : WithFakes
    {
        private static Action _sourceMethod;
        private static ApplicationException _sourceMethodException;
        private static Exception _result;
        private static string _circuitName = "test_circuit";

        private static Circuit _circuit;
        private static int _breakLimit = 0;
        private static TimeSpan _breakDuration;
        private static Circuit circuit1;
        private static Circuit circuit2;


        Establish context = () =>
        {
            _sourceMethod = An<Action>();
            _breakDuration = TimeSpan.Zero;
        };

        private Because of = () =>
        {
            Action t1 = () =>
            {
                circuit1 = CircuitBreaker.Execute(_circuitName, _sourceMethod, _breakLimit, _breakDuration);
            };

            Action t2 = () =>
            {
                circuit2 = CircuitBreaker.Execute(_circuitName, _sourceMethod, _breakLimit, _breakDuration);
            };

            Parallel.Invoke(t1, t2);
        };

        It should_only_have_one_instance_of_the_circuit_registered = () =>
        {
            circuit1.ShouldEqual(circuit2);
        };

        
        Cleanup after = () => CircuitBreaker.Reset();
    }

    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_is_executed_on_multiple_threads_and_exceptions_occur : WithFakes
    {
        private static Action _sourceMethod;
        private static ApplicationException _sourceMethodException;
        private static Exception _result;
        private static string _circuitName = "test_circuit";

        private static Circuit _circuit;
        private static int _breakLimit = 0;
        private static TimeSpan _breakDuration;
        private static Circuit circuit1;
        private static Circuit circuit2;


        Establish context = () =>
        {
            CircuitBreaker.Reset();
            _sourceMethod = An<Action>();
            _sourceMethodException = new ApplicationException("this is an exception");
            _sourceMethod.WhenToldTo(method => method()).Throw(_sourceMethodException);

            _breakDuration = TimeSpan.Zero;
        };

        private Because of = () =>
        {
            Action t1 = () =>
            {
                circuit1 = CircuitBreaker.Execute(_circuitName, _sourceMethod, _breakLimit, _breakDuration);
            };

            Action t2 = () =>
            {
                circuit2 = CircuitBreaker.Execute(_circuitName, _sourceMethod, _breakLimit, _breakDuration);
            };

            Parallel.Invoke(t1, t2);
        };

        It should_have_an_accurate_number_of_failures = () =>
        {
            circuit1.ShouldEqual(circuit2);
            circuit1.Failures.ShouldEqual(2);
        };

        Cleanup after = () => CircuitBreaker.Reset();
    }

}