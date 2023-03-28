namespace Lantern.Discv5.WireProtocol.Packet.Headers;

public class MaskedHeader
{
    private const int MaskingKeyLength = 16;
    
    public MaskedHeader(byte[] destinationNodeId, byte[]? maskingIv = default)
    {
        if (destinationNodeId.Length < MaskingKeyLength)
        {
            throw new ArgumentException($"destinationNodeId must be at least {MaskingKeyLength} bytes long.", nameof(destinationNodeId));
        }
        
        MaskingKey = destinationNodeId[..MaskingKeyLength];
        MaskingIv = maskingIv ?? new byte[MaskingKeyLength];
    }
    
    public byte[] MaskingKey { get; }
    
    public byte[] MaskingIv { get; }

    public byte[] GetMaskedHeader(byte[] header) => AesCryptography.AesCtrEncrypt(MaskingKey, MaskingIv, header);
}