using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

public class CodeStorage
{
    private readonly string _dbPath = "codes.db";
    private readonly string _connectionString;

    public CodeStorage()
    {
        _connectionString = $"Data Source={_dbPath};Version=3;";
        InitializeDatabase();
    }

    // Ensure database and table exist
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
                                IsUsed INTEGER DEFAULT 0
                              )";

        using var command = new SQLiteCommand(createTable, connection);
        command.ExecuteNonQuery();
    }

    // Store new codes in the DB
    public void SaveCodes(IEnumerable<string> codes)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        foreach (var code in codes)
        {
            var cmd = new SQLiteCommand("INSERT OR IGNORE INTO DiscountCodes (Code) VALUES (@code)", connection);
            cmd.Parameters.AddWithValue("@code", code);
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    // Check if code exists and is not used
    public bool UseCode(string code)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        var cmd = new SQLiteCommand("SELECT IsUsed FROM DiscountCodes WHERE Code = @code", connection);
        cmd.Parameters.AddWithValue("@code", code);
        var result = cmd.ExecuteScalar();

        if (result == null || Convert.ToInt32(result) == 1)
            return false;

        // Mark code as used
        cmd = new SQLiteCommand("UPDATE DiscountCodes SET IsUsed = 1 WHERE Code = @code", connection);
        cmd.Parameters.AddWithValue("@code", code);
        cmd.ExecuteNonQuery();

        return true;
    }

    public bool CodeExists(string code)
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        var cmd = new SQLiteCommand("SELECT 1 FROM DiscountCodes WHERE Code = @code", connection);
        cmd.Parameters.AddWithValue("@code", code);
        return cmd.ExecuteScalar() != null;
    }
}
