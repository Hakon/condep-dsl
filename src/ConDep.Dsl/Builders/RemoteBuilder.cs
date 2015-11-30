using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public abstract class RemoteBuilder : IOfferResult
    {
        public RemoteBuilder(ServerConfig server, ConDepSettings settings, CancellationToken token)
        {
            Server = server;
            Settings = settings;
            Token = token;
        }

        public ConDepSettings Settings { get; }

        public CancellationToken Token { get; }

        public ServerConfig Server { get; }

        public Result Result { get; set; }

        public abstract IOfferRemoteOperations Dsl { get; }
    }
}