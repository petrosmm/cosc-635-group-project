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
        public static int PORT_SERVER = 3000;
        public static int PORT_CLIENT = 49999;
        public static int WINDOW_SIZE = 4;
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

        public static bool CheckIfMeetsCriteria(this List<Frame> input, int sequence = 1)
        {
            var result = false;
            var @continue = true;
            var sequenceRef = sequence;
            var windowRelative = sequence - WINDOW_SIZE;
            while (windowRelative > WINDOW_SIZE && @continue)
            {
                
            }

            return result;
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

                var startSize = sequence < WINDOW_SIZE
                    ? 0
                    : sequence;

                if (windowRelative > -1)
                {
                    for (int i = startSize; i < windowRelative; i++)
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
