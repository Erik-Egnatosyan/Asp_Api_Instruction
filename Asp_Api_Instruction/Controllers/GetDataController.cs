using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace Asp_Api_Instruction.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetDataController : ControllerBase
    {
        // Определяем метод, который обрабатывает GET-запрос по адресу "ConnectToDB"
        [HttpGet("ConnectToDB")]
        public void DBConnect()
        {
            // Определяем строку подключения к базе данных
            string connectionString = "Data Source=(localdb)\\mssqllocaldb;Database=Rates;Trusted_Connection=True;";

            // Создаем объект SqlConnection, используя строку подключения, и оборачиваем его в блок using для автоматического закрытия соединения
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Открываем соединение с базой данных
                connection.Open();
            }
        }

        [HttpGet("CheckDBConnection")] // атрибут для маршрутизации HTTP GET-запроса по адресу "api/CheckDBConnection"
        public ActionResult<string> CheckDBConnection()
        {
            // строка подключения к базе данных
            string connectionString = "Data Source=(localdb)\\mssqllocaldb;Database=Rates;Trusted_Connection=True;";

            // объявление объекта SqlConnection в блоке using для автоматического закрытия подключения
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // открытие подключения к базе данных
                    connection.Open();

                    // если подключение успешно, возвращается строка "Connection successful!"
                    return "Connection successful!";
                }
                catch (Exception ex)
                {
                    // если произошла ошибка, возвращается строка "Connection failed: " с сообщением об ошибке
                    return $"Connection failed: {ex.Message}";
                }
            }
        }


        [HttpGet("GetDataFromDB")] // атрибут для маршрутизации HTTP GET-запроса по адресу "api/GetDataFromDB"
        public ActionResult<List<string>> GetDataFromDB()
        {
            // строка подключения к базе данных
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=Rates;Trusted_Connection=True;";

            // строка SQL-запроса для извлечения данных из таблицы JsonKeyValues
            string query = "SELECT Currency FROM JsonKeyValues";

            // объявление объектов SqlConnection и SqlCommand в блоке using для автоматического закрытия подключения и освобождения ресурсов
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection);

            // создание списка для хранения результатов запроса
            var currencies = new List<string>();

            // открытие подключения к базе данных
            connection.Open();

            // объявление объекта SqlDataReader в блоке using для автоматического закрытия и освобождения ресурсов
            // выполнение SQL-запроса с помощью метода ExecuteReader() и извлечение данных с помощью метода Read()
            // добавление каждой строки, соответствующей столбцу Currency, в список currencies
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    currencies.Add(reader.GetString(0));
                }
            }

            // возвращение списка currencies в формате JSON
            return Ok(currencies);
        }

        [HttpPost("CreateTable")]
        public ActionResult CreateTable()
        {
            try
            {
                // строка подключения к базе данных
                string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Rates;Integrated Security=True";

                // объявление объекта SqlConnection в блоке using для автоматического закрытия подключения
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // SQL-запрос на создание таблицы
                    string query = "Create table MyTable (id int PRIMARY KEY, name varchar(50))";

                    // создание объекта SqlCommand с использованием SQL-запроса и объекта SqlConnection
                    SqlCommand command = new SqlCommand(query, connection);

                    // открытие подключения к базе данных
                    connection.Open();

                    // выполнение SQL-запроса на создание таблицы
                    command.ExecuteNonQuery();
                }

                // возвращаем успешный результат в виде сообщения
                return Ok("Таблица успешно создана!");
            }
            catch (Exception ex)
            {
                // возвращаем ошибку в виде объекта BadRequestResult с сообщением об ошибке
                return BadRequest($"Ошибка при создании таблицы: {ex.Message}");
            }
        }


        [HttpGet("GetRates")]
        public ActionResult<string> GetRates()
        {
            try
            {
                // Создаем экземпляр HttpClient и добавляем заголовок apikey
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("apikey", "vbjeljWWPNFs5FGuVoXeefSI6jdplMS1");

                // Указываем URL API для получения курсов валют
                string url = @"https://api.apilayer.com/exchangerates_data/latest";

                // Выполняем HTTP-запрос и получаем ответ
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                // Если ответ успешный, то получаем результат в виде строки
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(result);
                    return Ok(result); // Возвращаем результат в виде объекта OkResult
                }
                else
                {
                    return NotFound("API не найдено!"); // Возвращаем сообщение об ошибке в виде объекта NotFoundResult
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}"); // Возвращаем сообщение об ошибке в виде объекта BadRequestResult
            }
        }

        [HttpGet("GetJsonRates")]
        public IActionResult GetJsonRates()
        {
            // строка подключения к базе данных
            string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Rates;Integrated Security=True";

            // объект SqlConnection для установления соединения с базой данных
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            // строка для хранения ответа от API
            string SaveInputs = string.Empty;

            // словарь для хранения пар валют-курс
            Dictionary<string, decimal> keyValuePairs = new Dictionary<string, decimal>();

            // объект HttpClient для отправки HTTP-запросов
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", "vbjeljWWPNFs5FGuVoXeefSI6jdplMS1");

            // адрес API, откуда получаем курсы валют
            string url = @"https://api.apilayer.com/exchangerates_data/latest";

            // отправляем GET-запрос по указанному адресу
            HttpResponseMessage response = httpClient.GetAsync(url).Result;

            // проверяем успешность получения ответа
            if (response.IsSuccessStatusCode)
            {
                // получаем ответ в виде строки
                SaveInputs = response.Content.ReadAsStringAsync().Result;

                // парсим ответ в формат JSON с помощью объекта JObject
                JObject json = JObject.Parse(SaveInputs);

                // извлекаем словарь пар валют-курс из JSON-объекта
                keyValuePairs = json["rates"].ToObject<Dictionary<string, decimal>>();

                // SQL-запрос на добавление записей в таблицу
                string insertCommand = "INSERT INTO JsonKeyValues (Currency, RateMoney) VALUES (@currency, @rate)";

                // создаем SqlCommand для выполнения SQL-запроса на добавление записей
                SqlCommand command = new SqlCommand(insertCommand, connection);

                // добавляем записи в таблицу
                foreach (KeyValuePair<string, decimal> pair in keyValuePairs)
                {
                    command.Parameters.AddWithValue("@currency", pair.Key);
                    command.Parameters.AddWithValue("@rate", pair.Value);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
            }
            else
            {
                // если запрос был неуспешным, возвращаем статус "Not Found" (код 404)
                return NotFound();
            }

            // закрываем соединение с базой данных
            connection.Close();

            // возвращаем статус "OK" (код 200)
            return Ok();
        }

    }
}