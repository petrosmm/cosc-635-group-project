using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using static Lib.Lib;

namespace Lib
{
    public static class Lib
    {
        static Random random = new Random();
        public static int PORT_SERVER = 3000;
        public static int PORT_CLIENT = 49999;
        public static int WINDOW_SIZE = 4;

        public static TimeSpan GetTimeSpanMs(int ms)
        {
            return new TimeSpan(0, 0, 0, 0, 500);
        }

        public static int GenerateRandom(Random rng)
        {
            return rng.Next(0, 99);
        }  

        public static Random GetRandom()
        {
            return random;
        }
    }

    public enum Type {
        none = 0,
        send = 1,
        receive = 2,
        request = 3
    }

    public class Frame
    {
        public Type Type { get; set; }

        public string Body { get; set; }

        public int Sequence { get; set; }

        public bool IsLast { get; set; }

        public override string ToString()
        {
            return $"{Type.ToString()}|{Sequence}|{Body}";
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

        public static List<Frame> GetFrames(this byte[] input)
        {
            var s = JsonSerializer.Deserialize<List<Frame>>(GetTextFromBytes(input));
            return s;
        }

        public static bool IsZeroFrame(this Frame frame)
        {
            return frame.Sequence == 0;
        }

        public static int HasIssueWithPriors(this List<Frame> input, int sequence = 0, Frame frameRecieved = null)
        {
            var result = -1;

            if (sequence > 0)
            {
                var windowRelative = sequence < WINDOW_SIZE 
                    ? WINDOW_SIZE
                    : sequence - (WINDOW_SIZE * 2);

                if(sequence > input.Count())
                {

                }

                var start = 0;
                if (input.Count() < WINDOW_SIZE)
                {
                    start = 0;
                    windowRelative = WINDOW_SIZE * 2;
                }
                else
                {

                }

                if (windowRelative > -1)
                {
                    for (int i = start; i < windowRelative; i++)
                    {
                        var item = input.FirstOrDefault(p => p.Sequence == i);
                        if (item == null)
                        {
                            // check if incoming frame is exempt
                            if (frameRecieved.Type == Type.send && frameRecieved.Sequence == i)
                            {
                                return -1;
                            }

                            return i;
                        }
                    }
                }
            }
            else
            {
                return -1;
            }

            return -1;
        }
    }
}
