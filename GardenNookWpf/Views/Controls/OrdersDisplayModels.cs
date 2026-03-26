using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GardenNookWpf.Views.Controls
{
    public sealed class KitchenOrderCardViewModel : INotifyPropertyChanged
    {
        private string _elapsedText = string.Empty;
        private bool _isOverdue;

        public int OrderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string OrderNumberText { get; set; } = string.Empty;
        public string OrderTypeText { get; set; } = string.Empty;
        public string PickupAtText { get; set; } = string.Empty;
        public Visibility PickupAtVisibility { get; set; } = Visibility.Collapsed;
        public string OrderCommentText { get; set; } = string.Empty;
        public Visibility OrderCommentVisibility { get; set; } = Visibility.Collapsed;

        public string ElapsedText
        {
            get => _elapsedText;
            set
            {
                if (_elapsedText == value)
                {
                    return;
                }

                _elapsedText = value;
                OnPropertyChanged();
            }
        }

        public bool IsOverdue
        {
            get => _isOverdue;
            set
            {
                if (_isOverdue == value)
                {
                    return;
                }

                _isOverdue = value;
                OnPropertyChanged();
            }
        }

        public List<KitchenOrderItemViewModel> DisplayItems { get; set; } = new List<KitchenOrderItemViewModel>();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class KitchenOrderItemViewModel
    {
        public string NameLine { get; set; } = string.Empty;
        public string ToppingsLine { get; set; } = string.Empty;
        public Visibility ToppingsVisibility { get; set; } = Visibility.Visible;
    }
}
