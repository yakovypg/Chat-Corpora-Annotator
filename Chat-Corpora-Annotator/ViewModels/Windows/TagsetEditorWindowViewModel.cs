using ChatCorporaAnnotator.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using ChatCorporaAnnotator.Infrastructure.Commands;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class TagsetEditorWindowViewModel : ViewModel
    {
        public Action DeactivateAction { get; set; }

        #region SystemCommands

        public ICommand DeactivateWindowCommand { get; }
        public bool CanDeactivateWindowCommandExecute(object parameter)
        {
            return true;
        }
        public void OnDeactivateWindowCommandExecuted(object parameter)
        {
            if (!CanDeactivateWindowCommandExecute(parameter))
                return;

            DeactivateAction?.Invoke();
        }

        #endregion

        public TagsetEditorWindowViewModel()
        {
            DeactivateWindowCommand = new RelayCommand(OnDeactivateWindowCommandExecuted, CanDeactivateWindowCommandExecute);
        }
    }
}
