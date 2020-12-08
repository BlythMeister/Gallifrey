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
        private readonly string errorDirectory;
        private readonly string encryptionPassPhrase;
        private readonly Mutex singleThreadMutex;

        public ItemSerializer(string fileName)
        {
            singleThreadMutex = new Mutex(false, fileName);
            var baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallifrey");
            var saveDirectory = Path.Combine(baseDirectory, "Data");
            var backupDirectory = Path.Combine(baseDirectory, "Backup");

            encryptionPassPhrase = $@"{Environment.UserDomainName}\{Environment.UserName}";
            errorDirectory = Path.Combine(baseDirectory, "SerializationErrors");
            savePath = Path.Combine(saveDirectory, fileName);
            backupPath = Path.Combine(backupDirectory, fileName);

            try
            {
                if (!Directory.Exists(baseDirectory))
                {
                    Directory.CreateDirectory(baseDirectory);
                }

                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                if (!Directory.Exists(errorDirectory))
                {
                    Directory.CreateDirectory(errorDirectory);
                }
            }
            catch (Exception)
            {
                savePath = null;
                // this will be evaluated later
            }

            if (savePath != null)
            {
                var legacySavePath = Path.Combine(baseDirectory, fileName);
                if (File.Exists(legacySavePath))
                {
                    File.Move(legacySavePath, savePath);
                }
            }
        }

        public void Serialize(T obj)
        {
            Serialize(obj, 0);
        }

        private void Serialize(T obj, int retryCount)
        {
            if (savePath == null)
            {
                throw new SerializerError("No Save Path");
            }

            singleThreadMutex.WaitOne();
            var tempWritePath = $"{backupPath}.temp.bak";
            var mainBackupPath = $"{backupPath}.bak";

            try
            {
                if (File.Exists(savePath))
                {
                    if (!File.Exists(tempWritePath))
                    {
                        File.Copy(savePath, tempWritePath, true);
                    }

                    if (!File.Exists(mainBackupPath))
                    {
                        File.Copy(savePath, mainBackupPath, true);
                    }
                    else if (File.GetLastWriteTimeUtc(mainBackupPath) < DateTime.UtcNow.AddHours(-1))
                    {
                        var rollingBackupPath = $"{backupPath}.24.bak";
                        if (File.Exists(rollingBackupPath))
                        {
                            File.Delete(rollingBackupPath);
                        }
                        for (var i = 23; i > 0; i--)
                        {
                            rollingBackupPath = $"{backupPath}.{i}.bak";
                            if (File.Exists(rollingBackupPath))
                            {
                                File.Move(rollingBackupPath, $"{backupPath}.{i + 1}.bak");
                            }
                        }
                        File.Move(mainBackupPath, $"{backupPath}.1.bak");
                        File.Copy(savePath, mainBackupPath, true);
                    }
                }
                File.WriteAllText(savePath, DataEncryption.EncryptCaseInsensitive(JsonConvert.SerializeObject(obj), encryptionPassPhrase));
            }
            catch (Exception ex)
            {
                if (retryCount >= 3)
                {
                    if (File.Exists(tempWritePath))
                    {
                        File.Copy(tempWritePath, savePath, true);
                    }

                    File.WriteAllText(Path.Combine(errorDirectory, $"Serialize_{DateTime.UtcNow:yyyy-MM-dd_hh-mm-ss}_{Guid.NewGuid().ToString()}.log"), ex.ToString());
                    throw new SerializerError("Error in serialization", ex);
                }
                Thread.Sleep(500);
                Serialize(obj, retryCount + 1);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempWritePath))
                    {
                        File.Delete(tempWritePath);
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
            return DeSerialize(savePath, 0, 0);
        }

        private T DeSerialize(string fileToUse, int retryCount, int backupNumber)
        {
            if (fileToUse == null)
            {
                throw new SerializerError("No Save Path");
            }

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
                if (retryCount >= 3)
                {
                    File.WriteAllText(Path.Combine(errorDirectory, $"DeSerializeFile_{DateTime.UtcNow:yyyy-MM-dd_hh-mm-ss}_{Guid.NewGuid()}.log"), ex.ToString());
                    var nextFile = backupNumber == 0 ? $"{backupPath}.bak" : $"{backupPath}.{backupNumber}.bak";
                    return DeSerialize(nextFile, 0, backupNumber + 1);
                }

                Thread.Sleep(200);
                return DeSerialize(fileToUse, retryCount + 1, backupNumber);
            }
            finally
            {
                singleThreadMutex.ReleaseMutex();
            }
        }
    }
}
