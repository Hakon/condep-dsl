using ConDep.Dsl.Validation;

namespace ConDep.Dsl.SemanticModel
{
    public interface IManageSequence<in T> : IValidate
    {
        void Add(T operation, bool addFirst = false);
        //bool IsValid(Notification notification);
    }
}