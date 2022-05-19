using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StulPlugin
{
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TwitchUser
    {
        public string ID;
        public string Name;
        public string DisplayName;
        public string Color;
        public Dictionary<string, int> Badges;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TwitchEmote
    {
        public string Name;
        public string ID;
        public int Count;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TwitchChatMessage
    {
        public TwitchUser User;
        public string Raw;
        public int Type;
        public string RawType;
        public Dictionary<string, string> Tags;
        public string Message;
        public string Channel;
        public string RoomID;
        public string ID;
        public string Time;
        public TwitchEmote[] Emotes;
        public int Bits;
        public bool Action;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TwitchEventSub
    {
        public string type;
        public string created_ad;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TwitchEvent
    {
        public TwitchEventSub subscription;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TwitchRaidEventData
    {
        public string from_broadcaster_user_id;
        public string from_broadcaster_user_login;
        public string from_broadcaster_user_name;
        public string to_broadcaster_user_id;
        public string to_broadcaster_user_login;
        public string to_broadcaster_user_name;
        public int viewers;
    }

    [Serializable]
    public class TwitchRaidEvent : TwitchEvent
    {
        [JsonProperty("event")] public TwitchRaidEventData eventData;
    }

    public class TwitchEventSource
    {
        public event Action<TwitchChatMessage> OnChatMessage;
        public event Action<TwitchRaidEvent> OnRaid;

        public async Task SetClient(KilovoltClient client)
        {
            // Chat messages
            await client.SubscribeKey("twitch/ev/chat-message", data =>
            {
                var message = JsonConvert.DeserializeObject<TwitchChatMessage>(data.data);
                OnChatMessage?.Invoke(message);
                Log.Info($"<{message.User.DisplayName}> {message.Message}");
            });

            // Raids
            await client.SubscribeKey("stulbe/ev/webhook", data =>
            {
                var message = JsonConvert.DeserializeObject<TwitchEvent>(data.data);
                Log.Info($"Received event of type \"{message.subscription.type}\"");
                switch (message.subscription.type)
                {
                    case "channel.raid":
                        var raid = JsonConvert.DeserializeObject<TwitchRaidEvent>(data.data);
                        OnRaid?.Invoke(raid);
                        break;
                }
            });
        }
    }
}