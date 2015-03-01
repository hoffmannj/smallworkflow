namespace SmallWorkflow
{
    public interface IRunnableTask<T>
    {
        void Run(T prev);
        void Run();
    }
}
