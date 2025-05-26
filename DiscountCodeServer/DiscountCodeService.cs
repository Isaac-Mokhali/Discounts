using System;
using System.Collections.Generic;
using System.Text;

public class DiscountCodeService
{
    private readonly CodeStorage _storage;
    private readonly Random _random = new();

    public DiscountCodeService(CodeStorage storage)
    {
        _storage = storage;
    }

    public List<string> GenerateCodes(int count)
    {
        if (count > 2000)
            throw new ArgumentException("Cannot generate more than 2000 codes at once.");

        var codes = new List<string>();

        while (codes.Count < count)
        {
            string code = GenerateCode();
            if (!_storage.CodeExists(code) && !codes.Contains(code))
            {
                codes.Add(code);
            }
        }

        _storage.SaveCodes(codes);
        return codes;
    }

    private string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        int length = _random.Next(7, 9); // 7-8 chars

        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
            sb.Append(chars[_random.Next(chars.Length)]);

        return sb.ToString();
    }

    public bool UseCode(string code)
    {
        return _storage.UseCode(code);
    }
}
