﻿using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public class RemoteDeploymentBuilder : RemoteBuilder, IOfferRemoteDeployment
    {
        public RemoteDeploymentBuilder(IOfferRemoteOperations dsl, ServerConfig server, ConDepSettings settings, CancellationToken token) : base(server, settings, token)
        {
            Dsl = dsl;
        }

        public override IOfferRemoteOperations Dsl { get; }
    }
}