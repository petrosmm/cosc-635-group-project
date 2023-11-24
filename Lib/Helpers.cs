using System;
using System.Collections.Generic;
using System.Data;
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
        static int OS = (int)Environment.OSVersion.Platform;
        public static int WINDOW_SIZE_INTERNAL = 100; 
        public static int WINDOW_SIZE = (OS == 4) || (OS == 128) ? Math.Min(WINDOW_SIZE_INTERNAL, 80) : WINDOW_SIZE_INTERNAL;
        public static int WINDOW_SIZE_SEND = 
            WINDOW_SIZE - 1 <= 0 
            ? 1 : WINDOW_SIZE - 1;
        public static int WINDOW_SIZE_PADDING = 2;

        public static TimeSpan GetTimeSpanMs(int ms)
        {
            return new TimeSpan(0, 0, 0, 0, 500);
        }


        public static TimeSpan GetTimeSpanSeconds(int s = 1)
        {
            return new TimeSpan(0, 0, 0, s);
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
        request = 3,
        special= 4
    }

    public class Frame
    {
        public Type Type { get; set; }

        public string Body { get; set; }

        public int Sequence { get; set; }

        public bool IsLast { get; set; }

        public string ToStringAlt()
        {
            return $"{Type.ToString()}|{Sequence}|{(Body.Count() < 20 ? Body : Body.Substring(0, 20) + "...")}" 
                ?? $"{Type.ToString()}|{Sequence}";
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

        public static string ToStringAlt(this List<Frame> input)
        {
            var frameStart = input.FirstOrDefault();

            var frameEnd = input.LastOrDefault();

            return $"frame {frameStart.Sequence} to frame {frameEnd.Sequence}";
        }

        public static int GetSequenceNumberTrailer(this List<Frame> input)
        {
            var frameEnd = input.LastOrDefault();

            return frameEnd.Sequence;
        }

        public static byte[] GetAsBytes(this List<Frame> input)
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

        private static int GetZeroIfFloor(int start)
        {
            return start < 0 ? 0 : start;
        }

        public static int HasIssueWithPriors(this List<Frame> framesCurrent, List<Frame> framesIncoming = null)
        {
            var framesCurrentCount = framesCurrent.Count();
            var frameRefStart = framesIncoming.FirstOrDefault();
            var frameRefEnd = framesIncoming.LastOrDefault();
            var sequenceStart = frameRefStart.Sequence;
            if (sequenceStart > 0)
            {
                var start = 0;
                var windowRelative = 0;

                if (sequenceStart <= WINDOW_SIZE)
                {
                    start = 0;
                    windowRelative = WINDOW_SIZE - 1;
                }
                else
                {
                    if (framesCurrentCount <= WINDOW_SIZE * 3)
                    {
                        start = GetZeroIfFloor((sequenceStart - WINDOW_SIZE) - WINDOW_SIZE_PADDING);
                        windowRelative = sequenceStart - 1;
                        if (windowRelative < 0)
                        {
                            Debugger.Break();
                        }
                    }
                    else
                    {
                        start = GetZeroIfFloor((sequenceStart - WINDOW_SIZE * 3) - WINDOW_SIZE_PADDING);
                        windowRelative = sequenceStart - 1;
                        if (windowRelative < 0)
                        {
                            Debugger.Break();
                        }
                    }
                 
                }

                if (windowRelative > -1)
                {
                    //  for (int i = GetZeroIfFloor(start - WINDOW_SIZE); i < windowRelative; i++)
                    for (int i = start; i < windowRelative; i++)
                    {
                        var item = framesCurrent.FirstOrDefault(p => p.Sequence == i);
                        if (item == null)
                        {
                            // check if incoming frame is exempt
                            
                            if (frameRefStart.Type == Type.send && frameRefStart.Sequence == i)
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
