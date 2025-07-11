using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays the local IPv4 address of the device on a TextMeshPro UI element in the scene.
/// Filters out VPN interfaces like NordLynx and WireGuard.
/// </summary>
public class IPAddressDisplay : MonoBehaviour
{
    // Reference to the UI text element where the IP address will be shown
    [SerializeField] private TMP_Text ipAddressText;

    /// <summary>
    /// Called on script start. Sets the IP address text to the first detected valid IPv4 address.
    /// </summary>
    private void Start()
    {
        ipAddressText.text = GetLocalIPAddress();
    }

    /// <summary>
    /// Retrieves the local IPv4 address of the machine, excluding VPN interfaces.
    /// </summary>
    /// <returns>
    /// A string representing the first available local IPv4 address, or 127.0.0.1 if none found.
    /// </returns>
    private string GetLocalIPAddress()
    {
        // Iterate through all available network interfaces
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            // Skip interfaces that are not currently up
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            // Get interface name and description in lowercase for matching
            string name = ni.Name.ToLower();
            string desc = ni.Description.ToLower();

            // Skip VPN interfaces such as NordLynx or WireGuard
            if (name.Contains("nordlynx") || desc.Contains("nordlynx") ||
                name.Contains("wireguard") || desc.Contains("wireguard"))
                continue;

            // Check for valid IPv4 addresses on this interface
            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork) // IPv4
                {
                    return ip.Address.ToString(); // Return the first valid local IP address found
                }
            }
        }

        // If no suitable address was found, return localhost as fallback
        return "127.0.0.1";
    }
}
