using Network.Common;
using Network.Packet;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Network.Packet.Response.Types;

public enum ServerOperationResult
{
    // Command Result
    None = 0,
    Success,
    Fail,
    Error,

    // Wrong Server State Operation
    Error_LobbyOperationOnly,
    Error_SceneChangingOperationOnly,
    Error_IngameOperationOnly,

    // Connection Operation
    NotConnectedToServer,

    // Lobby Operation
    Lobby_WrongCharacterType,
    Lobby_AlreaySelectedCharacter,
    Lobby_UnselectCharacter,
    Lobby_CharacterSelected,
    Lobby_ChangeCharacter,
    Lobby_CannotSelectCharacter,
    Lobby_CannotSelectCharacterWhenReady,

    Lobby_ReadyToStart,
    Lobby_CannotReady,
    Lobby_CannotReadyUntilSelectCharacter,
    Lobby_CannotStartUntilEveryUserSelectCharacter,

    Lobby_Unready,

    Lobby_StartTheGame,
    Lobby_OnlySuqadLeaderCanStartTheGame,
    Lobby_SomeUserAreNotReady,

    // About Inventory
    Inventory_FullWeaponSlot,
    Inventory_FullInventory,
    Inventory_CannotObtainThisItem,
    Inventory_WrongOperation,
    Inventory_YouAlreadyHaveThisWeapon,

    // Server Side Operation
    ServerClosed_AllSessionDisconnected,
    ServerClosed_SquadLeaderDisconnected,
    ServerClosed_Disconnected,
    ServerClosed_SceneChangeError,

    ServerSceneLoad_MapLoadingFail,

    // Client Side Operation
    ClientClosed_UnexpectedErrorOccurred,

    // ##### Result With Additional Data #####
    ResultWithAdditionalData = 1000,

    // About Inventory
    Inventory_SuccessObtainWeapon,
    Inventory_SuccessDropWeapon,
}

public static class GlobalTable
{
    public static bool HasAdditionalData(this ServerOperationResult serverOperationResult)
    {
        return (int)serverOperationResult > (int)ServerOperationResult.ResultWithAdditionalData;
    }

