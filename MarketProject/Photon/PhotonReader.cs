using System.Data.Common;
using System.Net;
using System.Text;

namespace MarketProject.Photon
{
    internal class PhotonReader
    {
        byte signature;
        MessageTypes type;

        byte operationCode;

        byte eventCode;

        ushort operationResponseCode;
        string? operationDebugString;

        public readonly ushort parameterCount;

        public static Dictionary<long, (int ItemID, byte quality, byte Enchantment)> GetItemAverageStats = [];

        public static string Location;

        private static HttpClient sharedClient = new(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
        {
            BaseAddress = new Uri("http://81.217.181.76:5026")
        };

        public PhotonReader(BigEndianReader p)
        {
            signature = p.ReadByte();
            type = (MessageTypes)p.ReadByte();

            if (type == MessageTypes.TypeUnknown1)
            {
                p.ReadByte();
            }
            else
            {
                if (type == MessageTypes.OperationRequest)
                {
                    operationCode = p.ReadByte();
                }
                else if (type == MessageTypes.EventDataType)
                {
                    eventCode = p.ReadByte();
                }
                else if (type == MessageTypes.OperationResponse || type == MessageTypes.otherOperationResponse)
                {
                    operationCode = p.ReadByte();
                    operationResponseCode = p.ReadUInt16();
                    byte type = p.ReadByte();
                    operationDebugString = (string?)PhotonDataPackage.Decode(p, type);
                }
                else
                {

                }

                parameterCount = p.ReadUInt16();
            }

            PhotonDataPackage package = new(p, parameterCount);

            if (package.operationCode != 0)
            {
                if ((type == MessageTypes.OperationResponse || type == MessageTypes.otherOperationResponse) && package.operationCode == OperationCode.opChangeCluster)
                {
                    Location = package.data[0].ToString();
                    Console.WriteLine("Changed to Location: " + Location);
                }

                if (type == MessageTypes.OperationRequest && package.operationCode == OperationCode.opAuctionGetItemAverageStats)
                {
                    if (Convert.ToByte(package.data[3]) == 0)
                    {
                        GetItemAverageStats.Add((int)package.data[255], (Convert.ToInt16(package.data[1]), Convert.ToByte(package.data[2]), (byte)(package.data.GetValueOrDefault((byte)4) ?? (byte)0)));
                    }
                    else
                    {
                        Console.WriteLine("Timescale 24h required");
                    }
                }

                if ((type == MessageTypes.OperationResponse || type == MessageTypes.otherOperationResponse) && package.operationCode == OperationCode.opAuctionGetItemAverageStats)
                {
                    if (GetItemAverageStats.TryGetValue((int)package.data[255], out var itemAverageStats))
                    {
                        if (Location == null)
                        {
                            Console.WriteLine("Location unknown. Zone change required.");
                        }
                        else
                        {

                            var counts = (object[])package.data[0];
                            var prices = (object[])package.data[1];
                            var timestamps = (object[])package.data[2];

                            List<HistoryAPI> data = [];

                            for (int i = 0; i < counts.Length; ++i)
                            {
                                HistoryAPI a = new HistoryAPI(itemAverageStats.ItemID.ToString(),
                                    DateTime.SpecifyKind(DateTime.FromBinary(Convert.ToInt64(timestamps[i])), DateTimeKind.Utc),
                                    Convert.ToInt32(counts[i]),
                                    (Convert.ToInt64(prices[i]) / Convert.ToInt32(counts[i])),
                                    itemAverageStats.quality,
                                    itemAverageStats.Enchantment);

                                data.Add(a);
                            }

                            var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json");

                            new Task(async () =>
                            {
                                var response = await sharedClient.PostAsync($"prices/{Location}", requestContent);
                                Console.WriteLine("History data sent to server: " + response.StatusCode.ToString());
                            }).Start();
                        }
                    }
                }
            }

            if (package.eventCode != 0)
            {
                if (!(package.eventCode == EventCode.evActiveSpellEffectsUpdate
                    || package.eventCode == EventCode.evMountStart
                    || package.eventCode == EventCode.evMountCancel
                    || package.eventCode == EventCode.evMounted
                    || package.eventCode == EventCode.evCastHit
                    || package.eventCode == EventCode.evLeave
                    || package.eventCode == EventCode.evCastSpell
                    || package.eventCode == EventCode.evCastStart
                    || package.eventCode == EventCode.evCharacterEquipmentChanged
                    || package.eventCode == EventCode.evCastHits
                    || package.eventCode == EventCode.evCastFinished
                    || package.eventCode == EventCode.evHealthUpdate
                    || package.eventCode == EventCode.evEnergyUpdate
                    || package.eventCode == EventCode.evMountHealthUpdate
                    || package.eventCode == EventCode.evRegenerationPlayerComboChanged
                    || package.eventCode == EventCode.evRegenerationEnergyChanged
                    || package.eventCode == EventCode.evRegenerationMountHealthChanged
                    || package.eventCode == EventCode.evRegenerationHealthChanged
                    || package.eventCode == EventCode.evNewCharacter // Interesting?
                    || package.eventCode == EventCode.evActionOnBuildingStart //???
                    || package.eventCode == EventCode.evActionOnBuildingFinished //???
                    || package.eventCode == EventCode.evDuelStarted
                    || package.eventCode == EventCode.evDuelDenied
                    || package.eventCode == EventCode.evDuelEnded
                    || package.eventCode == EventCode.evNewBuilding //???
                    || package.eventCode == EventCode.evChangeMountSkin
                    || package.eventCode == EventCode.evTakeSilver
                    || package.eventCode == EventCode.evChangeAvatar
                    || package.eventCode == EventCode.evInCombatStateUpdate
                    || package.eventCode == EventCode.evNewTravelpoint //???
                    || package.eventCode == EventCode.evUpdateSpellEffectArea
                    || package.eventCode == EventCode.evForcedMovementCancel
                    || package.eventCode == EventCode.evInvitedToArenaMatch
                    || package.eventCode == EventCode.evEnteringArenaCancel
                    || package.eventCode == EventCode.evTreasureChestUsingStart //???
                    || package.eventCode == EventCode.evBatchUseItemEnd
                    || package.eventCode == EventCode.evBatchUseItemStart
                    || package.eventCode == EventCode.evCastlePhaseChanged //???
                    || package.eventCode == EventCode.evChannelingEnded
                    || package.eventCode == EventCode.evChannelingUpdate
                    || package.eventCode == EventCode.evEnteringArenaLockCancel
                    || package.eventCode == EventCode.evEnteringArenaLockStart
                    || package.eventCode == EventCode.evUnlockVanityUnlock
                    || package.eventCode == EventCode.evOtherGrabbedLoot
                    || package.eventCode == EventCode.evCastSpells
                    || package.eventCode == EventCode.evAttack
                    || package.eventCode == EventCode.evRedZoneEventClusterStatus
                    || package.eventCode == EventCode.evNewTutorialBlocker
                    || package.eventCode == EventCode.evDuelReEnteredArea
                    || package.eventCode == EventCode.evCraftItemFinished
                    || package.eventCode == EventCode.evMiniMapOwnedBuildingsPositions //Interesting?
                    ))
                {

                }
            }
        }
    }

    enum MessageTypes
    {
        TypeUnknown1 = 1,
        OperationRequest = 2,
        otherOperationResponse = 3,
        EventDataType = 4,
        OperationResponse = 7
    }
    public record HistoryAPI(string Name, DateTimeOffset Time, int Count, long AvgPrice, byte Quality = 0, byte Enchantment = 0);
}
