using System;
using System.IO;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    public class ItemSerializer<T> where T : new()
    {
        private readonly string savePath;
        
        public ItemSerializer(string directory, string fileName)
        {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            savePath = Path.Combine(directory, fileName);
        }
        
        public ItemSerializer(string fileName) : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallifrey"), fileName)
        {
            
        }

        public ItemSerializer() 
        {

        }

        public void Serialize(T obj)
        {
            if(savePath == null) throw new NotImplementedException();

            File.WriteAllText(savePath, DataEncryption.Encrypt(JsonConvert.SerializeObject(obj)));
        }

        public T DeSerialize()
        {
            if (savePath == null) throw new NotImplementedException();

            T obj;

            if (File.Exists(savePath))
            {
                try
                {
                    var text = File.ReadAllText(savePath);
                    obj = DeSerialize(text);
                }
                catch (Exception)
                {
                    obj = new T();
                }

            }
            else
            {
                obj = new T();
            }

            return obj;
        }

        public T DeSerialize(string encryptedString)
        {
            T obj;

            try
            {
                var text = DataEncryption.Decrypt(encryptedString);
                obj = JsonConvert.DeserializeObject<T>(text);
            }
            catch (Exception)
            {
                obj = new T();
            }

            return obj;
        }
    }
}