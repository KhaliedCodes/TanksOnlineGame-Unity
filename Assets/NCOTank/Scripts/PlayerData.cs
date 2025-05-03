using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NGOTank
{

    public enum Team 
    {
        Red = 0,
        Blue = 1,
    }

    public enum Class
    {
        DPS = 0,
        Tank = 1,
    }
    public struct PlayerData : INetworkSerializable
    {
        public FixedString64Bytes PlayerName;
        public Team TeamId;
        public Class ClassId;
        // public PlayerClass Class;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref TeamId);
            serializer.SerializeValue(ref ClassId);
            // if (serializer.IsWriter)
            // {
            //     FastBufferWriter writer = serializer.GetFastBufferWriter();
            //     writer.WriteValueSafe(PlayerName);
            //     writer.WriteValueSafe(TeamId);
            //     writer.WriteValueSafe(Class);
            // }
            // else if (serializer.IsReader)
            // {
            //     FastBufferReader reader = serializer.GetFastBufferReader();
            //     reader.ReadValueSafe(out PlayerName);
            //     reader.ReadValueSafe(out TeamId);
            //     reader.ReadValueSafe(out Class);
            // }
        }

        public override string ToString()
        {
            return $"PlayerName: {PlayerName}, TeamId: {TeamId}";
        }
    }
}
