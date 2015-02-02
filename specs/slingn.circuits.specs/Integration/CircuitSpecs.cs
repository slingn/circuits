using System;
using System.Threading;
using Machine.Fakes;
using Machine.Specifications;

namespace slingn.circuits.specs.Integration.CircuitSpecs
{
    
    [Subject(typeof(Circuit))]
    public class When_a_circuit_executes_and_an_exception_occurs_and_its_break_duration_has_elapsed_and_it_is_checked : WithFakes
    {
        private static Action _sourceMethod;

        private static ApplicationException _sourceMethodException = new ApplicationException("this is an exception");
        
        private static Circuit _circuit;
        private static string _circuitName = "test_circuit";

        private static int _breaklimit = 1;
        private static TimeSpan _breakDuration = TimeSpan.FromMilliseconds(500);

        Establish context = () =>
        {
            _sourceMethod = An<Action>();
            _sourceMethod.WhenToldTo(method => method()).Throw(_sourceMethodException);

            _circuit = new Circuit(_circuitName, _breaklimit, _breakDuration);
        };

        Because of = () =>
        {
            try 
            { 
                _circuit.Execute(_sourceMethod);
            }
            catch { }

            //wait for the circuit break duration to expire 
            Thread.Sleep(_breakDuration.Add(TimeSpan.FromSeconds(1)));
            _circuit.Check();

        };

        It should_reset_the_failure_count = () => _circuit.Failures.ShouldEqual(0);

    }


}