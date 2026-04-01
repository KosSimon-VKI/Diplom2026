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

        public void ShowOrders(
            IReadOnlyCollection<KitchenOrderCardViewModel> pickupCards,
            IReadOnlyCollection<KitchenOrderCardViewModel> noPickupCards,
            string emptyText)
        {
            var allCards = new List<KitchenOrderCardViewModel>(pickupCards.Count + noPickupCards.Count);
            allCards.AddRange(pickupCards);
            allCards.AddRange(noPickupCards);

            OrdersItemsControl.ItemsSource = allCards;
            var hasOrders = allCards.Count > 0;
            OrdersScrollViewer.Visibility = hasOrders ? Visibility.Visible : Visibility.Collapsed;

            EmptyOrdersText.Text = emptyText;
            EmptyOrdersText.Visibility = hasOrders ? Visibility.Collapsed : Visibility.Visible;
        }

        public void ShowMessage(string message)
        {
            OrdersItemsControl.ItemsSource = null;
            OrdersScrollViewer.Visibility = Visibility.Collapsed;
            EmptyOrdersText.Text = message;
            EmptyOrdersText.Visibility = Visibility.Visible;
        }
    }
}
