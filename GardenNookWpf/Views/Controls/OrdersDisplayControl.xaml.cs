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
            PickupOrdersItemsControl.ItemsSource = pickupCards;
            NoPickupOrdersItemsControl.ItemsSource = noPickupCards;

            var hasPickupCards = pickupCards.Count > 0;
            var hasNoPickupCards = noPickupCards.Count > 0;

            PickupOrdersSection.Visibility = hasPickupCards ? Visibility.Visible : Visibility.Collapsed;
            NoPickupOrdersSection.Visibility = hasNoPickupCards ? Visibility.Visible : Visibility.Collapsed;

            EmptyOrdersText.Text = emptyText;
            EmptyOrdersText.Visibility = hasPickupCards || hasNoPickupCards
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public void ShowMessage(string message)
        {
            PickupOrdersItemsControl.ItemsSource = null;
            NoPickupOrdersItemsControl.ItemsSource = null;
            PickupOrdersSection.Visibility = Visibility.Collapsed;
            NoPickupOrdersSection.Visibility = Visibility.Collapsed;
            EmptyOrdersText.Text = message;
            EmptyOrdersText.Visibility = Visibility.Visible;
        }
    }
}