    private static Dictionary<ServerOperationResult, string> mSystemMessageTable = new()
    {
        // Command Result
        { ServerOperationResult.None                                    , "알 수 없는 오류입니다." },
        { ServerOperationResult.Success                                 , "성공" },
        { ServerOperationResult.Fail                                    , "실패" },
        { ServerOperationResult.Error                                   , "에러" },

        // Wrong Server State Operation
        { ServerOperationResult.Error_LobbyOperationOnly                , "해당 기능은 로비에서만 사용 가능합니다." },
        { ServerOperationResult.Error_SceneChangingOperationOnly        , "해당 기능은 씬 변경시에만 사용 가능합니다." },
        { ServerOperationResult.Error_IngameOperationOnly               , "해당 기능은 게임플레이에서만 사용 가능합니다." },

        // Connection Operation
        { ServerOperationResult.NotConnectedToServer                    , "서버에 연결되지 않았습니다." },

        // Lobby Operation
        { ServerOperationResult.Lobby_WrongCharacterType                , "잘못된 캐릭터를 선택했습니다." },
        { ServerOperationResult.Lobby_AlreaySelectedCharacter           , "이미 선택된 캐릭터입니다." },
        { ServerOperationResult.Lobby_UnselectCharacter                 , "캐릭터 선택을 취소했습니다." },
        { ServerOperationResult.Lobby_CharacterSelected                 , "캐릭터를 선택했습니다." },
        { ServerOperationResult.Lobby_ChangeCharacter                   , "캐릭터를 교체했습니다." },
        { ServerOperationResult.Lobby_CannotSelectCharacter             , "현재 캐릭터를 선택할 수 없습니다." },
        { ServerOperationResult.Lobby_CannotSelectCharacterWhenReady    , "준비상태에서는 캐릭터를 선택할 수 없습니다." },

        { ServerOperationResult.Lobby_ReadyToStart                      , "준비했습니다." },
        { ServerOperationResult.Lobby_CannotReady                       , "현재 준비할 수 없습니다." },
        { ServerOperationResult.Lobby_CannotReadyUntilSelectCharacter   , "캐릭터를 선택할 때 까지 준비할 수 없습니다." },
        { ServerOperationResult.Lobby_CannotStartUntilEveryUserSelectCharacter, "캐릭터를 선택하지 않은 유저가 있습니다." },

        { ServerOperationResult.Lobby_Unready                           , "준비를 취소했습니다." },

        { ServerOperationResult.Lobby_StartTheGame                      , "게임을 시작합니다." },
        { ServerOperationResult.Lobby_OnlySuqadLeaderCanStartTheGame    , "방장만 게임을 시작할 수 있습니다." },
        { ServerOperationResult.Lobby_SomeUserAreNotReady               , "준비하지 않은 유저가 있습니다." },

        // About Inventory
        { ServerOperationResult.Inventory_FullWeaponSlot                , "무기 슬롯이 가득 찼습니다." },
        { ServerOperationResult.Inventory_FullInventory                 , "인벤토리가 가득 찼습니다." },
        { ServerOperationResult.Inventory_CannotObtainThisItem          , "이 아이템은 습득할 수 없습니다." },
        { ServerOperationResult.Inventory_WrongOperation                , "잘못된 인벤토리 조작." },
        { ServerOperationResult.Inventory_YouAlreadyHaveThisWeapon      , "이미 가지고 있는 무기입니다." },

        // Server Side Operation
        { ServerOperationResult.ServerClosed_AllSessionDisconnected     , "모든 유저가 접속을 종료했습니다." },
        { ServerOperationResult.ServerClosed_SquadLeaderDisconnected    , "방장이 게임을 나갔습니다." },
        { ServerOperationResult.ServerClosed_Disconnected               , "서버의 연결이 끊겼습니다." },
        { ServerOperationResult.ServerClosed_SceneChangeError           , "서버 Scene 변경 에러!" },

        { ServerOperationResult.ServerSceneLoad_MapLoadingFail          , "서버 맵 불러오기 실패!" },

        // Client Side Operation
        { ServerOperationResult.ClientClosed_UnexpectedErrorOccurred    , "서버 Response handling 실패! 알 수 없는 에러." },

        // ##### Result With Additional Data #####

        // About Inventory
        { ServerOperationResult.Inventory_SuccessObtainWeapon           , "를 습득했습니다." },
        { ServerOperationResult.Inventory_SuccessDropWeapon             , "를 버렸습니다." },
    };

    public static string GetSystemMessageByCommandData(SessionResponseCommandData commandData)
    {
        ServerOperationResult result = (ServerOperationResult)commandData.ServerOperationResultCode;

        string systemResultMessage = GetSystemMessageByResult(result);

        if (result.HasAdditionalData())
        {
            switch (result)
            {
                case ServerOperationResult.Inventory_SuccessObtainWeapon:
                    {
                        ItemType obtainedItem = commandData.ObtainedItemType;
                        string itemName = obtainedItem.GetItemName();
                        return $"{itemName}{systemResultMessage}";
                    }

                case ServerOperationResult.Inventory_SuccessDropWeapon:
                    {
                        ItemType obtainedItem = commandData.DroppedWeaponItemType;
                        string itemName = obtainedItem.GetItemName();
                        return $"{itemName}{systemResultMessage}";
                    }

                default:
                    return systemResultMessage;
            }
        }

        return systemResultMessage;
    }

    public static string GetSystemMessageByResult(ServerOperationResult result)
    {
        return mSystemMessageTable.TryGetValue(result, out string message) ? message : "알 수 없는 에러";
    }

