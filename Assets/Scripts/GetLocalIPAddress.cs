using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class GetLocalIPAddress : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"Local IP Address: {RetrieveLocalIPAddress()}");
    }

    public static string RetrieveLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
