using System;
using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    internal class RemoteCompositeBuilder : IOfferRemoteComposition
    {
        private readonly CompositeSequence _compositeSequence;

        public RemoteCompositeBuilder(CompositeSequence compositeSequence)
        {
            _compositeSequence = compositeSequence;
            Deploy = new RemoteDeploymentBuilder(CompositeSequence);
            Execute = new RemoteExecutionBuilder(CompositeSequence);
            Configure = new RemoteConfigurationBuilder(CompositeSequence);
            Install = new RemoteInstallationBuilder(CompositeSequence);
        }

        public IOfferRemoteDeployment Deploy { get; private set; }
        public IOfferRemoteExecution Execute { get; private set; }
        public IOfferRemoteConfiguration Configure { get; private set; }
        public IOfferRemoteInstallation Install { get; private set; }

        internal CompositeSequence CompositeSequence
        {
            get { return _compositeSequence; }
        }

        public IOfferRemoteComposition OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteCompositeBuilder(CompositeSequence.NewConditionalCompositeSequence(condition));
        }
    }
}