using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using proto;

namespace ConsoleApplication3
{
    class Program
    {
        private TcpClient hyperionSocket = new TcpClient();
        private Stream hyperionStream;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.HyperionTestMethod();
        }
        private void HyperionTestMethod()
        {
            HyperionChangeColor(255, 255, 255);
            Console.Read();
        }

        private void HyperionChangeColor(int red, int green, int blue)
        {
            ColorRequest colorRequest = ColorRequest.CreateBuilder()
                .SetRgbColor((red * 256 * 256) + (green * 256) + blue)
                .SetPriority(1)
                .SetDuration(-1)
                .Build();

            HyperionRequest request = HyperionRequest.CreateBuilder()
                .SetCommand(HyperionRequest.Types.Command.COLOR)
                .SetExtension(ColorRequest.ColorRequest_, colorRequest)
                .Build();
            HyperionSendRequest(request);
        }

        private void HyperionSendRequest(HyperionRequest request)
        {
            int size = request.SerializedSize;

            Byte[] header = new byte[4];
            header[0] = (byte)((size >> 24) & 0xFF);
            header[1] = (byte)((size >> 16) & 0xFF);
            header[2] = (byte)((size >> 8) & 0xFF);
            header[3] = (byte)((size) & 0xFF);

            hyperionSocket.Connect("10.1.2.77", 19445);
            hyperionStream = hyperionSocket.GetStream();
            Console.WriteLine("CONNECTED!");

            StreamWriter sw = new StreamWriter(hyperionStream);
            sw.Write(header);
            hyperionStream.Write(header, 0, header.Count());
            request.WriteTo(hyperionStream);
            Console.WriteLine("WRITTEN DATA!");

            hyperionStream.Flush();
            Console.WriteLine("FLUSHED DATA!");

            HyperionReply reply = receiveReply();
            Console.WriteLine("REPLY: " + reply);

        }
        private HyperionReply receiveReply()
        {
            Stream input = hyperionSocket.GetStream();
            byte[] header = new byte[4];
            input.Read(header, 0, 4);
            int size = (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | (header[3]);
            byte[] data = new byte[size];
            input.Read(data, 0, size);
            HyperionReply reply = HyperionReply.ParseFrom(data);
            return reply;
        }
    }
}
