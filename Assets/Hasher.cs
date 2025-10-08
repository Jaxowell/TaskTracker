using System;
using System.Security.Cryptography;
using System.Text;

public static class Hasher
{
	/// <summary>
	/// Хеширует пароль с помощью SHA-256
	/// </summary>
	/// <param name="password">Пароль для хеширования</param>
	/// <returns>Хеш в виде hex-строки</returns>
	public static string HashPassword(string password)
	{
		using (SHA256 sha256Hash = SHA256.Create())
		{
			// Конвертируем пароль в байты
			byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

			// Конвертируем байты в hex-строку
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
			{
				builder.Append(bytes[i].ToString("x2"));
			}
			return builder.ToString();
		}
	}
	

	/// <summary>
	/// Проверяет, соответствует ли пароль хешу
	/// </summary>
	/// <param name="password">Пароль для проверки</param>
	/// <param name="hash">Ожидаемый хеш</param>
	/// <returns>True если пароль верный</returns>
	public static bool VerifyPassword(string password, string hash)
	{
		string hashOfInput = HashPassword(password);

		// Сравниваем хеши (регистронезависимо)
		StringComparer comparer = StringComparer.OrdinalIgnoreCase;
		return comparer.Compare(hashOfInput, hash) == 0;
	}
	
}