using System.Security.Cryptography;
using System.Text;

public class CredentialHasher
{
    // Поля для хранения логина и пароля
    private string login;
    private string password;

    // Метод для установки логина и пароля
    public void SetCredentials(string login, string password)
    {
        this.login = login; // Устанавливаем логин
        this.password = password; // Устанавливаем пароль
    }

    // Метод для перемешивания логина и пароля
    private string MixCredentials()
    {
        // Создаем новый StringBuilder для накопления перемешанных символов
        StringBuilder mixed = new StringBuilder();

        // Определяем максимальную длину между логином и паролем
        int maxLength = Math.Max(login.Length, password.Length);

        // Цикл для перебора символов логина и пароля
        for (int i = 0; i < maxLength; i++)
        {
            // Если индекс меньше длины логина, добавляем символ логина
            if (i < login.Length)
            {
                mixed.Append(login[i]); // Добавляем символ логина в StringBuilder
            }
            // Если индекс меньше длины пароля, добавляем символ пароля
            if (i < password.Length)
            {
                mixed.Append(password[i]); // Добавляем символ пароля в StringBuilder
            }
        }
        Console.WriteLine(mixed.ToString());
        // Преобразуем содержимое StringBuilder в строку и возвращаем
        return mixed.ToString();
    }

    // Метод для хеширования перемешанных логина и пароля
    public byte[] HashCredentials()
    {
        // Получаем перемешанные логин и пароль
        string mixedCredentials = MixCredentials();

        // Создаем объект SHA256 для хеширования
        using (SHA256 sha256 = SHA256.Create())
        {
            // Вычисляем хеш для перемешанных данных и возвращаем его в виде массива байт
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(mixedCredentials));
        }
    }

    // Метод для проверки введенного пароля с хешированным паролем
    public bool VerifyPassword(string enteredPassword, byte[] storedHash, string enteredLogin)
    {
        // Получаем перемешанные логин и пароль для проверки
        this.password = enteredPassword;
        this.login = enteredLogin;// Устанавливаем введенный пароль
        string mixedCredentials = MixCredentials(); // Перемешиваем логин и введенный пароль

        // Хешируем перемешанные данные
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashToCompare = sha256.ComputeHash(Encoding.UTF8.GetBytes(mixedCredentials));

            // Сравниваем хеши
            return hashToCompare.SequenceEqual(storedHash);
        }
    }
}
