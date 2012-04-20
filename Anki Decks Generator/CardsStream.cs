using System.IO;
using System.Text;

namespace deckgen
{
    public class CardsStream
    {
        int size;
        MemoryStream input;
        UTF8Encoding uniEncoding = new UTF8Encoding();
        FileStream output;

        public CardsStream(string outputPath, int size)
        {
            this.size = size;
            input = new MemoryStream(size);
            output = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public void Write(string str)
        {
            byte[] outputBytes = uniEncoding.GetBytes(str);
            if (input.Position + outputBytes.Length > size)
            {
                Save();
                input = new MemoryStream(size);
            }
            input.Write(outputBytes, 0, outputBytes.Length);
        }

        public void Save()
        {
            var end = input.Position;
            input.Seek(0, SeekOrigin.Begin);
            var buf = new byte[10000];
            while (input.Position < end)
            {
                output.Write(buf, 0, input.Read(buf, 0, 10000));
            }
        }

        ~CardsStream()
        {
            input.Close();
            output.Close();
        }
    }
}