    private static Dictionary<EntityType, string> mEntityNameTable = new()
    {
        { EntityType.kHumanoid                  , "휴머노이드" },
        { EntityType.kTestPlayer                , "테스트 플레이어" },
        { EntityType.kPlayerGriffin             , "그리핀" },
        { EntityType.kPlayerPoopu               , "푸프" },
        { EntityType.kPlayerClo                 , "클로" },
        { EntityType.kPlayerDerin               , "데린" },
        { EntityType.kLastPlayerEntity          , "휴머노이드 마지막 인덱스" },

        { EntityType.kEnemyHumanoidTestTurret   , "테스트 터렛" },
        { EntityType.kEnemyHumanoidHealTurret   , "테스트 힐 터렛" },
        { EntityType.kWisp                      , "위습" },
        { EntityType.kMob                       , "몹" },
        { EntityType.kTestMob                   , "테스트 몹" },
        { EntityType.kMagicBore                 , "매직보어" },
        { EntityType.kTurret                    , "터렛" },
        { EntityType.kSpirit                    , "스피릿" },

        { EntityType.kWeapon                    , "무기" },
        { EntityType.kRocketWeapon              , "로켓" }, // WiseMansWand
        { EntityType.kMagicStick                , "완드" }, // HellishMace
        { EntityType.kBioticRecovery            , "회복완드" }, // NobleSacrifice
        { EntityType.kWeaponKatana              , "카타나" },  // Katana
        { EntityType.kRayStick                  , "레이저" },
        { EntityType.kLightningStick            , "번개완드" }, // LightningWand
        { EntityType.kReflectSword              , "반사검" }, // SwordOfTheWorldTree

        { EntityType.kStructureMagicStoneA      , "큰 마법석" },
        { EntityType.kStructureMagicStoneB      , "마법석" },
        { EntityType.kStructureMagicStoneC      , "작은 마법석" },
        { EntityType.kStructureBossStoneConePhase_1, "푸른 마법석" },
        { EntityType.kStructureBossStoneConePhase_2, "붉은 마법석" },
        { EntityType.kStructureBossWaveRock     , "파도 마법석" },
    };
    
    public static string GetEntityName(this EntityType entityType)
    {
        return  mEntityNameTable.TryGetValue(entityType, out var name) ? name : GlobalDefaultString.DefaultEntityName;
    }

    private static Dictionary<ItemType, string> mItemNameTable = new Dictionary<ItemType, string>()
    {
        { ItemType.kWeaponWiseMansWand          , "현자의 지팡이" }, // 파란거
        { ItemType.kWeaponHellishMace           , "지옥 철퇴" },    // 빨간거
        { ItemType.kWeaponKeyOfWisdom           , "지혜의 열쇠" }, // 핑크^^
        { ItemType.kWeaponRayStick              , "레이저" },      // 레이저
        { ItemType.kWeaponLightningWand         , "번개 지팡이" }, // 번개
        { ItemType.kWeaponNobleSacrifice        , "숭고한 희생" }, // 힐무기
        { ItemType.kWeaponSwordOfTheWorldTree   , "세계수의 검" }, // 반사검
    };

    private static Dictionary<ItemType, string> mItemAdditionalInfo = new Dictionary<ItemType, string>()
    {
        { ItemType.kWeaponWiseMansWand          , "휴이의 지팡이를 본떠서 만든 완드" },
        { ItemType.kWeaponHellishMace           , "강력한 불 마술이 부여된 철퇴" },
        { ItemType.kWeaponKeyOfWisdom           , "별들을 조종할 수 있는 비밀의 열쇠" },
        { ItemType.kWeaponLightningWand         , "자연의 번개가 응축된 지팡이" },
        { ItemType.kWeaponNobleSacrifice        , "숲의 기운으로 회복하는 힘을 가진 완드" },
        { ItemType.kWeaponSwordOfTheWorldTree   , "식물의 뿌리가 얽힌 오래된 칼날" },
    };

    public static string GetItemName(this ItemType itemType)
    {
        return mItemNameTable.TryGetValue(itemType, out var name) ? name : GlobalDefaultString.DefaultItemName;
    }

