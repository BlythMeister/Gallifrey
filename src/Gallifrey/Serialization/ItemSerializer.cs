using System;
using System.IO;
using Gallifrey.Exceptions.Serialization;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    public class ItemSerializer<T> where T : new()
    {
        private readonly string savePath;

        public ItemSerializer(string directory, string fileName)
        {
            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                savePath = Path.Combine(directory, fileName);
            }
            catch (Exception)
            {
                savePath = null;
                // this will be evaluated later
            }

        }

        public ItemSerializer(string fileName)
            : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallifrey"), fileName)
        {

        }

        public ItemSerializer()
        {

        }

        /// <exception cref="SerializerError">Something has gone wrong with serialization, message contain more information</exception>
        public void Serialize(T obj)
        {
            if (savePath == null) throw new SerializerError("No Save Path");

            try
            {
                File.WriteAllText(savePath, DataEncryption.Encrypt(JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                throw new SerializerError("Error in serialization", ex);
            }
        }

        /// <exception cref="SerializerError">Something has gone wrong with deserialization, message contain more information</exception>
        public T DeSerialize()
        {
            if (savePath == null) throw new SerializerError("No Save Path");

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