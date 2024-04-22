using MarketProject.ENetHeaders;
using MarketProject.Photon;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        // Create a raw socket for UDP packets
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Udp);

        try
        {
            IPAddress localIP;
            using (Socket st = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                st.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = st.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address;
            }

            // Bind the socket to the local endpoint
            socket.Bind(new IPEndPoint(localIP, 0)); // Change YOUR_LOCAL_IP_ADDRESS to your local IP address

            // Set the socket to receive all incoming packets
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            byte[] buffer = new byte[2 * 1024 * 1024];
            socket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), BitConverter.GetBytes(1));

            // Start receiving packets asynchronously
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), new StateObject(buffer, socket));

            // Keep the program running until the user presses a key
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            // Close the socket when done
            socket.Close();
        }
    }

    // State object for receiving data
    class StateObject
    {
        public byte[] Buffer { get; }
        public Socket Socket { get; }

        public StateObject(byte[] buffer, Socket socket)
        {
            Buffer = buffer;
            Socket = socket;
        }
    }

    // Callback function to handle received packets
    static void ReceiveCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;


        try
        {
            // Receive packet data
            int bytesRead = state.Socket.EndReceive(ar);

            byte[] buffer = state.Buffer.Take(bytesRead).ToArray();

            //for (int i = 0; i < bytesRead; ++i)
            //{
            //    if (buffer[i] == 0x41 && buffer[i + 1] == 0x6d && buffer[i + 2] == 0x6f && buffer[i + 3] == 0x75 && buffer[i + 4] == 0x6e && buffer[i + 5] == 0x74)
            //    {

            //    }
            //}

            //string packetData = string.Join('-', buffer);
            //Console.WriteLine($"full data: {packetData}");
            //packetData = BitConverter.ToString(buffer);
            //Console.WriteLine($"hex data: {packetData}");

            // Process the received packet

            using MemoryStream ms = new MemoryStream(buffer);
            BigEndianReader p = new BigEndianReader(new BinaryReader(ms), buffer);

            int ipv4len = (p.ReadByte() & 0xF) * 4;

            p.BaseStream.Position = ipv4len;
            //Console.WriteLine(ipv4len);

            byte[] sourceIP = new byte[4];
            byte[] destinationIP = new byte[4];

            sourceIP[0] = buffer[12];
            sourceIP[1] = buffer[13];
            sourceIP[2] = buffer[14];
            sourceIP[3] = buffer[15];

            destinationIP[0] = buffer[16];
            destinationIP[1] = buffer[17];
            destinationIP[2] = buffer[18];
            destinationIP[3] = buffer[19];


            int sourcePort = p.ReadUInt16();
            int destinationPort = p.ReadUInt16();
            p.ReadUInt16();
            p.ReadUInt16();

            //Console.WriteLine(sourcePort + "|" + destinationPort);

            if (((sourceIP[0]==193 && sourceIP[1]==169 && sourceIP[2]==238) || (destinationIP[0] == 193 && destinationIP[1] == 169 && destinationIP[2] == 238))
                && (sourcePort == 5056 || destinationPort == 5056))
            {

                ENetProtocolHeader header = new ENetProtocolHeader(p);

                for (int i = 0; i < header.commandCount; i++)
                {
                    ENetProtocolCommandHeader commandHeader = new ENetProtocolCommandHeader(p);

                    if (commandHeader.command == ENetProtocolCommand.ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE)
                    {
                        ENetProtocolSendUnreliable sendUnreliable = new ENetProtocolSendUnreliable(p);

                        PhotonReader pr = new(p);

                    }
                    else if (commandHeader.command == ENetProtocolCommand.ENET_PROTOCOL_COMMAND_SEND_RELIABLE)
                    {
                        PhotonReader pr = new(p);
                    }
                    else if (commandHeader.command == ENetProtocolCommand.ENET_PROTOCOL_COMMAND_SEND_FRAGMENT)
                    {
                        FragmentedMessage? fm = FragmentReader.Read(p);
                        if (fm != null)
                        {
                            MemoryStream fms = new MemoryStream(fm.data);
                            BigEndianReader fp = new BigEndianReader(new BinaryReader(fms), buffer);
                            PhotonReader pr = new(fp);
                        }
                    }

                    else
                    {
                        p.BaseStream.Position += commandHeader.commandLength - 12;
                    }
                }
            }
        }
        catch (Exception e)
        {
            //Console.WriteLine(e.ToString());
        }
        finally
        {
            state.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
        }
    }

    public static void Print<T>(T obj) where T : class
    {
        if (obj == null)
        {
            Console.WriteLine("null");
            return;
        }

        string output = "";
        Type t = obj.GetType(); // Where obj is object whose properties you need.
        var pi = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        output += t.Name + ": ";

        foreach (var p in pi)
        {
            if (p.FieldType != typeof(string) && p.FieldType.GetInterface(nameof(IEnumerable)) != null)
            {
                output += p.Name + ": ";
                foreach (object o in (IEnumerable)p.GetValue(obj))
                {
                    output += "[" + o.ToString() + "]";
                }
            }
            else
            {
                output += p.Name + ": " + p.GetValue(obj) + "    ";
            }
        }
        Console.WriteLine(output);
    }
}

public class BigEndianReader
{
    public BigEndianReader(BinaryReader baseReader, byte[] buffer)
    {
        mBaseReader = baseReader;
        this.buffer = buffer;
    }

    public short ReadInt16()
    {
        return BitConverter.ToInt16(ReadBigEndianBytes(2), 0);
    }

    public ushort ReadUInt16()
    {
        return BitConverter.ToUInt16(ReadBigEndianBytes(2), 0);
    }

    public uint ReadUInt32()
    {
        return BitConverter.ToUInt32(ReadBigEndianBytes(4), 0);
    }

    public byte[] ReadBigEndianBytes(int count)
    {
        byte[] bytes = new byte[count];
        for (int i = count - 1; i >= 0; i--)
            bytes[i] = mBaseReader.ReadByte();

        return bytes;
    }

    public byte[] ReadBytes(int count)
    {
        return mBaseReader.ReadBytes(count);
    }

    public void Close()
    {
        mBaseReader.Close();
    }

    internal byte ReadByte()
    {
        return mBaseReader.ReadByte();
    }

    internal double ReadDouble()
    {
        return BitConverter.ToDouble(ReadBigEndianBytes(8), 0);
    }

    internal float ReadFloat()
    {
        return BitConverter.ToSingle(ReadBigEndianBytes(4), 0);
    }

    internal int ReadInt32()
    {
        return BitConverter.ToInt32(ReadBigEndianBytes(4), 0);
    }

    internal long ReadInt64()
    {
        return BitConverter.ToInt64(ReadBigEndianBytes(8), 0);
    }

    internal byte[] Preview()
    {
        return buffer.Skip((int)mBaseReader.BaseStream.Position).Take(100).ToArray();
    }

    public Stream BaseStream
    {
        get { return mBaseReader.BaseStream; }
    }

    private BinaryReader mBaseReader;
    internal readonly byte[] buffer;
}