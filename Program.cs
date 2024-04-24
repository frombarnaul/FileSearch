using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Использование: utility.exe <расширение файла> <текст для поиска>");
            return;
        }

        string extension = args[0];
        string searchText = args[1];

        // Получаем список файлов с указанным расширением
        string[] files = Directory.GetFiles(Environment.CurrentDirectory, $"*.{extension}", SearchOption.AllDirectories);

        // Создаем очередь для хранения результатов поиска
        Queue<string> foundFiles = new Queue<string>();

        // Создаем массив потоков для параллельного поиска
        Thread[] threads = new Thread[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            int index = i; // Фиксируем значение индекса
            threads[i] = new Thread(() =>
            {
                string file = files[index]; // Используем фиксированное значение индекса
                if (FileContainsText(file, searchText))
                {
                    lock (foundFiles)
                    {
                        foundFiles.Enqueue(file);
                    }
                }
            });
            threads[i].Start();
        }

        // Ждем завершения всех потоков
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        // Выводим найденные файлы
        Console.WriteLine("Найденные файлы:");
        while (foundFiles.Count > 0)
        {
            Console.WriteLine(foundFiles.Dequeue());
        }
    }

    static bool FileContainsText(string filePath, string searchText)
    {
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(searchText))
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла {filePath}: {ex.Message}");
        }
        return false;
    }
}
