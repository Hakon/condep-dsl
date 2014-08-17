using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class RemoteCompositeBuilder : IOfferRemoteComposition
    {
        private readonly CompositeSequence _compositeSequence;

        public RemoteCompositeBuilder(CompositeSequence compositeSequence)
        {
            _compositeSequence = compositeSequence;
            Deploy = new RemoteDeploymentBuilder(compositeSequence);
            ExecuteRemote = new RemoteExecutionBuilder(compositeSequence);
            Require = new InfrastructureBuilder(compositeSequence);
            Install = new RemoteInstallationBuilder(compositeSequence);
        }

        public IOfferRemoteDeployment Deploy { get; private set; }
        public IOfferRemoteExecution ExecuteRemote { get; private set; }
        public IOfferInfrastructure Require { get; private set; }
        public IOfferRemoteInstallation Install { get; private set; }

        public IOfferRemoteComposition OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteCompositeBuilder(_compositeSequence.NewConditionalCompositeSequence(condition));
        }
    }
}