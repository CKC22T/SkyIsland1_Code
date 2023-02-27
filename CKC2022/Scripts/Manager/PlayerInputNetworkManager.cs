using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Network.Packet;
using Utils;
using static Network.Packet.Request.Types;
using System;
using System.Linq;
using Network.Client;
using Network;

namespace CKC2022.Input
{
    public class PlayerInputNetworkManager : MonoSingleton<PlayerInputNetworkManager>
    {
        private readonly Dictionary<int, InputContainer> containersByPlayerID = new();
        private readonly List<HumanoidPlayerInput> InitializableInputs = new();
        private readonly List<ReplicatedHumanoidEntityInteractor> InitializableInteractors = new();

        public event Action<InputContainer> OnEntityBindingChanged;

        private CoroutineWrapper inputSenderWrapper;
        private CoroutineWrapper currentStateSenderWrapper;
        private int sendIndex;

        private CoroutineWrapper delaySender;
        public CoroutineWrapper DelaySender
        {
            get
            {
                if (delaySender == null)
                    delaySender = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);

                return delaySender;
            }
        }

        public bool isRunning { get; private set; }

        public static bool TryGetInputContainer(in int playerID, out InputContainer container)
        {
            return Instance.containersByPlayerID.TryGetValue(playerID, out container);
        }

        [Obsolete("When playerID(controllerID) had Implemented, use TryGetInputContainer instead")]
        public static bool TryGetAnyInputContainer(out InputContainer container)
        {
            container = Instance.containersByPlayerID.Values.FirstOrDefault();
            return Instance.containersByPlayerID.Count > 0;
        }

        public void AddInitializeInvocation(in HumanoidPlayerInput input)
        {
            if (isRunning)
            {
                input.Initialize(ClientSessionManager.Instance.SessionID);
                BindInput(ClientSessionManager.Instance.SessionID, input);
            }
            else
            {
                InitializableInputs.Add(input);
            }
        }

        public void AddInitializeInvocation(in ReplicatedHumanoidEntityInteractor interactor)
        {
            if (isRunning)
            {
                var container = BindInput(ClientSessionManager.Instance.SessionID, interactor);
                interactor.Initialize(container);
            }
            else
            {
                InitializableInteractors.Add(interactor);
            }
        }

        //call once
        protected override void Initialize()
        {
            base.Initialize();

            ClientSessionManager.OnInitialized += ClientSessionManager_OnInitialized;

            inputSenderWrapper = new CoroutineWrapper(this);
            currentStateSenderWrapper = new CoroutineWrapper(this);
        }

        //call once
        private void ClientSessionManager_OnInitialized(ClientSessionManager clientSessionManager)
        {
            // fafsafawf
            
            clientSessionManager.OnDisconnected += Release;
            clientSessionManager.OnGameStart += Instance_OnGameStart;
        }

        private void Instance_OnGameStart(int sessionID)
        {
            if (!isRunning)
            {
                containersByPlayerID.Clear();

                foreach (var invocation in InitializableInputs)
                {
                    invocation.Initialize(sessionID);
                    BindInput(ClientSessionManager.Instance.SessionID, invocation);
                }

                foreach(var interactor in InitializableInteractors)
                {
                    var container = BindInput(ClientSessionManager.Instance.SessionID, interactor);
                    interactor.Initialize(container);
                }

                isRunning = true;
                InitializableInputs.Clear();
                InitializableInteractors.Clear();
            }

            inputSenderWrapper.StartSingleton(SendInput());
            //currentStateSenderWrapper.StartSingleton(SendCurrentInputState());
        }

        public InputContainer BindInput(in int playerID, HumanoidPlayerInput input)
        {
            Debug.Log($"[Input] input is Binded. input is ({input}), player ID is ({playerID})");
            
            var container = GetInitializedContainer(playerID);
            container.BindInput(input);

            return container;
        }

        public InputContainer BindInput(in int playerID, in ReplicatedHumanoidEntityInteractor interactor)
        {
            Debug.Log($"[Input] interactor is Binded. interactor is ({interactor.gameObject.name}), player ID is ({playerID})");

            var container = GetInitializedContainer(playerID);
            var changed = container.BindInteractor(interactor);

            if (changed)
                OnEntityBindingChanged?.Invoke(container);
            
            return container;
        }

