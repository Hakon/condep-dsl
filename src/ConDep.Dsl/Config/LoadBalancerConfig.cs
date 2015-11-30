using System;

namespace ConDep.Dsl.Config
{
    [Serializable]
    public class LoadBalancerConfig
    {
        private int _timeoutInSeconds = 60;

        public string Name { get; set; }
        public string Provider { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SuspendMode { get; set; }
        public string Mode { get; set; }

        public int TimeoutInSeconds
        {
            get { return _timeoutInSeconds; }
            set { _timeoutInSeconds = value; }
        }

        public LbMode GetModeAsEnum()
        {
            switch (Mode.ToLower())
            {
                case "sticky":
                    return LbMode.Sticky;
                case "roundrobin":
                    return LbMode.RoundRobin;
                default:
                    throw new NotSupportedException(string.Format("Load Balancer Mode [{0}] is not supported.", Mode));
            }
    }
        public LoadBalancerSuspendMethod GetSuspendModeAsEnum()
        { 
            switch (SuspendMode.ToLower())
            {
                case "graceful":
                    return LoadBalancerSuspendMethod.Graceful;
                case "suspendclearconnections":
                    return LoadBalancerSuspendMethod.SuspendClearConnections;
                case "suspend":
                    return LoadBalancerSuspendMethod.Suspend;
                default:
                    throw new NotSupportedException(string.Format("Load Balancer Suspend Mode [{0}] is not supported.", SuspendMode));
            }
        }

        public dynamic CustomConfig { get; set; }
    }
}