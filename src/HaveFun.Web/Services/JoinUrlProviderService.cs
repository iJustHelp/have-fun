using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace HaveFun.Web;

public sealed class JoinUrlProviderService : IJoinUrlProviderService
{
    public JoinUrls GetJoinUrls(Uri currentUri)
    {
        var localhostUrl = BuildUrl(currentUri, "localhost");
        var lanAddress = GetLanAddress();
        var lanUrl = lanAddress is null ? null : BuildUrl(currentUri, lanAddress);
        var preferredUrl = ShouldPreferLanUrl(currentUri) ? lanUrl : null;

        return new JoinUrls(localhostUrl, lanUrl, preferredUrl);
    }

    private static string BuildUrl(Uri currentUri, string host)
    {
        var builder = new UriBuilder(currentUri)
        {
            Host = host
        };

        return builder.Uri.GetLeftPart(UriPartial.Authority);
    }

    private static string? GetLanAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(IsCandidateInterface)
            .SelectMany(GetCandidateAddresses)
            .OrderByDescending(candidate => candidate.Score)
            .Select(candidate => candidate.Address.ToString())
            .FirstOrDefault();
    }

    private static bool IsLanIpv4Address(IPAddress address)
    {
        return address.AddressFamily == AddressFamily.InterNetwork
            && !IPAddress.IsLoopback(address);
    }

    private static bool ShouldPreferLanUrl(Uri currentUri)
    {
        return currentUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || currentUri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || currentUri.Host.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCandidateInterface(NetworkInterface networkInterface)
    {
        return networkInterface.OperationalStatus == OperationalStatus.Up
            && networkInterface.NetworkInterfaceType is not NetworkInterfaceType.Loopback
            && networkInterface.NetworkInterfaceType is not NetworkInterfaceType.Tunnel
            && !LooksVirtualOrNonLan(networkInterface);
    }

    private static IEnumerable<LanAddressCandidate> GetCandidateAddresses(NetworkInterface networkInterface)
    {
        var properties = networkInterface.GetIPProperties();
        var hasGateway = properties.GatewayAddresses.Any(gateway => IsLanIpv4Address(gateway.Address));
        var typeScore = networkInterface.NetworkInterfaceType switch
        {
            NetworkInterfaceType.Wireless80211 => 20,
            NetworkInterfaceType.Ethernet => 15,
            _ => 0
        };

        return properties.UnicastAddresses
            .Select(unicastAddress => unicastAddress.Address)
            .Where(IsLanIpv4Address)
            .Select(address => new LanAddressCandidate(address, (hasGateway ? 100 : 0) + typeScore));
    }

    private static bool LooksVirtualOrNonLan(NetworkInterface networkInterface)
    {
        var text = $"{networkInterface.Name} {networkInterface.Description}";

        return ContainsAny(
            text,
            "vethernet",
            "virtual",
            "hyper-v",
            "wsl",
            "docker",
            "vmware",
            "virtualbox",
            "vpn",
            "bluetooth");
    }

    private static bool ContainsAny(string text, params string[] values)
    {
        return values.Any(value => text.Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record LanAddressCandidate(IPAddress Address, int Score);
}
