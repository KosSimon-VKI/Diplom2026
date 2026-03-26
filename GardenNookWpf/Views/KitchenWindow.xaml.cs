using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GardenNookWpf.Views.Controls;
using TransferModels.Kitchen;

namespace GardenNookWpf.Views
{
    /// <summary>
    /// Логика взаимодействия для KitchenWindow.xaml
    /// </summary>
    public partial class KitchenWindow : Window
    {
        private const string KitchenOrdersAddress = "https://localhost:7235/api/kitchen/orders";
        private const int OverdueOrderMinutes = 15;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _ordersRefreshTimer;
        private readonly DispatcherTimer _orderElapsedTimer;
        private bool _isLoadingOrders;
        private List<KitchenOrderCardViewModel> _orderCards = new List<KitchenOrderCardViewModel>();

        public KitchenWindow() : this(new HttpClient())
        {
        }

        public KitchenWindow(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _ordersRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(7)
            };
            _ordersRefreshTimer.Tick += OrdersRefreshTimer_Tick;

            _orderElapsedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _orderElapsedTimer.Tick += OrderElapsedTimer_Tick;

            InitializeComponent();
            Loaded += KitchenWindow_Loaded;
            Closed += KitchenWindow_Closed;
        }

        private async void KitchenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeButtonBackground(OrdersButton);
            await LoadOrdersAsync();
            _ordersRefreshTimer.Start();
            _orderElapsedTimer.Start();
        }

        private void KitchenWindow_Closed(object? sender, EventArgs e)
        {
            _ordersRefreshTimer.Stop();
            _orderElapsedTimer.Stop();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow authorizationWindow = new AuthorizationWindow();
            authorizationWindow.Show();
            this.Close();
        }

        private void ChangeButtonBackground(object sender)
        {
            Color selected = (Color)ColorConverter.ConvertFromString("#FF606E52");
            Color white = Colors.White;
            Color black = Colors.Black;

            OrdersButton.Background = new SolidColorBrush(white);
            TechCardsButton.Background = new SolidColorBrush(white);
            PreparationsButton.Background = new SolidColorBrush(white);
            StopListButton.Background = new SolidColorBrush(white);
            WriteOffButton.Background = new SolidColorBrush(white);

            OrdersButton.Foreground = new SolidColorBrush(black);
            TechCardsButton.Foreground = new SolidColorBrush(black);
            PreparationsButton.Foreground = new SolidColorBrush(black);
            StopListButton.Foreground = new SolidColorBrush(black);
            WriteOffButton.Foreground = new SolidColorBrush(black);

            if (sender is not Button button)
            {
                return;
            }

            button.Background = new SolidColorBrush(selected);
            button.Foreground = new SolidColorBrush(white);
        }

        private async void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeButtonBackground(sender);
            await LoadOrdersAsync();
        }

        private void TechCardsButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeButtonBackground(sender);
        }

        private void PreparationsButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeButtonBackground(sender);
        }

        private void StopListButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeButtonBackground(sender);
        }

        private void WriteOffButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeButtonBackground(sender);
        }

        private async void OrdersRefreshTimer_Tick(object? sender, EventArgs e)
        {
            await LoadOrdersAsync();
        }

        private void OrderElapsedTimer_Tick(object? sender, EventArgs e)
        {
            UpdateOrderHeaders(DateTime.Now);
        }

        private async Task LoadOrdersAsync()
        {
            if (_isLoadingOrders)
            {
                return;
            }

            try
            {
                _isLoadingOrders = true;
                var response = await _httpClient.GetAsync(KitchenOrdersAddress);

                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    OrdersDisplayControl.ShowMessage("Нет доступа к заказам");
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    OrdersDisplayControl.ShowMessage("Не удалось загрузить заказы");
                    return;
                }

                var responseJson = await response.Content.ReadAsStringAsync();

                var data = JsonSerializer.Deserialize<KitchenOrdersResponse>(responseJson, JsonOptions)
                    ?? new KitchenOrdersResponse();

                var cards = BuildOrderCards(data.Orders ?? new List<KitchenOrderDto>());
                _orderCards = cards;
                UpdateOrderHeaders(DateTime.Now);

                OrdersDisplayControl.ShowOrders(cards, "Нет активных заказов");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Garden Nook", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoadingOrders = false;
            }
        }

        private void UpdateOrderHeaders(DateTime now)
        {
            foreach (var card in _orderCards)
            {
                card.ElapsedText = BuildElapsedText(card.CreatedAt, now);
                card.IsOverdue = IsOrderOverdue(card.CreatedAt, now);
            }
        }

        private static string BuildElapsedText(DateTime? createdAt, DateTime now)
        {
            if (createdAt == null)
            {
                return "00:00";
            }

            var elapsed = now - createdAt.Value;
            if (elapsed < TimeSpan.Zero)
            {
                elapsed = TimeSpan.Zero;
            }

            var totalMinutes = (int)elapsed.TotalMinutes;
            return $"{totalMinutes:00}:{elapsed.Seconds:00}";
        }

        private static bool IsOrderOverdue(DateTime? createdAt, DateTime now)
        {
            if (createdAt == null)
            {
                return false;
            }

            return now - createdAt.Value >= TimeSpan.FromMinutes(OverdueOrderMinutes);
        }

        private static List<KitchenOrderCardViewModel> BuildOrderCards(IEnumerable<KitchenOrderDto> orders)
        {
            var cards = new List<KitchenOrderCardViewModel>();

            foreach (var order in orders)
            {
                var displayItems = new List<KitchenOrderItemViewModel>();
                var positionNumber = 1;

                foreach (var dish in order.Dishes ?? new List<KitchenOrderDishDto>())
                {
                    var toppingsLine = BuildDishToppingsLine(dish.Toppings);

                    displayItems.Add(new KitchenOrderItemViewModel
                    {
                        NameLine = $"{positionNumber}) {dish.Name} x{FormatQuantity(dish.Quantity)}",
                        ToppingsLine = toppingsLine,
                        ToppingsVisibility = string.IsNullOrWhiteSpace(toppingsLine) ? Visibility.Collapsed : Visibility.Visible
                    });

                    positionNumber++;
                }

                foreach (var topping in order.Toppings ?? new List<KitchenOrderStandaloneToppingDto>())
                {
                    displayItems.Add(new KitchenOrderItemViewModel
                    {
                        NameLine = $"{positionNumber}) {topping.Name} x{FormatQuantity(topping.Quantity)}",
                        ToppingsLine = string.Empty,
                        ToppingsVisibility = Visibility.Collapsed
                    });

                    positionNumber++;
                }

                if (displayItems.Count == 0)
                {
                    continue;
                }

                cards.Add(new KitchenOrderCardViewModel
                {
                    OrderId = order.OrderId,
                    CreatedAt = order.CreatedAt,
                    OrderNumberText = order.OrderId.ToString(CultureInfo.CurrentCulture),
                    ElapsedText = BuildElapsedText(order.CreatedAt, DateTime.Now),
                    IsOverdue = IsOrderOverdue(order.CreatedAt, DateTime.Now),
                    OrderTypeText = BuildOrderTypeText(order.OrderType),
                    OrderCommentText = order.Comment ?? string.Empty,
                    OrderCommentVisibility = string.IsNullOrWhiteSpace(order.Comment) ? Visibility.Collapsed : Visibility.Visible,
                    DisplayItems = displayItems
                });
            }

            return cards;
        }

        private static string BuildDishToppingsLine(IEnumerable<KitchenOrderDishToppingDto>? toppings)
        {
            if (toppings == null)
            {
                return string.Empty;
            }

            var toppingList = toppings.ToList();
            if (toppingList.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(Environment.NewLine, toppingList.Select(t => $"+ {t.Name} x{FormatQuantity(t.Quantity)}"));
        }

        private static string BuildOrderTypeText(string? orderType)
        {
            if (string.IsNullOrWhiteSpace(orderType))
            {
                return "Не указан тип";
            }

            return orderType.Trim();
        }

        private static string FormatQuantity(decimal quantity)
        {
            if (quantity == decimal.Truncate(quantity))
            {
                return quantity.ToString("0", CultureInfo.CurrentCulture);
            }

            return quantity.ToString("0.##", CultureInfo.CurrentCulture);
        }

    }
}
