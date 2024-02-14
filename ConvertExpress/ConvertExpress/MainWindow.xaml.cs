using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ConvertExpress
{
    /// Главное окно приложения
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
    }
}
