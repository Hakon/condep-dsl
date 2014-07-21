using System.Collections.Generic;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;

namespace ConDep.Dsl.SemanticModel.Sequence
{
    public abstract class LoadBalancerExecutorBase : IExecute
    {
        private readonly List<IExecuteOnServer> _sequence;

        protected LoadBalancerExecutorBase(List<IExecuteOnServer> sequence)
        {
            _sequence = sequence;
        }

        public abstract void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token);
        public string Name { get; private set; }

        protected void ExecuteOnServer(ServerConfig server, IReportStatus status, ConDepSettings settings, ILoadBalance loadBalancer, bool bringServerOfflineBeforeExecution, bool bringServerOnlineAfterExecution, CancellationToken token)
        {
            var errorDuringLoadBalancing = false;

            Logger.WithLogSection(server.Name, () =>
                {
                    try
                    {
                        if (bringServerOfflineBeforeExecution)
                        {
                            Logger.Info(string.Format("Taking server [{0}] offline in load balancer.", server.Name));
                            loadBalancer.BringOffline(server.Name, server.LoadBalancerFarm,
                                                      LoadBalancerSuspendMethod.Suspend, status);
                        }

                        ExecuteOnServer(server, status, settings, token);
                    }
                    catch
                    {
                        errorDuringLoadBalancing = true;
                        throw;
                    }
                    finally
                    {
                        //&& !status.HasErrors
                        if (bringServerOnlineAfterExecution && !errorDuringLoadBalancing)
                        {
                            Logger.Info(string.Format("Taking server [{0}] online in load balancer.", server.Name));
                            loadBalancer.BringOnline(server.Name, server.LoadBalancerFarm, status);
                        }
                    }
                });

        }

        protected virtual void ExecuteOnServer(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            Logger.WithLogSection("Deployment", () =>
                {
                    foreach (var element in _sequence)
                    {
                        token.ThrowIfCancellationRequested();

                        IExecuteOnServer elementToExecute = element;
                        if (element is CompositeSequence)
                            elementToExecute.Execute(server, status, settings, token);
                        else
                            Logger.WithLogSection(element.Name, () => elementToExecute.Execute(server, status, settings, token));
                    }
                });
        }


        public void DryRun()
        {
            foreach (var item in _sequence)
            {
                Logger.WithLogSection(item.Name, () => { item.DryRun(); });
            }
        }
    }
}