using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Бронирование аудитории
            var result = bookingModule.BookRoom(
                "101",
                new DateTime(2024, 1, 15, 10, 0, 0),
                new DateTime(2024, 1, 15, 12, 0, 0),
                "Лекция по математике",
                "Иванов И.И."
            );

            if (result.Success)
            {
                Console.WriteLine("Бронирование успешно!");
            }
            else
            {
                Console.WriteLine($"Ошибка: {result.ErrorMessage}");
            }
        }
    }
}
