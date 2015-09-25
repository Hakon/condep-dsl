using System;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Sequence
{
    public interface IOfferRemoteSequence : IManageSequence<IExecuteRemotely>, IExecuteRemotely
    {
        IOfferCompositeSequence NewCompositeSequence(RemoteCompositeOperation operation);
        IOfferCompositeSequence NewConditionalCompositeSequence(Predicate<ServerInfo> condition);
        IOfferCompositeSequence NewConditionalCompositeSequence(string conditionScript);
    }
}