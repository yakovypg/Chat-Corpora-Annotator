using ChatCorporaAnnotator.Infrastructure.Commands.Base;
using System;
using System.Windows;

namespace ChatCorporaAnnotator.Infrastructure.Commands
{
    internal class CloseWindowCommand : Command
    {
        private readonly Action? _lastAction;

        public CloseWindowCommand(Action? lastAction = null)
        {
            _lastAction = lastAction;
        }

        public override bool CanExecute(object? parameter)
        {
            return parameter is Window;
        }

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _lastAction?.Invoke();
            (parameter as Window).Close();
        }
    }
}
