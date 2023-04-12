using Lantern.Discv5.Rlp;
using Lantern.Discv5.WireProtocol.Messages.Responses;

namespace Lantern.Discv5.WireProtocol.Messages.Decoders;

public class TalkRespDecoder : IMessageDecoder<TalkRespMessage>
{
    public TalkRespMessage DecodeMessage(byte[] message)
    {
        var decodedMessage = RlpDecoder.Decode(message[1..]);
        return new TalkRespMessage(decodedMessage[0], decodedMessage[1]);
    }
}