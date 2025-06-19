using System;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Reown.AppKit.Unity;
using Reown.AppKit.Unity.Profile;
using Reown.Core;
using Reown.Core.Common.Model.Errors;
using UnityEngine;
using UnityEngine.UIElements;
using ButtonUtk = UnityEngine.UIElements.Button;

namespace Sample
{
    public class Dapp : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        private int _messageCounter = 0;

        private ButtonStruct[] _buttons;
        private VisualElement _buttonsContainer;

        private void Awake()
        {
            Debug.Log("[Dapp] Awake called");

            Application.targetFrameRate = Screen.currentResolution.refreshRate;

            if (_uiDocument == null)
            {
                Debug.LogError("[Dapp] _uiDocument n'est pas assigné !");
                return;
            }
            Debug.Log("[Dapp] _uiDocument assigné : " + _uiDocument.name);

            if (!_uiDocument.isActiveAndEnabled)
            {
                Debug.LogError("[Dapp] _uiDocument n'est pas actif au moment de Awake !");
                return;
            }

            if (_uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[Dapp] rootVisualElement est null dans Awake !");
                return;
            }
            Debug.Log("[Dapp] rootVisualElement OK");

            _buttonsContainer = _uiDocument.rootVisualElement.Q<VisualElement>("ButtonsContainer");
            if (_buttonsContainer == null)
            {
                Debug.LogError("[Dapp] ButtonsContainer introuvable dans le UXML !");
                return;
            }
            Debug.Log("[Dapp] ButtonsContainer trouvé");

            BuildButtons();
            Debug.Log("[Dapp] BuildButtons terminé");
        }

        private void BuildButtons()
        {
            Debug.Log("[Dapp] BuildButtons appelé");
            _buttons = new[]
            {
                new ButtonStruct
                {
                    Text = "Connect",
                    OnClick = OnConnectButton,
                    AccountRequired = false
                },
                new ButtonStruct
                {
                    Text = "Network",
                    OnClick = OnNetworkButton
                },
                new ButtonStruct
                {
                    Text = "Account",
                    OnClick = OnAccountButton,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Personal Sign",
                    OnClick = OnPersonalSignButton,
                    AccountRequired = true
                },
                // new ButtonStruct
                // {
                //     Text = "Sign Typed Data",
                //     OnClick = OnSignTypedDataV4Button,
                //     AccountRequired = true
                // },
                new ButtonStruct
                {
                    Text = "Send Transaction",
                    OnClick = OnSendTransactionButton,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Get Balance",
                    OnClick = OnGetBalanceButton,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Read Contract",
                    OnClick = OnReadContractClicked,
                    ChainIds = new HashSet<string>
                    {
                        "eip155:10"
                    }
                },
                new ButtonStruct
                {
                    Text = "Disconnect",
                    OnClick = OnDisconnectButton,
                    AccountRequired = true
                }
            };
        }

        private void RefreshButtons()
        {
            Debug.Log("[Dapp] RefreshButtons appelé");
            if (_buttonsContainer == null)
            {
                Debug.LogError("[Dapp] _buttonsContainer est null dans RefreshButtons !");
                return;
            }

            _buttonsContainer.Clear();

            foreach (var button in _buttons)
            {
                if (button.ChainIds != null && !button.ChainIds.Contains(AppKit.NetworkController?.ActiveChain?.ChainId))
                {
                    Debug.Log($"[Dapp] Bouton {button.Text} masqué (chainId non supporté)");
                    continue;
                }

                var buttonUtk = new ButtonUtk
                {
                    text = button.Text
                };
                buttonUtk.clicked += button.OnClick;

                if (button.AccountRequired.HasValue)
                {
                    switch (button.AccountRequired)
                    {
                        case true when !AppKit.IsAccountConnected:
                            buttonUtk.SetEnabled(false);
                            Debug.Log($"[Dapp] Bouton {button.Text} désactivé (compte non connecté)");
                            break;
                        case true when AppKit.IsAccountConnected:
                            buttonUtk.SetEnabled(true);
                            Debug.Log($"[Dapp] Bouton {button.Text} activé (compte connecté)");
                            break;
                        case false when AppKit.IsAccountConnected:
                            buttonUtk.SetEnabled(false);
                            Debug.Log($"[Dapp] Bouton {button.Text} désactivé (compte connecté)");
                            break;
                        case false when !AppKit.IsAccountConnected:
                            buttonUtk.SetEnabled(true);
                            Debug.Log($"[Dapp] Bouton {button.Text} activé (compte non connecté)");
                            break;
                    }
                }

                _buttonsContainer.Add(buttonUtk);
                Debug.Log($"[Dapp] Bouton {button.Text} ajouté à l'UI");
            }
        }

