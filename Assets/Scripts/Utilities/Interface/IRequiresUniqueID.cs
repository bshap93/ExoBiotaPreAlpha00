namespace Utilities.Interface
{
    public interface IRequiresUniqueID
    {
        string UniqueID { get; }
        void SetUniqueID();
        bool IsUniqueIDEmpty();
    }
}