    public static string GetItemAdditionalInfo(this ItemType itemType)
    {
        return mItemAdditionalInfo.TryGetValue(itemType, out var info) ? info : GlobalDefaultString.DefaultItemAdditionalInfo;
    }

    public static string GetItemStatInfo(this ItemType itemType)
    {
        if (ItemManager.TryGetConfig(itemType, out var config))
        {
            return $"공격력 : {config.DAMAGE}\n초  당 : {(int)(1 / config.FIRE_DELAY)}회";
        }

        return "";
    }

}

public enum AreaType
{
    None = -1,
    CheckPoint_0 = 0,
    CheckPoint_1,
    CheckPoint_2,
    CheckPoint_3,
    CheckPoint_4,
    CheckPoint_5,
}

public static class GlobalAreaName
{
    private static Dictionary<AreaType, string> mAreaName = new Dictionary<AreaType, string>()
    {
        { AreaType.CheckPoint_0, "모험의 시작" },
        { AreaType.CheckPoint_1, "돌아올 수 없는 숲" },
        { AreaType.CheckPoint_2, "탄식의 절벽" },
        { AreaType.CheckPoint_3, "대륙의 통로" },
        { AreaType.CheckPoint_4, "거인이 잠든 대지" },
        { AreaType.CheckPoint_5, "잊혀진 자의 흔적" },
    };

    public static string GetAreaName(this AreaType areaType)
    {
        return mAreaName.TryGetValue(areaType, out var name) ? name : GlobalDefaultString.DefaultAreaName;
    }

    public static string GetAreaName(int checkPointNumber)
    {
        return GetAreaName((AreaType)checkPointNumber);
    }

    public static string StageName = "영준의 둥지";
}

public enum PooriScriptType
{
    None,

    // Island 1
    Script_0_FirstGreetting,
    Script_1_HowToMove,
    Script_2_Fall,
    Script_3_CheckPoint,
    Script_4_FirstSecretFound,
    Script_5_AfterAmbushAttack,

    // Island 2
    Script_6_BridgeCollapsed,
    Script_7_WeaponInventory,
    Script_8_WhenYouFoundSecretWay,
    Script_9_OnLeafs,
    Script_10_WhenYouFallDownToBottom,
    Script_11_FirstMeetWithTurret,
    Script_12_ZommInOutTutorial,
    Script_13_FirstWave,
    Script_14_BridgeWave,

    // Island 3
    Script_15_WhenYouExitTheCave,
    Script_16_AfterBossCameOut,
    Script_17_WhenYouTryToEscape,
}

