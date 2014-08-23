using ConDep.Dsl.Operations;

namespace ConDep.Dsl
{
    /// <summary>
    /// Expose functionality for custom remote execution operations to be added to ConDep's execution sequence.
    /// </summary>
    public interface IConfigureRemoteExecution
    {
        void AddOperation(RemoteCompositeOperation operation);
        void AddOperation(IExecuteOnServer operation);
    }
}