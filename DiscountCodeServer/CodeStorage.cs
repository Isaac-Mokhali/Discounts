using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CodeStorage
{
    private readonly string _dbPath = "codes.db";
    private readonly string _connectionString;

    public CodeStorage()
    {
        _connectionString = $"Data Source={_dbPath};Version=3;";
        InitializeDatabase();
    }

    // Initialize the database and table
    private void InitializeDatabase()
    {
        if (!File.Exists(_dbPath))
        {
            SQLiteConnection.CreateFile(_dbPath);
        }

        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        string createTable = @"CREATE TABLE IF NOT EXISTS DiscountCodes (
                                Code TEXT PRIMARY KEY,
                                Salt TEXT,
                                IsUsed INTEGER DEFAULT 0
                              )";

        using var command = new SQLiteCommand(createTable, connection);
        command.ExecuteNonQuery();
    }

    // Securely hash discount code with a salt
    private string HashCode(string code, string salt)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] saltedCode = Encoding.UTF8.GetBytes(code + salt); // Combine code and salt
            byte[] hash = sha256.ComputeHash(saltedCode);
            return Convert.ToBase64String(hash);
        }
    }

    public void SaveCodes(IEnumerable<string> codes)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        foreach (var code in codes)
        {
            string salt = Guid.NewGuid().ToString(); // Generate a unique salt for each code
            string hashedCode = HashCode(code, salt);

            // Save the hashed code and the salt
            var cmd = new SQLiteCommand("INSERT OR IGNORE INTO DiscountCodes (Code, Salt) VALUES (@code, @salt)", connection);
            cmd.Parameters.AddWithValue("@code", hashedCode);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    // Attempts to use a discount code. Returns true if the code is valid and unused, and marks it as used.
    public bool UseCode(string code)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        var cmd = new SQLiteCommand("SELECT Code, Salt, IsUsed FROM DiscountCodes", connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            string storedHash = reader.GetString(0);
            string salt = reader.GetString(1);
            bool isUsed = reader.GetInt32(2) == 1;

            // Hash the input code using the retrieved salt
            string computedHash = HashCode(code, salt);

            if (computedHash == storedHash)
            {
                if (isUsed)
                    return false;

                // Mark the code as used in the database
                var updateCmd = new SQLiteCommand("UPDATE DiscountCodes SET IsUsed = 1 WHERE Code = @code", connection);
                updateCmd.Parameters.AddWithValue("@code", storedHash);
                updateCmd.ExecuteNonQuery();
                return true;
            }
        }

        return false; // No matching valid code found
    }

    // Checks if a discount code exists and is valid (regardless of whether it's been used)
    public bool CodeExists(string code)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        var cmd = new SQLiteCommand("SELECT Code, Salt FROM DiscountCodes", connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            string storedHash = reader.GetString(0);
            string salt = reader.GetString(1);


            string computedHash = HashCode(code, salt);


            if (computedHash == storedHash)
                return true; // Code exists and matches
        }

        return false; // No matching code found
    }
}

