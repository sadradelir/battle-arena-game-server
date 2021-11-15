using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MobarezooServer.Utilities
{
    /// <summary>
    /// A game timer class.
    /// </summary>
    public class GameTimer
    {
        private readonly double _secondsPerCount;
        private double _deltaTime;
        private long _baseTime;
        private long _pausedTime;
        private long _stopTime;
        private long _prevTime;
        private long _currTime;
        private bool _stopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTimer"/> class.
        /// </summary>
        public GameTimer()
        {
            _secondsPerCount = 0.0;
            _deltaTime = -1.0;
            _baseTime = 0;
            _pausedTime = 0;
            _prevTime = 0;
            _currTime = 0;
            _stopped = false;
            var countsPerSec = Stopwatch.Frequency;
            _secondsPerCount = 1.0 / countsPerSec;
        }

        /// <summary>
        /// Gets the total time the timer has been running.
        /// </summary>
        /// <value>
        /// The total time the timer has been running.
        /// </value>
        public float TotalTime
        {
            get
            {
                if (_stopped)
                {
                    return (float) (((_stopTime - _pausedTime) - _baseTime) * _secondsPerCount);
                }

                return (float) (((_currTime - _pausedTime) - _baseTime) * _secondsPerCount);
            }
        }

        /// <summary>
        /// Gets the delta time, which is the time passed since the last frame or tick.
        /// </summary>
        /// <value>
        /// The delta time.
        /// </value>
        public float DeltaTime => (float) _deltaTime;

        /// <summary>
        /// Resets the game timer.
        /// </summary>
        public void Reset()
        {
            var curTime = Stopwatch.GetTimestamp();
            _baseTime = curTime;
            _prevTime = curTime;
            _stopTime = 0;
            _stopped = false;
        }

        /// <summary>
        /// Starts the game timer.
        /// </summary>
        [SuppressMessage("ReSharper", "InvertIf")]
        public void Start()
        {
            var startTime = Stopwatch.GetTimestamp();
            if (_stopped)
            {
                _pausedTime += (startTime - _stopTime);
                _prevTime = startTime;
                _stopTime = 0;
                _stopped = false;
            }
        }

        /// <summary>
        /// Stops the game timer.
        /// </summary>
        [SuppressMessage("ReSharper", "InvertIf")]
        public void Stop()
        {
            if (!_stopped)
            {
                var curTime = Stopwatch.GetTimestamp();
                _stopTime = curTime;
                _stopped = true;
            }
        }

        /// <summary>
        /// Tick the game timer.
        /// </summary>
        public void Tick()
        {
            if (_stopped)
            {
                _deltaTime = 0.0;
                return;
            }

            var curTime = Stopwatch.GetTimestamp();
            _currTime = curTime;
            _deltaTime = (_currTime - _prevTime) * _secondsPerCount;
            _prevTime = _currTime;
            if (_deltaTime < 0.0)
            {
                _deltaTime = 0.0;
            }
        }
    }
}