        private async void Start()
        {
            Debug.Log("[Dapp] Start appelé");

            if (!AppKit.IsInitialized)
            {
                Debug.LogWarning("[Dapp] AppKit is not initialized. Please initialize AppKit first.");
                Notification.ShowMessage("AppKit is not initialized. Please initialize AppKit first.");
                return;
            }

            Debug.Log("[Dapp] AppKit est initialisé, appel RefreshButtons");
            RefreshButtons();

            try
            {
                AppKit.ChainChanged += (_, e) =>
                {
                    Debug.Log("[Dapp] AppKit.ChainChanged event");
                    RefreshButtons();

                    if (e.NewChain == null)
                    {
                        Notification.ShowMessage("Unsupported chain");
                        Debug.LogWarning("[Dapp] Unsupported chain");
                        return;
                    }
                };

                AppKit.AccountConnected += (_, e) =>
                {
                    Debug.Log("[Dapp] AppKit.AccountConnected event");
                    RefreshButtons();
                };

                AppKit.AccountDisconnected += (_, _) =>
                {
                    Debug.Log("[Dapp] AppKit.AccountDisconnected event");
                    RefreshButtons();
                };

                AppKit.AccountChanged += (_, e) =>
                {
                    Debug.Log("[Dapp] AppKit.AccountChanged event");
                    RefreshButtons();
                };

                AppKit.NetworkController.ChainChanged += (_, e) =>
                {
                    Debug.Log("[Dapp] NetworkController.ChainChanged event");
                    RefreshButtons();
                };

                Debug.Log("[Dapp] Tentative de reprise de session AppKit...");
                var sessionResumed = await AppKit.ConnectorController.TryResumeSessionAsync();
                Debug.Log($"[Dapp] Session resumed: {sessionResumed}");
            }
            catch (Exception e)
            {
                Notification.ShowMessage(e.Message);
                Debug.LogError("[Dapp] Exception dans Start: " + e);
                throw;
            }
        }

        public void OnConnectButton()
        {
            Debug.Log("[Dapp] OnConnectButton appelé");
            AppKit.OpenModal();
        }

        public void OnNetworkButton()
        {
            Debug.Log("[Dapp] OnNetworkButton appelé");
            AppKit.OpenModal(ViewType.NetworkSearch);
        }

        public void OnAccountButton()
        {
            Debug.Log("[Dapp] OnAccountButton appelé");
            AppKit.OpenModal(ViewType.Account);
        }

