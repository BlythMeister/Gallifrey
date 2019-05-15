<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

string path = @"F:\GIT\Gallifrey\.premiumAccess\PremiumInstanceIds.dat";
string passphrase = "";

void Main()
{
	if (string.IsNullOrWhiteSpace(passphrase))
	{
		Console.WriteLine("Encryption Passphrase");
		passphrase = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(passphrase))
		{
			Console.WriteLine("Passphrase is required!");
			return;
		}
	}

	var running = true;
	while (running)
	{
		Console.WriteLine("Enter Command (Manual Edit/Show/Add/Remove/Replace/Done/Site Hash)");
		var command = Console.ReadLine();
		Util.ClearResults();
		switch (command.ToLower())
		{
			case "site hash":
				CalculateSiteHash();
				break;
			case "manual edit":
				DecryptFile();
				Process.Start(path);
				Console.WriteLine("Press Enter To Encrypt");
				Console.ReadLine();
				EncryptFile();
				break;
			case "show":
				ShowFileContent();
				break;
			case "add":
				DoAdd();
				break;
			case "remove":
				DoRemove();
				break;
			case "replace":
				DoReplace();
				break;
			case "done":
				running = false;
				break;
			default:
				Console.WriteLine("Unknown Function {0}", command);
				break;
		}
		Console.WriteLine("");
		Console.WriteLine("");
	}

}

void CalculateSiteHash()
{
	Console.WriteLine("Enter Site URL");
	var text = Console.ReadLine();
	if (!text.StartsWith("http")) text = "http://" + text;
	var hostName = new Uri(text).Host;
	Console.WriteLine($"HostName: {hostName}");
	var bytes = Encoding.UTF8.GetBytes(hostName.ToLower());
	var hashstring = new SHA256Managed();
	var hash = hashstring.ComputeHash(bytes);
	Console.WriteLine(hash.Aggregate(string.Empty, (current, x) => current + $"{x:x2}"));
}

private void ShowFileContent()
{
	DecryptFile();
	Console.WriteLine(File.ReadAllText(path));
	EncryptFile();
}

private void DoAdd()
{
	Console.WriteLine("Enter InstanceId To Add");
	var instanceId = Console.ReadLine();
	Console.WriteLine("Enter Details (Name/Reason)");
	var detail = Console.ReadLine();
	var infoToAdd = string.Format("{0} - {1}", instanceId, detail);
	DecryptFile();
	var lines = File.ReadAllLines(path).ToList();
	lines.Add(infoToAdd);
	File.WriteAllLines(path, lines);
	EncryptFile();
	Console.WriteLine("Added: {0}", infoToAdd);
}

private void DoRemove()
{
	Console.WriteLine("Enter InstanceId To Remove");
	var instanceId = Console.ReadLine();
	DecryptFile();
	var lines = new List<string>();
	var removed = false;
	foreach (string line in File.ReadAllLines(path))
	{
		if (!line.StartsWith(instanceId))
		{
			lines.Add(line);
		}
		else
		{
			removed = true;
		}
	}
	File.WriteAllLines(path, lines);
	EncryptFile();
	if (removed)
	{
		Console.WriteLine("Removed: {0}", instanceId);
	}
	else
	{
		Console.WriteLine("Unable to locate: {0}", instanceId);
	}
}

private void DoReplace()
{
	Console.WriteLine("Enter OLD InstanceId To Remove");
	var oldInstanceId = Console.ReadLine();
	Console.WriteLine("Enter NEW InstanceId To Add");
	var newInstanceId = Console.ReadLine();
	DecryptFile();
	var lines = new List<string>();
	var swapped = false;
	foreach (string line in File.ReadAllLines(path))
	{
		if (!line.StartsWith(oldInstanceId))
		{
			lines.Add(line);
		}
		else
		{
			swapped = true;
			lines.Add(line.Replace(oldInstanceId, newInstanceId));
		}
	}

	File.WriteAllLines(path, lines);
	EncryptFile();

	if (swapped)
	{
		Console.WriteLine("Replaced: {0} With: {1}", oldInstanceId, newInstanceId);
	}
	else
	{
		Console.WriteLine("Unable to locate old id: {0}", oldInstanceId);
	}

}

private void DecryptFile()
{
	File.WriteAllText(path, DataEncryption.Decrypt(File.ReadAllText(path), passphrase));
}

private void EncryptFile()
{
	File.WriteAllText(path, DataEncryption.EncryptCaseSensitive(File.ReadAllText(path), passphrase));
}

internal static class DataEncryption
    {
        internal static string EncryptCaseInsensitive(string plainText, string passPhrase)
        {
            var vector = GetSha256Hash(Guid.NewGuid().ToString()).Substring(0, 16).Replace("|", "~");
            var encrypted = Encrypt(plainText, passPhrase.ToLower(), vector);
		return $"V2|CI|{vector}|{encrypted}";
	}

	internal static string EncryptCaseSensitive(string plainText, string passPhrase)
	{
		var vector = GetSha256Hash(Guid.NewGuid().ToString()).Substring(0, 16).Replace("|", "~");
		var encrypted = Encrypt(plainText, passPhrase, vector);
		return $"V2|CS|{vector}|{encrypted}";
	}

	internal static string Decrypt(string cipherText, string passPhrase)
        {
            var cipherParts = cipherText.Split(new[] { '|' }, 2);

            if (cipherParts[0] == "V1")
            {
                cipherParts = cipherText.Split(new[] { '|' }, 3);
                return Decrypt(cipherParts[2], passPhrase, cipherParts[1]);
            }

            if (cipherParts[0] == "V2")
            {
                cipherParts = cipherText.Split(new[] { '|' }, 4);

                if (cipherParts[1] == "CI")
                {
                    return Decrypt(cipherParts[3], passPhrase.ToLower(), cipherParts[2]);
                }

                if (cipherParts[1] == "CS")
                {
                    return Decrypt(cipherParts[3], passPhrase, cipherParts[2]);
                }
            }

            return string.Empty;
        }

        private static string Encrypt(string plainText, string passPhrase, string vector)
        {
            var initVectorBytes = Encoding.UTF8.GetBytes(vector);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var keyBytes = new Rfc2898DeriveBytes(passPhrase, initVectorBytes).GetBytes(32);
            var encryptor = new AesCryptoServiceProvider().CreateEncryptor(keyBytes, initVectorBytes);

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    var cipherTextBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(cipherTextBytes);
                }
            }
        }

        private static string Decrypt(string cipherText, string passPhrase, string vector)
        {
            var initVectorBytes = Encoding.ASCII.GetBytes(vector);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var keyBytes = new Rfc2898DeriveBytes(passPhrase, initVectorBytes).GetBytes(32);
            var decryptor = new AesCryptoServiceProvider().CreateDecryptor(keyBytes, initVectorBytes);

            using (var memoryStream = new MemoryStream(cipherTextBytes))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    var plainTextBytes = new byte[cipherTextBytes.Length];
                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                }
            }
        }

	internal static string GetSha256Hash(string text)
	{
		var bytes = Encoding.UTF8.GetBytes(text);
		var hashstring = new SHA256Managed();
		var hash = hashstring.ComputeHash(bytes);
		return hash.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
	}
}