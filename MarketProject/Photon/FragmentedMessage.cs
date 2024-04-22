
internal class FragmentedMessage
{

    internal byte[] data;
    bool[] received;

    public FragmentedMessage(int totalLength, int fragmentCount)
    {
        data = new byte[totalLength];
        received = new bool[fragmentCount];
    }

    internal void Write(BigEndianReader p, int fragmentOffset, int fragmentNumber)
    {
        p.BaseStream.Read(data, fragmentOffset, (int)(p.BaseStream.Length - p.BaseStream.Position));
        received[fragmentNumber] = true;
    }

    internal bool Finished
    {
        get
        {
            return received.All(x => x == true);
        }
    }
}