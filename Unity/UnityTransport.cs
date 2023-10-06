using Chromia;
using Chromia.Transport;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;

public class UnityTransport : ITransport
{
    public async Task<Chromia.Buffer> Get(Uri uri)
    {
        using var response = await HandleRequest(UnityWebRequest.Get(uri));
        HandleResponse(response);

        var content = response.downloadHandler.data;
        return Chromia.Buffer.From(content);
    }

    public async Task<Chromia.Buffer> Post(Uri uri, Chromia.Buffer content)
    {
        var uploadHandler = new UploadHandlerRaw(content.Bytes)
        {
            contentType = "application/json"
        };

        return await Post(uri, uploadHandler);
    }

    public async Task<Chromia.Buffer> Post(Uri uri, string content)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(content);
        var uploadHandler = new UploadHandlerRaw(bodyRaw)
        {
            contentType = "application/json"
        };

        return await Post(uri, uploadHandler);
    }

    public async Task Delay(int milliseconds)
    {
        await UniTask.Delay(milliseconds);
    }

    private static async UniTask<Chromia.Buffer> Post(Uri uri, UploadHandlerRaw uploadHandler)
    {
        using (var request = new UnityWebRequest(uri, "POST"))
        {
            request.uploadHandler = uploadHandler;
            request.downloadHandler = new DownloadHandlerBuffer();

            using var response = await HandleRequest(request);
            var content = response.downloadHandler.data;
            return Chromia.Buffer.From(content);
        };
    }

    private static async UniTask<UnityWebRequest> HandleRequest(UnityWebRequest request)
    {
        UnityWebRequest response = null;
        try
        {
            response = await request.SendWebRequest();
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

        HandleResponse(response);
        return response;
    }

    private static void HandleResponse(UnityWebRequest response)
    {
        if (response.result == UnityWebRequest.Result.ProtocolError)
            throw new TransportException(
                TransportException.ReasonCode.HttpError,
                response.error,
                (int)response.responseCode
            );
        else if (response.result == UnityWebRequest.Result.ConnectionError)
            throw new TransportException(
                TransportException.ReasonCode.Timeout,
                response.error
            );
        else if (response.result != UnityWebRequest.Result.Success)
            throw new TransportException(
                TransportException.ReasonCode.Unknown,
                response.error
            );
    }
}
