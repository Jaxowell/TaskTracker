using System;
using System.Security.Cryptography;
using System.Text;

public static class Hasher
{
	/// <summary>
	/// �������� ������ � ������� SHA-256
	/// </summary>
	/// <param name="password">������ ��� �����������</param>
	/// <returns>��� � ���� hex-������</returns>
	public static string HashPassword(string password)
	{
		using (SHA256 sha256Hash = SHA256.Create())
		{
			// ������������ ������ � �����
			byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

			// ������������ ����� � hex-������
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
			{
				builder.Append(bytes[i].ToString("x2"));
			}
			return builder.ToString();
		}
	}
	

	/// <summary>
	/// ���������, ������������� �� ������ ����
	/// </summary>
	/// <param name="password">������ ��� ��������</param>
	/// <param name="hash">��������� ���</param>
	/// <returns>True ���� ������ ������</returns>
	public static bool VerifyPassword(string password, string hash)
	{
		string hashOfInput = HashPassword(password);

		// ���������� ���� (������������������)
		StringComparer comparer = StringComparer.OrdinalIgnoreCase;
		return comparer.Compare(hashOfInput, hash) == 0;
	}
	
}