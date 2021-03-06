﻿using System;
using slingn.circuits.Exceptions;

namespace slingn.circuits
{
    /// <summary>
    /// Circuit keeps track of errors encountered while executing a portion of applications logic and allows for the 
    /// logic to be automatically temporarily (or permanently) disabled
    /// </summary>
    public class Circuit
    {
        private readonly string _name;
     
        /// <summary>
        /// number of errors to allow before breaking - Note: 0 means there is no limit (Circuit will never break)
        /// </summary>
        private readonly int _breakLimit;

        /// <summary>
        /// Number of failures recorded since the last time Circuit was Broken (Failures were reset)
        /// </summary>
        private int _failures;

        /// <summary>
        /// amount of time to break after encountering an error - Note: TimeSpan.Zero means a Broken Circuit (Off) will never be Connected (On)
        /// </summary>
        private readonly TimeSpan _breakDuration;
        
        /// <summary>
        /// timestamp of the last error
        /// </summary>
        private DateTime? _lastExceptionTimestamp;

        private int _numberOfExecutionAttempts;

        /// <summary>
        /// Creates a Circuit instance
        /// </summary>
        /// <param name="name">The Circuits' Name</param>
        /// <param name="breakLimit">Number of times the Circuits' Action/Function is 
        /// allowed to throw an Exception before Breaking the Circuit (Off)</param>
        /// <param name="breakDuration">Time Span to wait after the Circuit is Broken (Off) before 
        /// allowing the Circuit to be Connected (On)</param>
        public Circuit(string name, int breakLimit, TimeSpan breakDuration)
        {
            _name = name;
            _breakLimit = breakLimit;
            _breakDuration = breakDuration; 
        }

        /// <summary>
        /// Creates a Circuit instance (Default Break Duration is 5 Minutes)
        /// </summary>
        /// <param name="name">The Circuits' Name</param>
        /// <param name="breakLimit">Number of times the Circuits' Action/Function is 
        /// allowed to throw an Exception before Breaking the Circuit (Off)</param>
        public Circuit(string name, int breakLimit)
            : this(name, breakLimit, TimeSpan.FromMinutes(5)) { }



        /// <summary>
        /// Number of failures recorded since the last time Circuit was Broken (Off)
        /// </summary>
        public int Failures
        {
            get { return _failures; }
        }

        /// <summary>
        /// The Circuit's Name
        /// </summary>
        public string Name
        {
            get { return _name; }
        }


        /// <summary>
        /// Expiration Date/Time - used the Last Recorded Exception Date/Time plus the Break Duration
        /// </summary>
        public DateTime ExpirationDate
        {
            get
            {
                if (_lastExceptionTimestamp.HasValue == false) 
                    return DateTime.MinValue;

                return _lastExceptionTimestamp.Value.Add(_breakDuration);
            }
        }


        /// <summary>
        /// Checks whether the Circuit is Broken - Note: A Circuit can never be broken if its Break Limit is set to 0 or less
        /// </summary>
        /// <returns>true if the Circuit is Disconnected (Off), false if it is Connected (On)</returns>
        public bool IsBroken()
        {
            var canBeBroken = _breakLimit > 0;
            var hasMoreFailuresThanBreakLimitAllows = _failures >= _breakLimit;

            var isBroken = canBeBroken && hasMoreFailuresThanBreakLimitAllows;

            return isBroken;
        }


        /// <summary>
        /// Executes tracks and traps exceptions generated by the specified Action
        /// </summary>
        /// <param name="action">The Circuit's Action/Function</param>
        public void Execute(Action action)
        {
            try
            {
                _numberOfExecutionAttempts++;
                action();
            }
            catch (Exception ex)
            {
                _failures++;
                _lastExceptionTimestamp = DateTime.Now;
                string message = string.Format("A Circuit Execution exception has occurred while executing the Circuit {0}.", _name);
                throw new CircuitExecutionException(message, ex);
            }
        }

        public int NumberOfExecutionAttempts
        {
            get { return _numberOfExecutionAttempts; }
        }

        public TimeSpan BreakDuration
        {
            get { return _breakDuration; }
        }

        public int BreakLimit
        {
            get { return _breakLimit; }
        }

        private bool ShouldResetFailureCount()
        {
            if(!_lastExceptionTimestamp.HasValue || _breakDuration == TimeSpan.Zero) return false;

            var isBreakDurationExpired = DateTime.Now > ExpirationDate;
            return isBreakDurationExpired;
        }

        /// <summary>
        /// Check's whether the Circuit should be Connected (On)
        /// </summary>
        public void Check()
        {
            if (!ShouldResetFailureCount()) return;
            _failures = 0;
        }

    }
}