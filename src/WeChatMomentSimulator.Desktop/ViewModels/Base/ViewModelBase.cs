using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeChatMomentSimulator.Desktop.ViewModels
{
    /// <summary>
    /// Base class for all ViewModel classes in the application
    /// Implements INotifyPropertyChanged for binding support
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Event raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets property value and raises PropertyChanged event if value changed
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="field">Backing field reference</param>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Property name (auto-filled by compiler)</param>
        /// <returns>True if value changed</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        
        /// <summary>
        /// Sets property value and raises PropertyChanged event if value changed
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="field">Backing field reference</param>
        /// <param name="value">New value</param>
        /// <param name="onChanged">Action to invoke when value changes</param>
        /// <param name="propertyName">Property name (auto-filled by compiler)</param>
        /// <returns>True if value changed</returns>
        /// <remarks>
        /// This method allows for additional actions to be taken when a property changes,
        /// such as updating other properties or performing validation.
        /// </remarks>
        protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        /// <summary>
        /// Raises PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}