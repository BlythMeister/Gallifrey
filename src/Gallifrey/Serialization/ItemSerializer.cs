using Gallifrey.Exceptions.Serialization;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;

namespace Gallifrey.Serialization
{
    public class ItemSerializer<T> where T : new()
    {
        private readonly string savePath;
        private readonly string backupPath;
        private readonly string serialisationErrorDirectory;
        private readonly string encryptionPassPhrase;
        private string TempWritePath => backupPath + ".temp.bak";
        private string BackupPath => backupPath + ".bak";
        private string BackupPathPlus1 => backupPath + ".1.bak";
        private string BackupPathPlus2 => backupPath + ".2.bak";
        private string BackupPathPlus3 => backupPath + ".3.bak";
        private string BackupPathPlus4 => backupPath + ".4.bak";
        private readonly Mutex singleThreadMutex;

        public ItemSerializer(string fileName)
        {
            singleThreadMutex = new Mutex(false, fileName);
            var saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallifrey");
            var backupDirectory = Path.Combine(saveDirectory, "Backup");

            encryptionPassPhrase = $@"{Environment.UserDomainName}\{Environment.UserName}";
            serialisationErrorDirectory = Path.Combine(saveDirectory, "SerializationErrors");
            savePath = Path.Combine(saveDirectory, fileName);
            backupPath = Path.Combine(backupDirectory, fileName);

            try
            {
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
                if (!Directory.Exists(backupDirectory)) Directory.CreateDirectory(backupDirectory);
                if (!Directory.Exists(serialisationErrorDirectory)) Directory.CreateDirectory(serialisationErrorDirectory);
            }
            catch (Exception)
            {
                savePath = null;
                // this will be evaluated later
            }
        }

        public void Serialize(T obj)
        {
            Serialize(obj, 0);
        }

        private void Serialize(T obj, int retryCount)
        {
            if (savePath == null) throw new SerializerError("No Save Path");

            singleThreadMutex.WaitOne();

            try
            {
                if (File.Exists(savePath))
                {
                    if (!File.Exists(TempWritePath))
                    {
                        File.Copy(savePath, TempWritePath, true);
                    }

                    if (!File.Exists(BackupPath) || new FileInfo(BackupPath).LastWriteTimeUtc.Date < DateTime.UtcNow.Date)
                    {
                        File.Copy(savePath, BackupPath, true);
                    }

                    if (!File.Exists(BackupPathPlus1) || new FileInfo(BackupPathPlus1).LastWriteTimeUtc.Date < DateTime.UtcNow.Date.AddDays(-1))
                    {
                        File.Copy(savePath, BackupPathPlus1, true);
                    }

                    if (!File.Exists(BackupPathPlus2) || new FileInfo(BackupPathPlus2).LastWriteTimeUtc.Date < DateTime.UtcNow.Date.AddDays(-2))
                    {
                        File.Copy(savePath, BackupPathPlus2, true);
                    }

                    if (!File.Exists(BackupPathPlus3) || new FileInfo(BackupPathPlus3).LastWriteTimeUtc.Date < DateTime.UtcNow.Date.AddDays(-3))
                    {
                        File.Copy(savePath, BackupPathPlus3, true);
                    }

                    if (!File.Exists(BackupPathPlus4) || new FileInfo(BackupPathPlus4).LastWriteTimeUtc.Date < DateTime.UtcNow.Date.AddDays(-4))
                    {
                        File.Copy(savePath, BackupPathPlus4, true);
                    }
                }
                File.WriteAllText(savePath, DataEncryption.EncryptCaseInsensitive(JsonConvert.SerializeObject(obj), encryptionPassPhrase));
            }
            catch (Exception ex)
            {
                if (retryCount >= 3)
                {
                    if (File.Exists(TempWritePath)) File.Copy(TempWritePath, savePath, true);
                    File.WriteAllText(Path.Combine(serialisationErrorDirectory, $"Serialize_{DateTime.UtcNow:yyyy-MM-dd_hh-mm-ss}_{Guid.NewGuid().ToString()}.log"), ex.ToString());
                    throw new SerializerError("Error in serialization", ex);
                }
                Thread.Sleep(500);
                Serialize(obj, retryCount + 1);
            }
            finally
            {
                try
                {
                    if (File.Exists(TempWritePath))
                    {
                        File.Delete(TempWritePath);
                    }
                }
                catch (Exception)
                {
                    //ignored
                }

                singleThreadMutex.ReleaseMutex();
            }
        }

        public T DeSerialize()
        {
            return DeSerializeFile(savePath, 0);
        }

        public T DeSerialize(string encryptedString)
        {
            return DeSerializeText(encryptedString, 0);
        }

        private T DeSerializeFile(string fileToUse, int retryCount)
        {
            if (fileToUse == null) throw new SerializerError("No Save Path");

            if (!File.Exists(fileToUse))
            {
                return new T();
            }

            singleThreadMutex.WaitOne();

            try
            {
                var encryptedString = File.ReadAllText(fileToUse);
                var text = DataEncryption.Decrypt(encryptedString, encryptionPassPhrase);
                return JsonConvert.DeserializeObject<T>(text);
            }
            catch (Exception ex)
            {
                if (retryCount >= 3 && fileToUse == savePath)
                {
                    Thread.Sleep(200);
                    return DeSerializeFile(BackupPath, 0);
                }

                if (retryCount >= 3 && fileToUse == BackupPath)
                {
                    Thread.Sleep(200);
                    return DeSerializeFile(BackupPathPlus1, 0);
                }

                if (retryCount >= 3 && fileToUse == BackupPathPlus1)
                {
                    Thread.Sleep(200);
                    return DeSerializeFile(BackupPathPlus2, 0);
                }

                if (retryCount >= 3 && fileToUse == BackupPathPlus2)
                {
                    Thread.Sleep(200);
                    return DeSerializeFile(BackupPathPlus3, 0);
                }

                if (retryCount >= 3 && fileToUse == BackupPathPlus3)
                {
                    Thread.Sleep(200);
                    return DeSerializeFile(BackupPathPlus4, 0);
                }

                if (retryCount >= 3)
                {
                    File.WriteAllText(Path.Combine(serialisationErrorDirectory, $"DeSerializeFile_{DateTime.UtcNow:yyyy-MM-dd_hh-mm-ss}_{Guid.NewGuid().ToString()}.log"), ex.ToString());
                    return new T();
                }

                Thread.Sleep(200);
                return DeSerializeFile(fileToUse, retryCount + 1);
            }
            finally
            {
                singleThreadMutex.ReleaseMutex();
            }
        }

        private T DeSerializeText(string encryptedString, int retryCount)
        {
            try
            {
                var text = DataEncryption.Decrypt(encryptedString, encryptionPassPhrase);
                return JsonConvert.DeserializeObject<T>(text);
            }
            catch (Exception ex)
            {
                if (retryCount >= 3)
                {
                    File.WriteAllText(Path.Combine(serialisationErrorDirectory, $"DeSerializeText_{DateTime.UtcNow:yyyy-MM-dd_hh-mm-ss}_{Guid.NewGuid().ToString()}.log"), ex.ToString());
                    return new T();
                }

                Thread.Sleep(100);
                return DeSerializeText(encryptedString, retryCount + 1);
            }
        }
    }
}
