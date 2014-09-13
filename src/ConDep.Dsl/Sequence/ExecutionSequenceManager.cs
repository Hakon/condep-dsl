using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations.LoadBalancer;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public class ExecutionSequenceManager : IValidate
    {
        private readonly IEnumerable<ServerConfig> _servers;
        private readonly ILoadBalance _loadBalancer;
        internal readonly List<LocalSequence> _localSequences = new List<LocalSequence>();
        private readonly List<RemoteSequence> _remoteSequences = new List<RemoteSequence>();
        private LoadBalancerExecutorBase _internalLoadBalancer;

        public ExecutionSequenceManager(IEnumerable<ServerConfig> servers, ILoadBalance loadBalancer)
        {
            _servers = servers;
            _loadBalancer = loadBalancer;
            _internalLoadBalancer = GetLoadBalancer();
        }

        public LocalSequence NewLocalSequence(string name)
        {
            var sequence = new LocalSequence(name, _loadBalancer);
            _localSequences.Add(sequence);
            return sequence;
        }

        public RemoteSequence NewRemoteSequence(string name)
        {
            var sequence = new RemoteSequence();
            _remoteSequences.Add(sequence);
            return sequence;
        }

        public void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            foreach (var localSequence in _localSequences)
            {
                token.ThrowIfCancellationRequested();

                LocalSequence sequence = localSequence;
                Logger.WithLogSection(localSequence.Name, () => sequence.Execute(status, settings, token));
            }

            var serversToDeployTo = _internalLoadBalancer.GetServerExecutionOrder(status, settings, token);
            var errorDuringLoadBalancing = false;

            foreach (var server in serversToDeployTo)
            {
                var server1 = server;
                try
                {
                    _internalLoadBalancer.BringOffline(server1, status, settings, token);
                    if (!server1.PreventDeployment)
                    {
                        foreach (var remoteSequence in _remoteSequences)
                        {
                            token.ThrowIfCancellationRequested();

                            var sequence = remoteSequence;
                            Logger.WithLogSection(remoteSequence.Name, () => sequence.Execute(server1, status, settings, token));
                        }
                    }
                }
                catch
                {
                    errorDuringLoadBalancing = true;
                    throw;
                }
                finally
                {
                    if (!errorDuringLoadBalancing && !settings.Options.StopAfterMarkedServer)
                    {
                        _internalLoadBalancer.BringOnline(server1, status, settings, token);
                    }
                }
            }
        }

        public bool IsValid(Notification notification)
        {
            return _localSequences.All(x => x.IsValid(notification));
        }

        public void DryRun()
        {
            foreach (var item in _localSequences)
            {
                Logger.WithLogSection(item.Name, () => { item.DryRun(); });
            }
        }

        private LoadBalancerExecutorBase GetLoadBalancer()
        {
            //if (_paralell)
            //{
            //    return new ParalellRemoteExecutor(_servers);
            //}

            switch (_loadBalancer.Mode)
            {
                case LbMode.Sticky:
                    return new StickyLoadBalancerExecutor(_loadBalancer);
                case LbMode.RoundRobin:
                    return new RoundRobinLoadBalancerExecutor(_servers, _loadBalancer);
                default:
                    throw new ConDepLoadBalancerException(string.Format("Load Balancer mode [{0}] not supported.",
                                                                    _loadBalancer.Mode));
            }
            return null;
        }
    }
}