
using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;
using TransferModels.Staff;
using System.Text.Json;
using System.Net.Http;


namespace GardenNookWpf.Views
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        private string login = string.Empty;
        private string password = string.Empty;
        private readonly HttpClient _httpClient = new HttpClient();
        private string ServerAdress = "https://localhost:7235/api/auth/staff";

        public AuthorizationWindow()
        {
            InitializeComponent();
           
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;
            login = LoginBox.Text;
            password = PasswordBox.Text;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))//ПРОВЕРКА НА ПУСТОТУ
            {
                if (string.IsNullOrEmpty(login))
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#FFE22D2D");
                    LoginLabel.Foreground = new SolidColorBrush(color);
                    LoginBox.BorderBrush = new SolidColorBrush(color);
                }
                else
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#FF474747");
                    LoginLabel.Foreground = new SolidColorBrush(color);
                    LoginBox.BorderBrush = new SolidColorBrush(color);
                }

                if (string.IsNullOrEmpty(password))
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#FFE22D2D");
                    PasswordLabel.Foreground = new SolidColorBrush(color);
                    PasswordBox.BorderBrush = new SolidColorBrush(color);
                }
                else
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#FF474747");
                    PasswordLabel.Foreground = new SolidColorBrush(color);
                    PasswordBox.BorderBrush = new SolidColorBrush(color);
                }

                return;
            }
            else
            {
                Color color1 = (Color)ColorConverter.ConvertFromString("#FF474747");
                LoginLabel.Foreground = new SolidColorBrush(color1);
                LoginBox.BorderBrush = new SolidColorBrush(color1);
                PasswordLabel.Foreground = new SolidColorBrush(color1);
                PasswordBox.BorderBrush = new SolidColorBrush(color1);

                var user = new StaffDto
                {
                    Login = login,
                    Password = password
                };

                var json = JsonSerializer.Serialize(user);
                var request = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await _httpClient.PostAsync(ServerAdress, request);
                    if(!response.IsSuccessStatusCode)
                    {
                        return;
                    }

                    var responseJson = await response.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<StaffResponse>
                        (responseJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }
                        );

                    if (result.User == null)
                    {
                        Color color = (Color)ColorConverter.ConvertFromString("#FFE22D2D");
                        LoginBox.BorderBrush = new SolidColorBrush(color);
                        PasswordBox.BorderBrush = new SolidColorBrush(color);
                        ErrorLabel.Visibility = Visibility.Visible;
                        return;
                    }

                    else
                    {

                        switch (result.User.Role)
                        {
                            case "Повар":
                                KitchenWindow kitchenWindow = new KitchenWindow(_httpClient);
                                kitchenWindow.Show();
                                this.Close();
                                break;

                            case "Администратор":
                                break;

                            case "Бариста":
                                break;

                        }   
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }
    }
}