        public void ReleaseInteractor(in ReplicatedHumanoidEntityInteractor interactor)
        {
            foreach (var container in containersByPlayerID.Values)
            {
                if (container.Interactor == interactor)
                {
                    container.BindInteractor(null);
                    OnEntityBindingChanged?.Invoke(container);
                }
            }
        }

        private InputContainer GetInitializedContainer(in int playerID)
        {
            if (!containersByPlayerID.TryGetValue(playerID, out var container))
            {
                container = new InputContainer();
                container.OnAttack += SendAttack;
                container.OnRevive += SendRevive;
                container.OnJump += SendJump;
                container.OnWeaponSelected += SendEquipWeapon;
                container.OnCheckPointInteract += SendCheckPointInteract;
                container.OnWeaponReleased += SendReleaseWeapon;
                container.OnWeaponSwapSelected += SendWeaponSwapSelected;
                containersByPlayerID.Add(playerID, container);
            }

            return container;
        }


        //Send Immediately
        private void SendAttack(InputContainer container, bool isDown)
        {
            if (!isDown)
                return;

            //allowed
            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            var use_weapon_data_b = UseWeaponData.CreateBuilder()
                .SetOrigin(container.Interactor.BindingTarget.transform.position.ToData())
                .SetDirection(container.RawViewVector.Value.ToData());

            var input_data_b = InputData.CreateBuilder()
                .SetUseWeaponData(use_weapon_data_b);

            var request_b = networkService.GetRequestBuilder(RequestHandle.kUpdateInput)
                .SetUpdateInput(input_data_b);

            //networkService.SendToServerViaUdp(request_b.Build());
            //networkService.SendToServerViaUdp(request_b.Build());
            //networkService.SendToServerViaUdp(request_b.Build());

            DelaySender.Start(DelaySend(request_b.Build(), container.Interactor.DetectorCreationDelay));

            //complete
            container.Interactor.SetWeaponUsedTime();
        }

        IEnumerator DelaySend(Request request, float delay)
        {
            if (delay > 0f)
                yield return YieldInstructionCache.WaitForSeconds(delay);

            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            networkService.SendToServerViaUdp(request);
            networkService.SendToServerViaUdp(request);
            networkService.SendToServerViaUdp(request);

            yield break;
        }


        //Send Immediately
        private void SendRevive(InputContainer container, bool isDown)
        {
            if (!isDown)
                return;

            //if (!ClientNetworkService.TryGetInstance(out var networkService))
            //    return;

            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            var inputDataBuilder = InputData.CreateBuilder()
                .SetReviveWish(true);

            var requestBuilder = networkService.GetRequestBuilder(RequestHandle.kUpdateInput)
                .SetUpdateInput(inputDataBuilder);

            networkService.SendToServerViaTcp(requestBuilder.Build());
        }

        //Send Immediately
        private void SendJump(InputContainer container, bool isDown)
        {
            if (!isDown)
                return;

            //allowed
            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            var inputDataBuilder = InputData.CreateBuilder()
                .SetJumpWish(true);

            var requestBuilder = networkService.GetRequestBuilder(RequestHandle.kUpdateInput)
                .SetUpdateInput(inputDataBuilder);

            networkService.SendToServerViaUdp(requestBuilder.Build());
            networkService.SendToServerViaUdp(requestBuilder.Build());
            networkService.SendToServerViaUdp(requestBuilder.Build());

            //complete
            //container.Interactor.SetWeaponUsedTime();
        }

        private void SendEquipWeapon(InputContainer container, ReplicableItemObject obj)
        {
            SendEquipWeapon(obj.ItemObjectID);
        }

        /// <summary>Check Point의 무기 습득을 시도합니다.</summary>
        private void SendCheckPointInteract(InputContainer container, int checkPointNumber)
        {
            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            var request_b = networkService.GetRequestBuilder(RequestHandle.kRequestTryObtainCheckPointItem)
                .SetCheckPointNumber(checkPointNumber);

            networkService.SendToServerViaTcp(request_b.Build());
        }

