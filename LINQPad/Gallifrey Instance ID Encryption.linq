<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

string path = @"F:\GIT\Gallifrey.Releases\download\PremiumInstanceIds";

void Main()
{
	var running = true;
	while(running)
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
	if(!text.StartsWith("http")) text = "http://" + text;
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
	var infoToAdd = string.Format("{0} - {1}",instanceId,detail);
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
		if(!line.StartsWith(instanceId))
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
	if(removed)
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
		if(!line.StartsWith(oldInstanceId))
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
		
	if(swapped)
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
	File.WriteAllText(path, DataEncryption.Decrypt(File.ReadAllText(path)));
}

private void EncryptFile()
{
	File.WriteAllText(path, DataEncryption.Encrypt(File.ReadAllText(path)));
}

internal static class DataEncryption
    {
        //TODO: Maybe these shouldn't be stored in plain code?
        //TODO: Though, this is to make hacking the app settings a pain, rather than pure security.
        private const string PassPhrase = "WOq2kKSbvHTcKp9e";
        private const string InitVector = "pId6i1bN1aCVTaHN";
        private const int Keysize = 256;

        internal static string Encrypt(string plainText)
        {
            var initVectorBytes = Encoding.UTF8.GetBytes(InitVector);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new PasswordDeriveBytes(PassPhrase, null);
            var keyBytes = password.GetBytes(Keysize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

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

        internal static string Decrypt(string cipherText)
        {
            var initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var password = new PasswordDeriveBytes(PassPhrase, null);
            var keyBytes = password.GetBytes(Keysize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

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
    }