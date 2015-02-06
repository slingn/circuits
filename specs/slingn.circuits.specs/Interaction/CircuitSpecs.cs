using System;
using Machine.Fakes;
using Machine.Specifications;
using slingn.circuits.Exceptions;

namespace slingn.circuits.specs.Interaction.CircuitSpecs
{
    [Subject(typeof(Circuit))]
    public class When_a_circuit_executes : WithFakes
    {
        private static Action _sourceMethod;

        private static ApplicationException _sourceMethodException = new ApplicationException("this is an exception");

        private static Circuit _circuit;
        private static string _circuitName = "test_circuit";

        private static int _breakLimit = 1;
        private static TimeSpan _breakDuration = TimeSpan.FromMilliseconds(500);

        Establish context = () =>
        {
            _sourceMethod = An<Action>();

            _circuit = new Circuit(_circuitName, _breakLimit, _breakDuration);
        };

        Because of = () =>
        {
            _circuit.Execute(_sourceMethod);
        };

        private It should_execute_the_specified_action = () => _sourceMethod.WasToldTo(method => method());
    }

    [Subject(typeof(Circuit))]
    public class When_a_circuit_is_executed_and_an_exception_occurs : WithFakes
    {
        private static Action _sourceMethod;

        private static ApplicationException _sourceMethodException = new ApplicationException("this is an exception");

        private static Circuit _circuit;
        private static string _circuitName = "test_circuit";

        private static int _breakLimit = 1;
        private static TimeSpan _breakDuration = TimeSpan.FromMilliseconds(500);

        Establish context = () =>
        {
            _sourceMethod = An<Action>();
            _sourceMethod.WhenToldTo(method => method()).Throw(_sourceMethodException);

            _circuit = new Circuit(_circuitName, _breakLimit, _breakDuration);
        };

        Because of = () =>
        {
            try
            {
                _circuit.Execute(_sourceMethod);
            }
            catch (Exception ex)
            {
                _result = ex;
            }

        };

        It should_throw_a_circuit_execution_exception = () =>
        {
            _result.ShouldBeAssignableTo<CircuitExecutionException>();
            _result.InnerException.ShouldEqual(_sourceMethodException);
        };

        It should_increment_the_number_of_failures = () =>
        {
            _circuit.Failures.ShouldEqual(1);
        };

        private static Exception _result;
    }
}