namespace Ninu.Emulator
{
    public interface IPersistable
    {
        void SaveState(SaveStateContext context);
        void LoadState(SaveStateContext context);
    }
}