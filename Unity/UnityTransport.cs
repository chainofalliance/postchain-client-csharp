using Chromia;
using Chromia.Transport;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using System.Threading;

public class UnityTransport : ITransport
{
    public async Task<Chromia.Buffer> Get(Uri uri, CancellationToken ct)
    {
        using var response = await HandleRequest(UnityWebRequest.Get(uri), ct);
        HandleResponse(response);

        var content = response.downloadHandler.data;
        return Chromia.Buffer.From(content);
    }

    public async Task<Chromia.Buffer> Post(Uri uri, Chromia.Buffer content, CancellationToken ct)
    {
        var uploadHandler = new UploadHandlerRaw(content.Bytes)
        {
            contentType = "application/json"
        };

        return await Post(uri, uploadHandler, ct);
    }

    public async Task<Chromia.Buffer> Post(Uri uri, string content, CancellationToken ct)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(content);
        var uploadHandler = new UploadHandlerRaw(bodyRaw)
        {
            contentType = "application/json"
        };

        return await Post(uri, uploadHandler, ct);
    }

    public async Task Delay(int milliseconds, CancellationToken ct)
    {
        await UniTask.Delay(milliseconds, cancellationToken: ct);
    }

    private static async UniTask<Chromia.Buffer> Post(Uri uri, UploadHandlerRaw uploadHandler, CancellationToken ct)
    {
        using (var request = new UnityWebRequest(uri, "POST"))
        {
            request.uploadHandler = uploadHandler;
            request.downloadHandler = new DownloadHandlerBuffer();

            using var response = await HandleRequest(request, ct);
            var content = response.downloadHandler.data;
            return Chromia.Buffer.From(content);
        };
    }

    private static async UniTask<UnityWebRequest> HandleRequest(UnityWebRequest request, CancellationToken token)
    {
        try
        {
            await request.SendWebRequest().WithCancellation(token);
        }
        catch (Exception e)
        {
            if (e is UnityWebRequestException u)
                HandleResponse(u.UnityWebRequest);
            else
                throw new TransportException(
                    TransportException.ReasonCode.Unknown,
                    e.Message
                );
        }

        HandleResponse(request);
        return request;
    }

    private static void HandleResponse(UnityWebRequest response)
    {
        var str = $"{response.error} {response.downloadHandler.text}";
        if (response.result == UnityWebRequest.Result.ProtocolError)
            throw new TransportException(
                TransportException.ReasonCode.HttpError,
                str,
                (int)response.responseCode
            );
        else if (response.result == UnityWebRequest.Result.ConnectionError)
            throw new TransportException(
                TransportException.ReasonCode.Timeout,
                str
            );
        else if (response.result != UnityWebRequest.Result.Success)
            throw new TransportException(
                TransportException.ReasonCode.Unknown,
                str
            );
    }
}
