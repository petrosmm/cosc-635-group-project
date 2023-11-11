using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Lib
{
    public static class Lib
    {
        public static int PORT_SERVER = 3000;
        public static int PORT_CLIENT = 49999;
        public static int WINDOW_SIZE = 4;


    }

    public enum Type {
        none = 0,
        send = 1,
        receive = 2,
    }

    public class Frame
    {
        public Type Type { get; set; }

        public string Body { get; set; }

        public int Sequence { get; set; }
        public bool IsLast { get; set; }

        public override string ToString()
        {
            return $"{Type.ToString()}|{Body}";
        }
    }

    public static class Helpers
    {
        public static string GetTextFromBytes(byte[] input)
        {
            var result = ASCIIEncoding.UTF8.GetString(input);
            return result;
        }

        public static byte[] GetBytesFromText(string input)
        {
            var result = Encoding.ASCII.GetBytes(input);
            return result;
        }

        public static byte[] GetAsBytes(this Frame input)
        {
            var s = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(input));
            return s;
        }

        public static Frame GetFrame(this byte[] input)
        {
            var s = JsonSerializer.Deserialize<Frame>(GetTextFromBytes(input));
            return s;
        }
    }
}
