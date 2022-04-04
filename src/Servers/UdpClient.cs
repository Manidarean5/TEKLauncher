﻿using System.Net.Sockets;
using System.Threading;

namespace TEKLauncher.Servers;

/// <summary>Manages UDP traffic of the application.</summary>
/// <remarks>The purpose of this class is using only one socket to send and receive datagrams from all servers.</remarks>
static class UdpClient
{
    /// <summary>Ongoing datagram transactions.</summary>
    static readonly Dictionary<IPEndPoint, TaskCompletionSource<byte[]>> s_transactions = new(32);
    /// <summary>Underlying UDP socket.</summary>
    static readonly Socket s_socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { SendTimeout = 2000 };
    /// <summary>Starts receive loop thread.</summary>
    static UdpClient()
    {
        s_socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        new Thread(ReceiveLoop).Start();
    }
    /// <summary>Processes all incoming datagrams in a loop.</summary>
    static void ReceiveLoop()
    {
        Span<byte> buffer = stackalloc byte[1400]; //1400 bytes is the max size of Steam server query packet
        EndPoint remoteEndpoint = new IPEndPoint(0, 0);
        try
        {
            for (;;)
            {
                int bytesRead = s_socket.ReceiveFrom(buffer, ref remoteEndpoint);
                if (bytesRead <= 0 || remoteEndpoint is not IPEndPoint ipEndpoint)
                    return;
                TaskCompletionSource<byte[]>? completionSource;
                lock (s_transactions)
                {
                    if (s_transactions.TryGetValue(ipEndpoint, out completionSource))
                        s_transactions.Remove(ipEndpoint);
                    else
                        return;
                }
                completionSource.SetResult(buffer[..bytesRead].ToArray());
            }
        }
        catch { }
    }
    /// <summary>Releases the underlying socket and cancels all its send and receive operations.</summary>
    public static void Dispose() => s_socket.Dispose();
    /// <summary>Sends a datagram to specified server and gets response from it.</summary>
    /// <param name="endpoint">Endpoint of the server to send <paramref name="request"/> to and receive response from.</param>
    /// <param name="request">A span of bytes that contains the data to be sent to <paramref name="endpoint"/>.</param>
    /// <returns>Server response, or <see langword="null"/> if transaction timed out.</returns>
    public static byte[]? Transact(IPEndPoint endpoint, ReadOnlySpan<byte> request)
    {
        var completionSource = new TaskCompletionSource<byte[]>();
        lock (s_transactions)
            s_transactions.Add(endpoint, completionSource);
        lock (s_socket)
            try { s_socket.SendTo(request, endpoint); }
            catch { return null; }
        if (!completionSource.Task.Wait(2000))
        {
            lock (s_transactions)
                s_transactions.Remove(endpoint);
            return null;
        }
        return completionSource.Task.Result;
    }
}