namespace United.Utility.Config
{
    public class TimeoutPolicyConfig
    {
        /// <summary>
        /// Timeout seconds
        /// </summary>
        public int Seconds { get; set; } = 3;//30;
    }

    public class RetryPolicyConfig
    {
        /// <summary>
        /// The retry times if request is failed
        /// </summary>
        public int RetryCount { get; set; } = 1;//3;
    }

    public class CircuitBreakerPolicyConfig
    {
        /// <summary>
        /// The retry times before the CircuitBreaker opens
        /// </summary>
        public int AllowExceptions { get; set; } = 3;

        /// <summary>
        /// The sleep duration for the opening circuit breaker, based on seconds
        /// </summary>
        public int BreakDuration { get; set; } = 6;//60;
    }
}
