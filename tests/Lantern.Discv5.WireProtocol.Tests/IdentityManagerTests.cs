using System.Net;
using Lantern.Discv5.Enr;
using Lantern.Discv5.Enr.Entries;
using Lantern.Discv5.WireProtocol.Connection;
using Lantern.Discv5.WireProtocol.Identity;
using Lantern.Discv5.WireProtocol.Logging;
using Lantern.Discv5.WireProtocol.Session;
using Lantern.Discv5.WireProtocol.Utility;
using NUnit.Framework;

namespace Lantern.Discv5.WireProtocol.Tests;

[TestFixture]
public class IdentityManagerTests
{
    private static IdentityManager _identityManager = null!;
    
    [SetUp]
    public void Setup()
    {
        var connectionOptions = new ConnectionOptions();
        var sessionOptions = SessionOptions.Default;
        var loggerFactory = LoggingOptions.Default;
        _identityManager = new IdentityManager(sessionOptions, Discv5ProtocolBuilder.CreateNewRecord(connectionOptions, sessionOptions.Verifier, sessionOptions.Signer),loggerFactory);
    }
    
    [Test]
    public void Test_IdentityManager_ShouldResultInFalseWhenNoIpAndPortIsSet()
    {
        Assert.IsFalse(_identityManager.IsIpAddressAndPortSet());
    }
    
    [Test]
    public void Test_IdentityManager_ShouldResultInTrueWhenIpV4AndPortIsSet()
    {
        _identityManager.UpdateIpAddressAndPort(new IPEndPoint(IpUtility.GetLocalIpAddress(), 1234));
        Assert.IsTrue(_identityManager.IsIpAddressAndPortSet());
    }
    
    [Test]
    public void Test_IdentityManager_ShouldResultInTrueWhenIpV6AndPortIsSet()
    {
        _identityManager.UpdateIpAddressAndPort(new IPEndPoint(IPAddress.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334"), 1234));
        Assert.IsTrue(_identityManager.IsIpAddressAndPortSet());
    }
    
    [Test]
    public void Test_IdentityManager_ShouldUpdateIpV4AndPortCorrectly()
    {
        var node = _identityManager.Record;
        
        Assert.IsFalse(node.HasKey(EnrEntryKey.Ip));
        Assert.IsFalse(node.HasKey(EnrEntryKey.Udp));
        var endpoint = new IPEndPoint(IpUtility.GetLocalIpAddress(), 1234);

        _identityManager.UpdateIpAddressAndPort(endpoint);
        
        Assert.AreEqual(endpoint.Address, node.GetEntry<EntryIp>(EnrEntryKey.Ip).Value);
        Assert.AreEqual(endpoint.Port, node.GetEntry<EntryUdp>(EnrEntryKey.Udp).Value);
    }
    
    [Test]
    public void Test_IdentityManager_ShouldUpdateIpV6AndPortCorrectly()
    {
        var node = _identityManager.Record;
        
        Assert.IsFalse(node.HasKey(EnrEntryKey.Ip6));
        Assert.IsFalse(node.HasKey(EnrEntryKey.Udp6));
        var endpoint = new IPEndPoint(IPAddress.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334"), 1234);

        _identityManager.UpdateIpAddressAndPort(endpoint);
        
        Assert.AreEqual(endpoint.Address, node.GetEntry<EntryIp6>(EnrEntryKey.Ip6).Value);
        Assert.AreEqual(endpoint.Port, node.GetEntry<EntryUdp6>(EnrEntryKey.Udp6).Value);
    }
}