        /// <summary>무기를 버립니다.</summary>
        private void SendReleaseWeapon(InputContainer container)
        {
            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            var request_b = networkService.GetRequestBuilder(RequestHandle.kRequestItemObjectDropWeapon);

            networkService.SendToServerViaTcp(request_b.Build());

            WeaponChangePopup.Instance.Open();
        }

        private void SendWeaponSwapSelected(int selection)
        {
            selection--;

            if (selection < 0 || selection >= ServerConfiguration.MAX_WEAPON_ITEM_INVENTORY)
            {
                selection = 0;
            }

            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            var requestSwap = networkService.GetRequestBuilder(RequestHandle.kRequestSwapWeapon)
                .SetSwapInventoryIndex(selection);

            if (ClientSessionManager.Instance.TryGetMyInventory(out var inventory))
            {
                if (selection < inventory.GetWeaponCount())
                {
                    WeaponChangePopup.Instance.Open(selection.ToString());
                }
            }

            networkService.SendToServerViaTcp(requestSwap.Build());
        }

        private void SendEquipWeapon(in int itemObjectID)
        {
            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            var obtain_b = RequestItemObjectObtainData.CreateBuilder()
                .SetItemObjectId(itemObjectID);

            var request_b = networkService.GetRequestBuilder(RequestHandle.kRequestItemObjectObtainData)
                .SetRequestItemObjectObtainData(obtain_b);

            networkService.SendToServerViaTcp(request_b.Build());
        }

        //Send per tick
        IEnumerator SendInput()
        {
            yield return new WaitForEndOfFrame();

            while (isRunning)
            {
                foreach (var container in containersByPlayerID.Values)
                {
                    SendData(container);
                }

                yield return YieldInstructionCache.WaitForSeconds(Network.ServerConfiguration.SERVER_TICK_DELTA_TIME);
            }
        }

        IEnumerator SendCurrentInputState()
        {
            yield return new WaitForEndOfFrame();

            while (isRunning)
            {
                foreach (var container in containersByPlayerID.Values)
                {
                    SendDataForce(container);
                }

                yield return YieldInstructionCache.WaitForSeconds(Network.ServerConfiguration.SERVER_NETWORK_TICK);
            }
        }

        private void SendData(in InputContainer container)
        {
            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            var input_b = InputData.CreateBuilder();

            //if (container.CharacterLookDirection.GetDirtyAndClear(out var view))
            //{
            //    if (view.magnitude > Vector2.kEpsilon)
            //        input_b.SetViewDirection(view.ToData());
            //}

            //if (container.CameraSpaceMovementDirection.GetDirtyAndClear(out var moveDir))
            //    input_b.SetMovementDirection(moveDir.ToData());

            //// 전송할 데이터가 없으면 전송하지 않음.
            //if (input_b.HasViewDirection == false && input_b.HasMovementDirection == false)
            //    return;

            // 강제 전송

            input_b.SetViewDirection(container.CharacterLookDirection.Value.ToData());
            input_b.SetMovementDirection(container.CameraSpaceMovementDirection.Value.ToData());

            var request_b = networkService.GetRequestBuilder(RequestHandle.kUpdateInput)
                .SetUpdateInput(input_b);

            networkService.SendToServerViaUdp(request_b.Build());
            //networkService.SendToServerViaUdp(request_b.Build());
        }

        private void SendDataForce(in InputContainer container)
        {
            ClientNetworkManager networkService = ClientNetworkManager.Instance;

            var input_b = InputData.CreateBuilder()
                .SetMovementDirection(container.CameraSpaceMovementDirection.Value.ToData());

            if (container.CharacterLookDirection.Value.magnitude > Vector2.kEpsilon)
                input_b.SetViewDirection(container.CharacterLookDirection.Value.ToData());


            var request_b = networkService.GetRequestBuilder(RequestHandle.kUpdateInput)
                .SetUpdateInput(input_b);

            networkService.SendToServerViaUdp(request_b.Build());
        }

        public void Release()
        {
            isRunning = false;

            inputSenderWrapper.Stop();
            currentStateSenderWrapper.Stop();
        }
    }
}