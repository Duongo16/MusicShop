using System.Windows.Input;

namespace MusicShop.WPF.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object?, Task>? _execAsync;
        private readonly Predicate<object?>? _can;
        public RelayCommand(Func<object?, Task> execAsync, Predicate<object?>? can = null)
        { _execAsync = execAsync; _can = can; }

        public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public async void Execute(object? parameter) => await (_execAsync?.Invoke(parameter) ?? Task.CompletedTask);
    }
}
