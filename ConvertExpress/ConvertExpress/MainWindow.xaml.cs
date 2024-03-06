using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CurrencyParser
{
    /// <summary>
    /// Главное окно приложения
    /// </summary>
    public partial class MainWindow : Window
    {
        // Таймер для обновления времени
        private DispatcherTimer _timer;

        // Конструктор класса MainWindow
        public MainWindow()
        {
            InitializeComponent();

            // Установка текущего времени при запуске приложения
            UpdateTime("Данные за " + DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss") + " GMT+03:00");

            // Установка DataContext для привязки данных
            DataContext = this;

            // Привязка обработчика события изменения текста в TextBox
            inputPriceTo.TextChanged += TextBox_TextChanged;

            // Загрузка данных о валютах
            LoadCurrencyData();

            // Установка значения по умолчанию для TextBox
            inputPriceTo.Text = "1";

            // Создание и настройка таймера для обновления времени каждую секунду
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        // Обработчик события таймера для обновления времени
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Формирование текущего времени и обновление его в интерфейсе
            string currentTime = "Данные за " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " GMT+03:00";
            UpdateTime(currentTime);
        }

        // Метод для обновления отображаемого времени
        private void UpdateTime(string time)
        {
            // Обновление содержимого TextBlock
            TimeTextbox.Text = time;
        }

        // Список объектов CurrencyItem для хранения данных о валютах
        private List<CurrencyItem> _currencyItems;

        // Свойство для доступа к списку валют
        public List<CurrencyItem> CurrencyItems
        {
            get { return _currencyItems; }
            set
            {
                _currencyItems = value;
                OnPropertyChanged("CurrencyItems");
            }
        }

        // Список объектов CurrencyItem для хранения данных о валютах для исходной валюты
        public List<CurrencyItem> FromCode { get; set; }

        // Список объектов CurrencyItem для хранения данных о валютах для целевой валюты
        public List<CurrencyItem> FromCurrencyItems { get; set; }
        public List<CurrencyItem> ToCurrencyItems { get; set; }

        // Метод для загрузки данных о валютах
        private void LoadCurrencyData()
        {
            // Получение данных о валютах с использованием парсера
            CurrencyItems = CurrencyParser1.ParseCurrencies();

            // Добавление рубля в список валют
            CurrencyItems.Add(new CurrencyItem { Code = "RUB", Name = "Рубль", Rate = "1" });

            // Создание отсортированных списков для каждого ComboBox
            FromCurrencyItems = CurrencyItems.OrderBy(currency => currency.Name)
                                              .Select(currency => new CurrencyItem
                                              {
                                                  Name = $"{currency.Name} ({currency.Code})",
                                                  Code = currency.Code,
                                                  Rate = currency.Rate
                                              })
                                              .ToList();
            ToCurrencyItems = CurrencyItems.OrderBy(currency => currency.Name)
                                            .Select(currency => new CurrencyItem
                                            {
                                                Name = $"{currency.Name} ({currency.Code})",
                                                Code = currency.Code,
                                                Rate = currency.Rate
                                            })
                                            .ToList();
        }

        // Обработчик изменения выбранной валюты в ComboBox
        private void CurrencyComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateExchangeRate();
        }

        // Метод для обновления курса обмена
        private void UpdateExchangeRate()
        {
            // Проверка выбраны ли валюты для обмена
            if (fromCurrencyComboBox.SelectedItem != null && toCurrencyComboBox.SelectedItem != null)
            {
                // Извлечение объектов CurrencyItem для выбранных валют
                CurrencyItem fromCurrencyItem = (CurrencyItem)fromCurrencyComboBox.SelectedItem;
                CurrencyItem toCurrencyItem = (CurrencyItem)toCurrencyComboBox.SelectedItem;

                // Извлечение кодов валют из объектов CurrencyItem
                string fromCurrencyCode = fromCurrencyItem.Code;
                string toCurrencyCode = toCurrencyItem.Code;

                // Извлечение курсов обмена для выбранных валют
                string fromRate = fromCurrencyComboBox.SelectedValue.ToString();
                string toRate = toCurrencyComboBox.SelectedValue.ToString();

                // Проверка, удалось ли извлечь курсы обмена
                if (double.TryParse(fromRate, out double fromValue) && double.TryParse(toRate, out double toValue))
                {
                    // Проверка, корректен ли введенный пользователем текст в TextBox
                    string check = inputPriceTo.Text;
                    bool isDouble = double.TryParse(check, out _);
                    if (isDouble == true && Convert.ToDouble(inputPriceTo.Text) > -1)
                    {
                        // Вычисление курса обмена и обновление интерфейса
                        double inputPriceToConvert = Convert.ToDouble(inputPriceTo.Text);
                        double exchangeRate = toValue / fromValue;
                        double convertedAmount = inputPriceToConvert * exchangeRate;
                        exchangeRateTextBlock.Text = $"{convertedAmount:F4}";

                        // Вычисление курса для одной единицы валюты и обновление интерфейса
                        double reverseExchangeRate = 1 / exchangeRate;
                        double OnePriceExchangeRate = exchangeRate;
                        PriceShowOne.Text = $"1 {toCurrencyCode} = {OnePriceExchangeRate:F4} {fromCurrencyCode}";
                        PriceShowSecond.Text = $"1 {fromCurrencyCode} = {reverseExchangeRate:F4} {toCurrencyCode}";
                    }
                    else
                    {
                        // Вывод сообщения об ошибке в случае некорректного ввода
                        exchangeRateTextBlock.Text = $"???";
                    }
                }
            }
        }

        // Метод для получения кода валюты по ее названию
        private string GetCurrencyCode(string currencyName)
        {
            // Поиск объекта CurrencyItem по имени валюты и возврат свойства Code
            CurrencyItem currencyItem = CurrencyItems.FirstOrDefault(item => item.Name == currencyName);
            return currencyItem != null ? currencyItem.Code : string.Empty;
        }

        // Обработчик нажатия кнопки для обмена выбранных валют местами
        private void SwapCurrenciesButton_Click(object sender, RoutedEventArgs e)
        {
            // Обмен местами выбранных валют
            int fromIndex = fromCurrencyComboBox.SelectedIndex;
            fromCurrencyComboBox.SelectedIndex = toCurrencyComboBox.SelectedIndex;
            toCurrencyComboBox.SelectedIndex = fromIndex;
            string fromRate = fromCurrencyComboBox.SelectedValue.ToString();
            string toRate = toCurrencyComboBox.SelectedValue.ToString();
            CurrencyItem fromCurrencyItem = (CurrencyItem)fromCurrencyComboBox.SelectedItem;
            CurrencyItem toCurrencyItem = (CurrencyItem)toCurrencyComboBox.SelectedItem;

            // Извлечение кодов валют из объектов CurrencyItem
            string fromCurrencyCode = fromCurrencyItem.Code;
            string toCurrencyCode = toCurrencyItem.Code;

            // Проверка, удалось ли извлечь курсы обмена
            if (double.TryParse(fromRate, out double fromValue) && double.TryParse(toRate, out double toValue))
            {
                // Проверка, корректен ли введенный пользователем текст в TextBox
                string check = inputPriceTo.Text;
                bool isDouble = double.TryParse(check, out _);
                if (isDouble == true && Convert.ToDouble(inputPriceTo.Text) > -1)
                {
                    // Вычисление курса обмена и обновление интерфейса
                    double inputPriceToConvert = Convert.ToDouble(inputPriceTo.Text);
                    double exchangeRate = toValue / fromValue;
                    double convertedAmount = inputPriceToConvert * exchangeRate;
                    exchangeRateTextBlock.Text = $"{convertedAmount:F4}";

                    // Вычисление курса для одной единицы валюты и обновление интерфейса
                    double reverseExchangeRate = 1 / exchangeRate;
                    double OnePriceExchangeRate = exchangeRate;
                    PriceShowOne.Text = $"1 {toCurrencyCode} = {OnePriceExchangeRate:F4} {fromCurrencyCode}";
                    PriceShowSecond.Text = $"1 {fromCurrencyCode} = {reverseExchangeRate:F4} {toCurrencyCode}";
                }
                else
                {
                    // Вывод сообщения об ошибке в случае некорректного ввода
                    exchangeRateTextBlock.Text = $"???";
                }
            }
        }

        // Обработчик изменения текста в TextBox
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Обновление курса обмена при изменении текста в TextBox
            if (fromCurrencyComboBox.SelectedItem != null && toCurrencyComboBox.SelectedItem != null)
            {
                UpdateExchangeRate();
            }
        }

        // Обработчик ввода текста в TextBox
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Проверка ввода пользователя: разрешены только цифры и запятая
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != ',')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        // Событие, вызываемое при изменении свойства объекта
        public event PropertyChangedEventHandler PropertyChanged;

        // Метод для вызова события изменения свойства
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