        public async void OnGetBalanceButton()
        {
            Debug.Log("[Dapp] OnGetBalanceButton appelé");

            try
            {
                Notification.ShowMessage("Getting balance with WalletConnect Blockchain API...");

                var account = await AppKit.GetAccountAsync();
                Debug.Log($"[Dapp] Account récupéré: {(account != null ? account.Address : "null")}");

                var balance = await AppKit.Evm.GetBalanceAsync(account.Address);
                Debug.Log($"[Dapp] Balance récupérée: {balance}");

                Notification.ShowMessage($"Balance: {Web3.Convert.FromWei(balance)} ETH");
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"{nameof(RpcResponseException)}:\n{e.Message}");
                Debug.LogError("[Dapp] Exception dans OnGetBalanceButton: " + e);
            }
        }

        public async void OnPersonalSignButton()
        {
            Debug.Log("[Dapp] OnPersonalSignButton appelé");

            var messageCounter = ++_messageCounter;
            try
            {
                var account = await AppKit.GetAccountAsync();
                Debug.Log($"[Dapp] Account récupéré: {(account != null ? account.Address : "null")}");

                var message = $"Hello from Unity! (Request #{messageCounter})";

                Notification.ShowMessage($"Signing message:\n\n{message}");

#if !UNITY_WEBGL || !UNITY_EDITOR
                // await System.Threading.Tasks.Task.Delay(1_000);
#endif

                var signature = await AppKit.Evm.SignMessageAsync(message);
                Debug.Log($"[Dapp] Signature reçue: {signature}");
                var isValid = await AppKit.Evm.VerifyMessageSignatureAsync(account.Address, message, signature);
                Debug.Log($"[Dapp] Signature validée: {isValid}");

                Notification.ShowMessage($"Signature valid: {isValid} (Request #{messageCounter})");
            }
            catch (ReownNetworkException e)
            {
                Notification.ShowMessage($"Error processing personal_sign request #{messageCounter}\n\n{nameof(RpcResponseException)}:\n{e.Message}");
                Debug.LogError("[Dapp] Exception dans OnPersonalSignButton: " + e);
            }
        }

        public async void OnDisconnectButton()
        {
            Debug.Log("[Dapp] OnDisconnectButton appelé");

            try
            {
                Notification.ShowMessage($"Disconnecting...");
                await AppKit.DisconnectAsync();
                Debug.Log("[Dapp] Déconnexion réussie");
                Notification.Hide();
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"{e.GetType()}:\n{e.Message}");
                Debug.LogError("[Dapp] Exception dans OnDisconnectButton: " + e);
            }
        }

        public async void OnSendTransactionButton()
        {
            Debug.Log("[Dapp] OnSendTransactionButton appelé");

            const string toAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";

            try
            {
                Notification.ShowMessage("Sending transaction...");

                var value = Web3.Convert.ToWei(0.001);
                var result = await AppKit.Evm.SendTransactionAsync(toAddress, value);
                Debug.Log("[Dapp] Transaction hash: " + result);

                Notification.ShowMessage("Transaction sent");
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"Error sending transaction.\n{e.Message}");
                Debug.LogError("[Dapp] Exception dans OnSendTransactionButton: " + e);
            }
        }

        public async void OnSignTypedDataV4Button()
        {
            Debug.Log("[Dapp] OnSignTypedDataV4Button appelé");

            Notification.ShowMessage("Signing typed data...");

            var account = await AppKit.GetAccountAsync();
            Debug.Log($"[Dapp] Account récupéré: {(account != null ? account.Address : "null")}");

            Debug.Log("[Dapp] Get mail typed definition");
            var typedData = GetMailTypedDefinition();
            var mail = new Mail
            {
                From = new Person
                {
                    Name = "Cow",
                    Wallets = new List<string>
                    {
                        "0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826",
                        "0xDeaDbeefdEAdbeefdEadbEEFdeadbeEFdEaDbeeF"
                    }
                },
                To = new List<Person>
                {
                    new()
                    {
                        Name = "Bob",
                        Wallets = new List<string>
                        {
                            "0xbBbBBBBbbBBBbbbBbbBbbbbBBbBbbbbBbBbbBBbB",
                            "0xB0BdaBea57B0BDABeA57b0bdABEA57b0BDabEa57",
                            "0xB0B0b0b0b0b0B000000000000000000000000000"
                        }
                    }
                },
                Contents = "Hello, Bob!"
            };

            var ethChainId = Utils.ExtractChainReference(account.ChainId);

            typedData.Domain.ChainId = BigInteger.Parse(ethChainId);
            typedData.SetMessage(mail);

            var jsonMessage = typedData.ToJson();

            try
            {
                var signature = await AppKit.Evm.SignTypedDataAsync(jsonMessage);
                Debug.Log($"[Dapp] Signature typed data: {signature}");

                var isValid = await AppKit.Evm.VerifyTypedDataSignatureAsync(account.Address, jsonMessage, signature);
                Debug.Log($"[Dapp] Signature typed data validée: {isValid}");

                Notification.ShowMessage($"Signature valid: {isValid}");
            }
            catch (Exception e)
            {
                Notification.ShowMessage("Error signing typed data");
                Debug.LogError("[Dapp] Exception dans OnSignTypedDataV4Button: " + e);
            }
        }

        public async void OnReadContractClicked()
        {
            Debug.Log("[Dapp] OnReadContractClicked appelé");

            const string contractAddress = "0x521B4C065Bbdbe3E20B3727340730936912DfA46";
            const string abi = "function supply() view returns (uint256)";

            Notification.ShowMessage("Reading smart contract state...");

            try
            {
                var staked = await AppKit.Evm.ReadContractAsync<BigInteger>(contractAddress, abi, "supply");
                Debug.Log($"[Dapp] Valeur supply récupérée: {staked}");
                var stakedFormated = Web3.Convert.FromWei(staked);
                var result = $"Total Tokens Staked:\n{stakedFormated:N0} WCT";

                Notification.ShowMessage(result);
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"Contract reading error.\n{e.Message}");
                Debug.LogError("[Dapp] Exception dans OnReadContractClicked: " + e);
            }
        }

        private TypedData<Domain> GetMailTypedDefinition()
        {
            Debug.Log("[Dapp] GetMailTypedDefinition appelé");
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = "Ether Mail",
                    Version = "1",
                    ChainId = 1,
                    VerifyingContract = "0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC"
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(Group), typeof(Mail), typeof(Person)),
                PrimaryType = nameof(Mail)
            };
        }

        public const string CryptoPunksAbi =
            @"[{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""stateMutability"":""view"",""type"":""function""}]";
    }

    internal struct ButtonStruct
    {
        public string Text;
        public Action OnClick;
        public bool? AccountRequired;
        public HashSet<string> ChainIds;
    }
}