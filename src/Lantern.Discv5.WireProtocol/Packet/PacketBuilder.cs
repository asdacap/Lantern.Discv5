using Lantern.Discv5.Enr;
using Lantern.Discv5.Rlp;
using Lantern.Discv5.WireProtocol.Identity;
using Lantern.Discv5.WireProtocol.Message;
using Lantern.Discv5.WireProtocol.Packet.Headers;
using Lantern.Discv5.WireProtocol.Packet.Types;
using Lantern.Discv5.WireProtocol.Session;
using Lantern.Discv5.WireProtocol.Utility;
using Microsoft.Extensions.Logging;

namespace Lantern.Discv5.WireProtocol.Packet;

public class PacketBuilder(IIdentityManager identityManager, IAesCrypto aesCrypto, IRequestManager requestManager,
        ILoggerFactory loggerFactory)
    : IPacketBuilder
{
    private readonly ILogger<PacketBuilder> _logger = loggerFactory.CreateLogger<PacketBuilder>();

    public Tuple<byte[], StaticHeader> BuildRandomOrdinaryPacket(byte[] destNodeId)
    {
        var maskingIv = RandomUtility.GenerateRandomData(PacketConstants.MaskingIvSize);
        var packetNonce = RandomUtility.GenerateRandomData(PacketConstants.NonceSize);
        
        requestManager.AddCachedHandshakeInteraction(packetNonce, destNodeId);
        
        var ordinaryPacket = new OrdinaryPacketBase(identityManager.Record.NodeId);
        var packetStaticHeader = ConstructStaticHeader(PacketType.Ordinary, ordinaryPacket.AuthData, packetNonce);
        var maskedHeader = new MaskedHeader(destNodeId, maskingIv);
        var encryptedMaskedHeader = maskedHeader.GetMaskedHeader(packetStaticHeader.GetHeader(), aesCrypto);
        var randomData = RandomUtility.GenerateRandomData(PacketConstants.RandomDataSize);
        var packet = ByteArrayUtils.Concatenate(maskingIv, encryptedMaskedHeader, randomData);
        
        return Tuple.Create(packet, packetStaticHeader);
    }

    public Tuple<byte[], StaticHeader> BuildOrdinaryPacket(byte[] message, byte[] destNodeId, byte[] maskingIv, byte[] messageCount)
    {
        var ordinaryPacket = new OrdinaryPacketBase(identityManager.Record.NodeId);
        var packetNonce = ByteArrayUtils.JoinByteArrays(messageCount, RandomUtility.GenerateRandomData(PacketConstants.PartialNonceSize));

        _logger.LogInformation("Added cached request using nonce: {PacketNonce}", Convert.ToHexString(packetNonce));
        
        requestManager.AddCachedHandshakeInteraction(packetNonce, destNodeId);
        
        var packetStaticHeader = ConstructStaticHeader(PacketType.Ordinary, ordinaryPacket.AuthData, packetNonce);
        var maskedHeader = new MaskedHeader(destNodeId, maskingIv);
        var encryptedMaskedHeader = maskedHeader.GetMaskedHeader(packetStaticHeader.GetHeader(), aesCrypto);
        var packet = ByteArrayUtils.Concatenate(maskingIv, encryptedMaskedHeader);
        
        return Tuple.Create(packet, packetStaticHeader);
    }
    
    public Tuple<byte[], StaticHeader> BuildWhoAreYouPacketWithoutEnr(byte[] destNodeId, byte[] packetNonce, byte[] maskingIv)
    {
        var whoAreYouPacket = new WhoAreYouPacketBase(RandomUtility.GenerateRandomData(PacketConstants.IdNonceSize), 0);
        var packetStaticHeader = ConstructStaticHeader(PacketType.WhoAreYou, whoAreYouPacket.AuthData, packetNonce);
        var maskedHeader = new MaskedHeader(destNodeId, maskingIv);
        var encryptedMaskedHeader = maskedHeader.GetMaskedHeader(packetStaticHeader.GetHeader(), aesCrypto);
        var packet = ByteArrayUtils.JoinByteArrays(maskingIv, encryptedMaskedHeader);
         
        return Tuple.Create(packet, packetStaticHeader);
    }

    public Tuple<byte[], StaticHeader> BuildWhoAreYouPacket(byte[] destNodeId, byte[] packetNonce, IEnr dest, byte[] maskingIv)
    {
        var whoAreYouPacket = new WhoAreYouPacketBase(RandomUtility.GenerateRandomData(PacketConstants.IdNonceSize), dest.SequenceNumber);
        var packetStaticHeader = ConstructStaticHeader(PacketType.WhoAreYou, whoAreYouPacket.AuthData, packetNonce);
        var maskedHeader = new MaskedHeader(destNodeId, maskingIv);
        var encryptedMaskedHeader = maskedHeader.GetMaskedHeader(packetStaticHeader.GetHeader(), aesCrypto);
        var packet = ByteArrayUtils.JoinByteArrays(maskingIv, encryptedMaskedHeader);
        
        return Tuple.Create(packet, packetStaticHeader);
    }
    
    public Tuple<byte[], StaticHeader> BuildHandshakePacket(byte[] idSignature, byte[] ephemeralPubKey, byte[] destNodeId, byte[] maskingIv, byte[] messageCount)
    {
        var handshakePacket = new HandshakePacketBase(idSignature, ephemeralPubKey,identityManager.Record.NodeId, identityManager.Record.EncodeRecord());
        var packetNonce = ByteArrayUtils.JoinByteArrays(messageCount, RandomUtility.GenerateRandomData(PacketConstants.PartialNonceSize));
        var packetStaticHeader = ConstructStaticHeader(PacketType.Handshake, handshakePacket.AuthData, packetNonce);
        var maskedHeader = new MaskedHeader(destNodeId, maskingIv);
        var encryptedMaskedHeader = maskedHeader.GetMaskedHeader(packetStaticHeader.GetHeader(), aesCrypto);
        var packet = ByteArrayUtils.Concatenate(maskingIv, encryptedMaskedHeader);
        
        return Tuple.Create(packet, packetStaticHeader);
    }
    
    private static StaticHeader ConstructStaticHeader(PacketType packetType, byte[] authData, byte[] nonce)
    {
        return new StaticHeader(ProtocolConstants.ProtocolId, ProtocolConstants.Version, authData, (byte)packetType, nonce);
    }
}