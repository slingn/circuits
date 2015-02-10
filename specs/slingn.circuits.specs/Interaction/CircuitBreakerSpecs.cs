using System;
using Machine.Fakes;
using Machine.Specifications;

namespace slingn.circuits.specs.Interaction.CircuitBreakerSpecs
{
    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_is_added_into_a_circuit_breaker_and_then_the_circuit_is_requested : WithFakes
    {
        private static int _breaklimit = 1;
        private static string _circuitName = "test_circuit";

        private static Circuit _result;

        Because of = () => CircuitBreaker.Add(_circuitName, _breaklimit);

        It should_retrieve_the_circuit = () => _result = CircuitBreaker.Get(_circuitName);

        Cleanup after = () => CircuitBreaker.Reset();
        
    }

    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_breaker_is_asked_to_execute_and_the_specified_circuit_name_has_never_been_added_or_executed_before_and_then_the_circuit_is_requested : WithFakes
    {
        private static Action _sourceMethod;
        private static int _breaklimit = 1;
        private static string _circuitName = "test_circuit";

        private static Circuit _result;

        Establish context = () =>
        {
            _sourceMethod = An<Action>();
        };

        Because of = () => CircuitBreaker.Execute(_circuitName, _sourceMethod, _breaklimit);

        It should_be_able_to_retrieve_the_circuit = () => CircuitBreaker.Get(_circuitName).ShouldNotBeNull();

        Cleanup after = () => CircuitBreaker.Reset();

    }

    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_breaker_is_asked_to_execute_without_configuration_parameters_and_the_specified_circuit_name_does_not_have_any_configuration_information : WithFakes
    {
        private static Action _sourceMethod;
        private static int _breaklimit = 1;
        private static string _circuitName = "test_circuit";

        private static Circuit _result;

        Establish context = () =>
        {
            _sourceMethod = An<Action>();
        };

        Because of = () =>
        {
            _result = CircuitBreaker.Execute(_circuitName, _sourceMethod);
        };

        It should_have_the_default_break_limit = () => _result.BreakLimit.ShouldEqual(CircuitBreaker.DefaultBreakLimit);
        It should_have_the_default_break_duration = () => _result.BreakDuration.ShouldEqual(CircuitBreaker.DefaultBreakDuration);

        Cleanup after = () => CircuitBreaker.Reset();

    }

    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_breaker_executes : WithFakes
    {
        private static Action _sourceMethod;
        private static int _breaklimit = 1;
        private static string _circuitName = "test_circuit";

        Establish context = () =>
        {
            _sourceMethod = An<Action>();
        };

        Because of = () => CircuitBreaker.Execute(_circuitName, _sourceMethod, _breaklimit);

        It should_execute_its_circuits_method = () => _sourceMethod.WasToldTo(method => method());

        Cleanup after = () => CircuitBreaker.Reset();
    }

    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_breaker_executes_and_is_told_not_to_raise_any_exceptions_and_an_exception_occurs : WithFakes
    {
        private static Action _sourceMethod;
        private static ApplicationException _sourceMethodException = new ApplicationException("this is an exception");
        private static string _circuitName = "test_circuit";

        private static Exception _result;

        private static Circuit _circuit;

        private static int _breaklimit = 1;

        Establish context = () =>
        {
            _sourceMethod = An<Action>();
            _sourceMethod.WhenToldTo(method => method()).Throw(_sourceMethodException);
        };

        Because of = () =>
        {
            try
            {
                _circuit = CircuitBreaker.Execute(_circuitName, _sourceMethod, _breaklimit);
            }
            catch (Exception ex)
            {
                _result = ex;
            }

        };

        It should_not_raise_an_exception = () => _result.ShouldBeNull();

        It should_have_1_failure = () => _circuit.Failures.ShouldEqual(1);

        Cleanup after = () => CircuitBreaker.Reset();
        
    }
    
    [Subject(typeof(CircuitBreaker))]
    public class When_a_circuit_breaker_has_executed_a_circuit : WithFakes
    {
        static Action _sourceMethod;
        static Circuit _result;
        private static string _circuitName = "test_circuit";

        Establish context = () =>
        {
            _sourceMethod = () => An<Action>();

            CircuitBreaker.Execute(_circuitName, _sourceMethod, 1, TimeSpan.FromSeconds(1), false);

        };

        Because of = () =>
        {
            _result = CircuitBreaker.Get(_circuitName);
        };

        It should_return_the_circuit = () => _result.ShouldNotBeNull();

        Cleanup after = () => CircuitBreaker.Reset();
    }
}