public static class GlobalPooriScript
{
    private static Dictionary<PooriScriptType, string> mPooriScriptTable = new Dictionary<PooriScriptType, string>()
    {

                                                   // 경계선 ^^
        // Island 1
        { PooriScriptType.Script_0_FirstGreetting,
            "안녕하세요? 처음 보는 분들이네요!\n저는 [ 푸리 ]라고 해요. 만나서 반가워요!" },

        { PooriScriptType.Script_1_HowToMove,
            "이 곳 [ 하늘섬 ]을 둘러보고 싶다면\n[ WASD ]를 눌러서 움직여 보세요" },

        { PooriScriptType.Script_2_Fall,
            "저 앞에 반짝이는 물건은 [ 무기 ]에요\n가까이 가서 [ E ]를 눌러 주워볼까요?\n" +
            "낮은 절벽은 무서워 하지 말아요. 떨어져도 안 아프니까요!" },

        { PooriScriptType.Script_3_CheckPoint,
            "[ 유적 ]에 가까이 다가가서\n활성화할 수 있어요\n[ 유적 ]을 활성화하면 친구와\n신비한 무기가 소환돼요!\n\n" +
            "신비한 무기는 정말 강력하니 \n꼭 가져가세요!" },

        { PooriScriptType.Script_4_FirstSecretFound,
            "우와~, 숨겨진 무기를 찾았네요?\n이런 장소에는 나가는 길이 숨겨져 있어요." },

        { PooriScriptType.Script_5_AfterAmbushAttack,
            "앗, [ 매직보어 ]들의 기습이에요!\n[ 매직보어 ]들의 출몰 지역에\n[ 표지판 ]이 설치되어 있으니 조심하세요!\n\n" +
            "[ 마우스 왼쪽 클릭 ]으로 무기를 사용해서\n매직보어들과 전투하세요!" },
        
        // Island 2
        { PooriScriptType.Script_6_BridgeCollapsed,
            "헉! 다리가 무너졌어요!\n[ 유적 ]에 도착하면 섬의 에너지가 그 유적으로\n집중되기 때문에 이런 현상이 발생해요" },

        { PooriScriptType.Script_7_WeaponInventory,
            "만약 무기를 바꾸고 싶다면\n[ 숫자 1, 2, 3 ]을 눌러서 바꿀 수 있어요!\n" +
            "혹시 공간이 모자라다면\n[ G ]를 눌러 무기를 내려놓으세요" },

        { PooriScriptType.Script_8_WhenYouFoundSecretWay,
            "우와! 여기 이런 길이 있었네요?\n저도 처음 보는 신기한 길이에요!" },

        { PooriScriptType.Script_9_OnLeafs,
            "여긴 꽤 높네요\n\n어? 저 아래 반짝이는 건 무기일까요?" },

        { PooriScriptType.Script_10_WhenYouFallDownToBottom,
            "다리가 좁긴 했지만\n떨어질 정도는 아니었던 거 같은데..\n\n실수하신거죠? 하하!" },

        { PooriScriptType.Script_11_FirstMeetWithTurret,
            "저 돌탑은 가까이 가면 공격하는 포탑이에요\n\n절~대로 파괴되지 않으니 조심해서 피해가세요!" },

        { PooriScriptType.Script_12_ZommInOutTutorial,
            "[ 마우스 휠 ]을 움직여서\n크게 보거나 작게 볼 수 있어요" },

        { PooriScriptType.Script_13_FirstWave,
            "길을 막고 있는 돌이 적을 부르는\n[ 마법석 ] 이에요\n[ 마법석 ]을 부수면 더 이상\n적들이 오지 않을 거에요!" },

        { PooriScriptType.Script_14_BridgeWave,
            "적들이 다리가 연결되는 것을 방해하고 있어요!\n적들을 물리쳐서 다리를 완성시켜 주세요" },

        // Island 3
        { PooriScriptType.Script_15_WhenYouExitTheCave,
            "여기는 좀 어둡네요…\n분위기가 음침해요" },

        { PooriScriptType.Script_16_AfterBossCameOut,
            "스리핏이 관문을 지키고 있어요.\n쓰러뜨리면 관문이 열릴거에요." },
    };

    public static string GetPooriScript(this PooriScriptType pooriScriptType)
    {
        return mPooriScriptTable.TryGetValue(pooriScriptType, out var name) ? name : GlobalDefaultString.DefaultScriptText;
    }
}

public static class GlobalDefaultString
{
    public const string DefaultUsername = "무척추영준";
    public const string DefaultEntityName = "척추없는 영준";
    public const string DefaultItemName = "영준의 척추";
    public const string DefaultItemAdditionalInfo = "영준의 척추로 이루어진 무기";
    public const string DefaultAreaName = "영준의 집";
    public const string DefaultStageName = "영준의 둥지";
    public const string DefaultScriptText = "안녕? 난 영준이야.";
}

public static class Global
{
    public static readonly int LayerIndex_Entity = LayerMask.NameToLayer("Entity"); // Entities include mob
    public static readonly int LayerIndex_Weapon = LayerMask.NameToLayer("Weapon"); // Weapon on the ground, just like prop
    public static readonly int LayerIndex_Detector = LayerMask.NameToLayer("Detector"); // Projectiles
    public static readonly int LayerIndex_Ground = LayerMask.NameToLayer("Ground"); // Environment background
    public static readonly int LayerIndex_Deactivated = LayerMask.NameToLayer("Deactivated"); // Deactivated object, no collision detection occur
}

