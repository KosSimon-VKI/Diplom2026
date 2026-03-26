using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GardenNookWpf.Views.Controls
{
    /// <summary>
    /// Interaction logic for OrdersDisplayControl.xaml
    /// </summary>
    public partial class OrdersDisplayControl : UserControl
    {
        public OrdersDisplayControl()
        {
            InitializeComponent();
        }

        public void ShowOrders(IReadOnlyCollection<KitchenOrderCardViewModel> cards, string emptyText)
        {
            OrdersItemsControl.ItemsSource = cards;
            EmptyOrdersText.Text = emptyText;
            EmptyOrdersText.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowMessage(string message)
        {
            OrdersItemsControl.ItemsSource = null;
            EmptyOrdersText.Text = message;
            EmptyOrdersText.Visibility = Visibility.Visible;
        }
    }
}
