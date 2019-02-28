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
        private readonly string serialisationErrorDirectory;
        private readonly string encryptionPassPhrase;
        private string TempWritePath => savePath + ".temp";
        private string BackupPath => savePath + ".bak";
        private string BackupPathPlus1 => savePath + ".bak+1";
        private readonly Mutex singleThreadMutex;

        public ItemSerializer(string fileName)
        {
            singleThreadMutex = new Mutex(false, fileName);
            var saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallifrey");

            encryptionPassPhrase = $@"{Environment.UserDomainName}\{Environment.UserName}";
            serialisationErrorDirectory = Path.Combine(saveDirectory, "Errors");
            savePath = Path.Combine(saveDirectory, fileName);

            try
            {
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
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
