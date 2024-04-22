using System.Collections.Concurrent;

internal class FragmentReader
{
    public static Dictionary<int, FragmentedMessage> fragments = [];

    public static FragmentedMessage? Read(BigEndianReader p)
    {
        int startSequenceNumber = p.ReadInt32();
        int fragmentCount = p.ReadInt32();
        int fragmentNumber = p.ReadInt32();
        int totalLength = p.ReadInt32();
        int fragmentOffset = p.ReadInt32();

        FragmentedMessage message;

        if (!fragments.TryGetValue(startSequenceNumber, out message))
        {
            message = new(totalLength, fragmentCount);
            fragments.Add(startSequenceNumber, message);
        }

        message.Write(p, fragmentOffset, fragmentNumber);

        if (message.Finished)
        {
            fragments.Remove(startSequenceNumber);
            return message;
        }

        return null;
    }
}