public static class GlobalSceneName
{
    public static readonly string TitleSceneName = "Title";
    public static readonly string LobbySceneName = "Lobby";
    public static readonly string ClientSceneName = "Client";
    public static readonly string ServerSceneName = "Server";
    public static readonly string GamePlayScene = "FinalStage";
    public static readonly string VictoryCreditScene = "Credit";

    public static readonly string TestPlayScene = "StageT01_Resource";
}

#if UNITY_EDITOR

public static class GlobalPath
{
    public static readonly string PrefabPath = $"{Application.dataPath}/CKC2022/Prefabs";

    public static readonly string MasterSuffix = "Master";
    public static readonly string RemoteSuffix = "Remote";

    public static readonly string ItemSuffix = "Item";

    public static readonly string DataPathOnEnvironment = Application.dataPath.Replace('/', '\\');

    public static readonly string DetectorPrefabsPath = @$"{DataPathOnEnvironment}\CKC2022\Prefabs\Detectors";
    public static readonly string EnemyPrefabsPath = $@"{DataPathOnEnvironment}\CKC2022\\Prefabs\Entities\Enemy";
    public static readonly string HumanoidPrefabsPath = $@"{DataPathOnEnvironment}\CKC2022\Prefabs\Entities\Humanoid";
    public static readonly string WeaponPrefabsPath = $@"{DataPathOnEnvironment}\CKC2022\Prefabs\Entities\Weapon";
    public static readonly string StructurePrefabsPath = $@"{DataPathOnEnvironment}\CKC2022\Prefabs\Entities\Structure\";
    public static readonly string ItemPrefabsPath = $@"{DataPathOnEnvironment}\CKC2022\Prefabs\Items";

    /// <summary>해당 위치의 Prefab을 불러옵니다.</summary>
    /// <param name="folderPathInAsset">폴더 위치</param>
    /// <param name="containContext">포함되어야 하는 문자열</param>
    /// <returns>Prefab 리스트</returns>
    public static List<GameObject> GetPrefabsFileFromPath(string folderPathInAsset, string containContext = null)
    {
        return GetPrefabsFileFromPath(folderPathInAsset, new List<string> { containContext });
    }

    /// <summary>해당 위치의 Prefab을 불러옵니다.</summary>
    /// <param name="folderPathInAsset">폴더 위치</param>
    /// <param name="containContext">포함되어야 하는 문자열 리스트</param>
    /// <returns>Prefab 리스트</returns>
    public static List<GameObject> GetPrefabsFileFromPath(string folderPathInAsset, List<string> containContext = null)
    {
        var filePaths = Directory.GetFiles(folderPathInAsset);
        List<GameObject> loadedPrefabs = new List<GameObject>();

        foreach (string filePath in filePaths)
        {
            bool isNotMatch = false;

            if (containContext != null)
            {
                foreach (string context in containContext)
                {
                    if (!string.IsNullOrEmpty(context) && filePath.ToLower().Contains(context.ToLower()) == false)
                    {
                        isNotMatch = true;
                        break;
                    }
                }
            }

            if (isNotMatch)
                continue;

            string currentFilePath = filePath.Replace(DataPathOnEnvironment, "Assets");

            if (IsPrefabFile(currentFilePath) && TryLoadPrefabFromFilePath(currentFilePath, out var loadedPrefab))
            {
                loadedPrefabs.Add(loadedPrefab);
            }
        }

        return loadedPrefabs;
    }

    public static bool TryLoadPrefabFromFilePath(string prefabFilePath, out GameObject prefabObject)
    {
        prefabObject = AssetDatabase.LoadAssetAtPath(prefabFilePath, typeof(GameObject)) as GameObject;
        return prefabObject != null;
    }

    public static bool IsPrefabFile(string prefabFilePath)
    {
        string fileExtension = Path.GetExtension(prefabFilePath).ToLower();
        return fileExtension == ".prefab";
    }
}

#endif