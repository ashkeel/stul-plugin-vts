using NativeWebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StulPlugin
{
    using KvSubscriber = Action<KvData>;

    [Serializable]
    class KvInfo
    {
        public string key;

        public KvInfo(string key)
        {
            this.key = key;
        }
    }

    [Serializable]
    public class KvData
    {
        public string key;
        public string data;

        public KvData(string key, string data)
        {
            this.key = key;
            this.data = data;
        }
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    class ClientMessage<T>
    {
        public string command;
        public string request_id;
        public T data;

        public ClientMessage(string command, T data)
        {
            this.command = command;
            this.data = data;
            request_id = Guid.NewGuid().ToString();
        }
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    class ServerMessage
    {
        public string type;
        public bool? ok;
        public string request_id;
    }

    [Serializable]
    class ServerResponse<T> : ServerMessage
    {
        public T data;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    class ServerPush : ServerMessage
    {
        public string key;
        public string new_value;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    class ServerError : ServerMessage
    {
        public string error;
        public string details;
    }

    class KvMessages
    {
        public static ClientMessage<KvInfo> Subscribe(string key)
        {
            return new ClientMessage<KvInfo>("ksub", new KvInfo(key));
        }

        public static ClientMessage<KvInfo> Unsubscribe(string key)
        {
            return new ClientMessage<KvInfo>("kunsub", new KvInfo(key));
        }

        public static ClientMessage<KvInfo> Get(string key)
        {
            return new ClientMessage<KvInfo>("kget", new KvInfo(key));
        }

        public static ClientMessage<KvData> SetKey(string key, string data)
        {
            return new ClientMessage<KvData>("kset", new KvData(key, data));
        }
    }


    public class KilovoltClient : MonoBehaviour
    {
        private WebSocket _websocket;
        private Dictionary<string, Action<string>> _pending = new Dictionary<string, Action<string>>();
        private Dictionary<string, List<KvSubscriber>> _subscriptions = new Dictionary<string, List<KvSubscriber>>();
        
        public async Task Connect(string url = "ws://localhost:4337/ws", string authKey = "")
        {
            var result = new TaskCompletionSource<bool>();
            _websocket = new WebSocket(url);
            _websocket.OnOpen += async () =>
            {
                Log.Info("Connected to strimertul!");
                // Send auth key if provided
                if (!string.IsNullOrEmpty(authKey))
                {
                    Log.Debug("Authenticating");
                    await AuthFlow(authKey);
                }
                result.SetResult(true);
            };
            _websocket.OnError += e => { Log.Error("Error! " + e); };
            _websocket.OnClose += e => { Log.Warn("Connection closed!"); };
            _websocket.OnMessage += async bytes =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                var payloads = message.Split('\n');
                foreach (var payload in payloads)
                {
                    Log.Debug("-> " + payload);
                    var obj = JsonConvert.DeserializeObject<ServerMessage>(payload);
                    switch (obj.type)
                    {
                        case "hello":
                            break;
                        case "response":
                            // Handle response
                            if (_pending.ContainsKey(obj.request_id))
                            {
                                var callback = _pending[obj.request_id];
                                _pending.Remove(obj.request_id);
                                callback(payload);
                            }

                            break;
                        case "push":
                            // Handle subs
                            var push = JsonConvert.DeserializeObject<ServerPush>(payload);
                            var data = new KvData(push.key, push.new_value);
                            if (_subscriptions.ContainsKey(push.key))
                            {
                                var subscribers = _subscriptions[push.key];
                                foreach (var subscriber in subscribers)
                                {
                                    subscriber(data);
                                }
                            }

                            break;
                        default:
                            Log.Error($"unknown msg: {obj.type}");
                            break;
                    }
                }
            };

            _websocket.Connect();
            await result.Task;
        }

        // Basic functions (bare string values)

        public async Task<string> Get(string key)
        {
            var msg = KvMessages.Get(key);
            return await Send<KvInfo, string>(msg);
        }

        public async Task Set(string key, string data)
        {
            var msg = KvMessages.SetKey(key, data);
            await SendMessage(msg);
        }

        public async Task SubscribeKey(string key, Action<KvData> callback)
        {
            if (!_subscriptions.ContainsKey(key))
            {
                _subscriptions[key] = new List<KvSubscriber>();
            }

            _subscriptions[key].Add(callback);
            await SendMessage(KvMessages.Subscribe(key));
        }

        public async Task UnsubscribeKey(string key, Action<KvData> callback)
        {
            if (_subscriptions.ContainsKey(key))
            {
                _subscriptions[key].Remove(callback);
                if (_subscriptions[key].Count == 0)
                {
                    _subscriptions.Remove(key);
                    await SendMessage(KvMessages.Unsubscribe(key));
                }
            }
        }

        // JSON shortcuts

        public async Task<T> GetJSON<T>(string key) => JsonConvert.DeserializeObject<T>(await Get(key));
        public async Task SetJSON<T>(string key, T data) => await Set(key, JsonConvert.SerializeObject(data));

        // Auth flow

        [Serializable]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        class AuthChallenge
        {
            public string challenge;
            public string salt;
        }

        [Serializable]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        class AuthResponse
        {
            public string hash;

            public AuthResponse(string hash)
            {
                this.hash = hash;
            }
        }

        private async Task AuthFlow(string authKey)
        {
            // Send challenge request
            var challenge = await Send<string, AuthChallenge>(new ClientMessage<string>("klogin", null));

            // Prepare hash
            var keyBytes = Encoding.UTF8.GetBytes(authKey);
            var challengeBytes = Convert.FromBase64String(challenge.challenge);
            var saltBytes = Convert.FromBase64String(challenge.salt);
            var hasher = new HMACSHA256(keyBytes.Concat(saltBytes).ToArray());
            var result = hasher.ComputeHash(challengeBytes);

            // Send response
            var response = new AuthResponse(Convert.ToBase64String(result));
            await SendMessage(new ClientMessage<AuthResponse>("kauth", response));
        }

        // Internals for sending messages

        private async Task<TResponse> Send<TRequest, TResponse>(ClientMessage<TRequest> message)
        {
            var result = new TaskCompletionSource<TResponse>();
            _pending[message.request_id] = data =>
            {
                var response = JsonConvert.DeserializeObject<ServerResponse<TResponse>>(data);
                result.SetResult(response.data);
            };
            await SendMessage(message);
            return await result.Task;
        }

        private async Task SendMessage<T>(T message)
        {
            if (_websocket.State != WebSocketState.Open)
            {
                throw new Exception("websocket not connected!");
            }

            var json = JsonConvert.SerializeObject(message);
            Log.Debug("<- " + json);
            await _websocket.SendText(json);
        }

        void Update()
        {
            _websocket.DispatchMessageQueue();
        }

        private async void OnApplicationQuit()
        {
            await Close();
        }

        public async Task Close()
        {
            if (_websocket.State == WebSocketState.Open)
            {
                await _websocket.Close();
            }
        }
    }
}