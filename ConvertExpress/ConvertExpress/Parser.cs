using System;
using System.Collections.Generic;
using System.Xml.Linq;
using HtmlAgilityPack; // Подключение библиотеки для парсинга HTML

namespace ConvertExpress
{
    public static class ConvertExpress1
    {
        // Метод для парсинга данных о валютах
        public static List<CurrencyItem> ParseCurrencies()
        {
            List<CurrencyItem> currencies = new List<CurrencyItem>(); // Создание списка для хранения данных о валютах

            string url = "https://www.cbr.ru/currency_base/daily/"; // URL адрес страницы с курсами валют ЦБ РФ
            HtmlWeb web = new HtmlWeb(); // Создание объекта для загрузки веб-страницы
            HtmlDocument document = web.Load(url); // Загрузка веб-страницы

            HtmlNode table = document.DocumentNode.SelectSingleNode("//table[@class='data']"); // Поиск таблицы с курсами валют

            if (table != null) // Если таблица найдена
            {
                HtmlNodeCollection rows = table.SelectNodes(".//tr"); // Получение коллекции строк таблицы

                for (int i = 1; i < rows.Count; i++) // Итерация по каждой строке таблицы, начиная со второй (первая строка содержит заголовки)
                {
                    HtmlNodeCollection cells = rows[i].SelectNodes(".//td"); // Получение коллекции ячеек текущей строки

                    if (cells != null && cells.Count >= 5) // Если ячейки найдены и их количество больше или равно 5
                    {
                        // Извлечение данных о валюте из ячеек
                        string currencyCode = cells[1].InnerText.Trim(); // Код валюты
                        string currencyName = cells[3].InnerText.Trim(); // Название валюты
                        string unit = cells[2].InnerText.Trim(); // Количество единиц валюты
                        string exchangeRate = cells[4].InnerText.Trim(); // Курс обмена

                        double rate;
                        if (double.TryParse(exchangeRate, out rate)) // Попытка преобразовать строку курса обмена в число
                        {
                            double units;
                            if (double.TryParse(unit, out units)) // Попытка преобразовать строку количества единиц валюты в число
                            {
                                // Вычисление курса, учитывая количество единиц валюты
                                rate /= units;
                            }

                            // Добавление данных о валюте в список
                            currencies.Add(new CurrencyItem
                            {
                                Code = currencyCode,
                                Name = currencyName,
                                Rate = rate.ToString()
                            });
                        }
                    }
                }
            }

            // Возвращение списка данных о валютах
            return currencies;
        }
    }

    // Класс для представления информации о валюте
    public class CurrencyItem
    {
        public string Code { get; set; } // Код валюты
        public string Name { get; set; } // Название валюты
        public string Rate { get; set; } // Курс обмена
    }
}
