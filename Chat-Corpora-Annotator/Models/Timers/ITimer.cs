namespace ChatCorporaAnnotator.Models.Timers
{
    internal interface ITimer
    {
        bool IsActive { get; }

        void Stop();
        void Start();
    }
}
