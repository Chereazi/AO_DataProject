using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MarketProject.ENetHeaders
{
    internal class ENetProtocolHeader //12
    {
        internal ushort peerID;
        internal byte crcEnabled;
        internal byte commandCount;
        internal uint sentTime;
        internal uint challenge;

        public ENetProtocolHeader(BigEndianReader p)
        {
            peerID = p.ReadUInt16();
            crcEnabled = p.ReadByte();
            commandCount = p.ReadByte();
            sentTime = p.ReadUInt32();
            challenge = p.ReadUInt32();
        }
    }

    internal class ENetProtocolCommandHeader //12
    {
        internal ENetProtocolCommand command;
        internal byte channelID;
        internal ENetPacketFlag flags;
        internal byte unknown;
        internal uint commandLength;
        internal uint reliableSequenceNumber;

        internal ENetProtocolCommandHeader(BigEndianReader p)
        {
            command = (ENetProtocolCommand)p.ReadByte();
            channelID = p.ReadByte();
            flags = (ENetPacketFlag)p.ReadByte();
            unknown = p.ReadByte();
            commandLength = p.ReadUInt32();
            reliableSequenceNumber = p.ReadUInt32();
        }
    }

    //internal class ENetProtocolAcknowledge
    //{
    //    ENetProtocolCommandHeader header;
    //    ushort receivedReliableSequenceNumber;
    //    ushort receivedSentTime;
    //}

    //internal class ENetProtocolConnect
    //{
    //    ENetProtocolCommandHeader header;
    //    ushort outgoingPeerID;
    //    byte incomingSessionID;
    //    byte outgoingSessionID;
    //    uint mtu;
    //    uint windowSize;
    //    uint channelCount;
    //    uint incomingBandwidth;
    //    uint outgoingBandwidth;
    //    uint packetThrottleInterval;
    //    uint packetThrottleAcceleration;
    //    uint packetThrottleDeceleration;
    //    uint connectID;
    //    uint data;
    //}

    //internal class ENetProtocolVerifyConnect
    //{
    //    ENetProtocolCommandHeader header;
    //    ushort outgoingPeerID;
    //    byte incomingSessionID;
    //    byte outgoingSessionID;
    //    uint mtu;
    //    uint windowSize;
    //    uint channelCount;
    //    uint incomingBandwidth;
    //    uint outgoingBandwidth;
    //    uint packetThrottleInterval;
    //    uint packetThrottleAcceleration;
    //    uint packetThrottleDeceleration;
    //    uint connectID;
    //}

    //internal class ENetProtocolBandwidthLimit
    //{
    //    ENetProtocolCommandHeader header;
    //    uint incomingBandwidth;
    //    uint outgoingBandwidth;
    //}

    //internal class ENetProtocolThrottleConfigure
    //{
    //    ENetProtocolCommandHeader header;
    //    uint packetThrottleInterval;
    //    uint packetThrottleAcceleration;
    //    uint packetThrottleDeceleration;
    //}

    //internal class ENetProtocolDisconnect
    //{
    //    ENetProtocolCommandHeader header;
    //    uint data;
    //}

    //internal class ENetProtocolPing
    //{
    //    ENetProtocolCommandHeader header;
    //}

    internal class ENetProtocolSendUnreliable
    {
        internal ushort data;
        internal ushort sequenceNumber;

        internal ENetProtocolSendUnreliable(BigEndianReader p)
        {
            data = p.ReadUInt16();
            sequenceNumber = p.ReadUInt16();
        }
    }

    internal class ENetProtocolSendUnsequenced
    {
        ENetProtocolCommandHeader header;
        ushort unsequencedGroup;
        ushort dataLength;
    }

    internal class ENetProtocolSendFragment
    {
        ENetProtocolCommandHeader header;
        ushort startSequenceNumber;
        ushort dataLength;
        uint fragmentCount;
        uint fragmentNumber;
        uint totalLength;
        uint fragmentOffset;
    }

    enum ENetProtocolCommand
    {
        ENET_PROTOCOL_COMMAND_NONE = 0,
        ENET_PROTOCOL_COMMAND_ACKNOWLEDGE = 1,
        ENET_PROTOCOL_COMMAND_CONNECT = 2,
        ENET_PROTOCOL_COMMAND_VERIFY_CONNECT = 3,
        ENET_PROTOCOL_COMMAND_DISCONNECT = 4,
        ENET_PROTOCOL_COMMAND_PING = 5,
        ENET_PROTOCOL_COMMAND_SEND_RELIABLE = 6,
        ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE = 7,
        ENET_PROTOCOL_COMMAND_SEND_FRAGMENT = 8,
        ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED = 9,
        ENET_PROTOCOL_COMMAND_BANDWIDTH_LIMIT = 10,
        ENET_PROTOCOL_COMMAND_THROTTLE_CONFIGURE = 11,
        ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE_FRAGMENT = 12,
        ENET_PROTOCOL_COMMAND_COUNT = 13,
        ENET_PROTOCOL_COMMAND_MASK = 0x0F
    }

    [Flags]
    enum ENetProtocolFlag
    {
        ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE = (1 << 7),
        ENET_PROTOCOL_COMMAND_FLAG_UNSEQUENCED = (1 << 6),
        ENET_PROTOCOL_HEADER_FLAG_COMPRESSED = (1 << 14),
        ENET_PROTOCOL_HEADER_FLAG_SENT_TIME = (1 << 15),
        ENET_PROTOCOL_HEADER_FLAG_MASK = ENET_PROTOCOL_HEADER_FLAG_COMPRESSED | ENET_PROTOCOL_HEADER_FLAG_SENT_TIME,
        ENET_PROTOCOL_HEADER_SESSION_MASK = (3 << 12),
        ENET_PROTOCOL_HEADER_SESSION_SHIFT = 12
    }

    [Flags]
    enum ENetPacketFlag
    {
        ENET_PACKET_FLAG_NONE = 0,
        ENET_PACKET_FLAG_RELIABLE = (1 << 0),
        ENET_PACKET_FLAG_UNSEQUENCED = (1 << 1),
        ENET_PACKET_FLAG_NO_ALLOCATE = (1 << 2),
        ENET_PACKET_FLAG_UNRELIABLE_FRAGMENTED = (1 << 3),
        ENET_PACKET_FLAG_INSTANT = (1 << 4),
        ENET_PACKET_FLAG_UNTHROTTLED = (1 << 5),
        ENET_PACKET_FLAG_SENT = (1 << 8)
    }

    enum MessageType
    {
        Request = 2,
        Response = 3,
        Event = 4
    }
}
