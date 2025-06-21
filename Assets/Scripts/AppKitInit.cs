using Reown.AppKit.Unity;
using Reown.AppKit.Unity.Model;
using Reown.Core.Common.Logging;
using UnityEngine;
using UnityLogger = Reown.Sign.Unity.UnityLogger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sample
{
    public class AppKitInit : MonoBehaviour
    {
        public string WalletAddress { get; private set; }
        public static event Action OnAppKitInitialized;

        [Header("UI")]
        public GameObject walletWaitPanel;

        private static bool _isInitializing = false;
        private static bool walletPanelHasBeenHidden = false;
        private static bool gameOverHasBeenTriggered = false;

        [Header("Scene Management")]
        [SerializeField] private bool shouldSwitchScene = false;
        [SerializeField] private string targetSceneName = "";

        [Header("Interaction Management")]
        [SerializeField] private bool disableInteractionsOnModal = true;
        [SerializeField] private string[] interactionScriptNames = { "PlayerController" }; // Ajoute ici tous tes scripts d'interaction joueur
        [SerializeField] private float checkInterval = 0.2f;

        private List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>();
        private bool isModalActive = false;
        private Coroutine modalCheckCoroutine;
        private Coroutine walletCheckCoroutine;

        private float walletDisconnectedTime = -1f;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SetWalletAddressJS(string wallet);
#endif

        private void Start()
        {
            ReownLogger.Instance = new UnityLogger();

            if (AppKit.IsInitialized && AppKit.IsAccountConnected && walletWaitPanel != null)
                walletWaitPanel.SetActive(false);

            StartCoroutine(InitializeAppKitWithRetry());
            walletCheckCoroutine = StartCoroutine(WalletPanelCheckRoutine());

            // GameOver auto si panel affiché au lancement
            StartCoroutine(AutoGameOverIfPanel());
        }

        private void Update()
        {
            // Désactive les scripts d'interaction joueur tant que le panel wallet est affiché
            if (walletWaitPanel != null && walletWaitPanel.activeSelf)
            {
                foreach (var script in FindObjectsOfType<MonoBehaviour>())
                {
                    if (script == null) continue;
                    foreach (var target in interactionScriptNames)
                        if (script.GetType().Name == target && script.enabled)
                            script.enabled = false;
                }
            }
            else
            {
                foreach (var script in FindObjectsOfType<MonoBehaviour>())
                {
                    if (script == null) continue;
                    foreach (var target in interactionScriptNames)
                        if (script.GetType().Name == target && !script.enabled)
                            script.enabled = true;
                }
            }
        }

        private IEnumerator AutoGameOverIfPanel()
        {
            yield return new WaitForSeconds(0.1f);
            if (walletWaitPanel != null && walletWaitPanel.activeSelf)
            {
                var gm = GameManager.Instance;
                if (gm != null && !gm.IsGameOver)
                    gm.GameOver();
            }
        }

        private IEnumerator WalletPanelCheckRoutine()
        {
            while (true)
            {
                if (AppKit.IsInitialized && AppKit.IsAccountConnected)
                {
                    var task = AppKit.GetAccountAsync();
                    while (!task.IsCompleted) yield return null;

                    if (!task.IsFaulted && task.Result != null && !string.IsNullOrEmpty(task.Result.Address))
                    {
                        WalletAddress = task.Result.Address;
                        walletDisconnectedTime = -1f;
                        Debug.Log("[AppKitInit] WalletAddress récupéré: " + WalletAddress);
                        SetWalletOnWindow(WalletAddress);

                        if (walletWaitPanel != null && walletWaitPanel.activeSelf && !walletPanelHasBeenHidden)
                        {
                            walletWaitPanel.SetActive(false);
                            walletPanelHasBeenHidden = true;
                            Debug.Log("[AppKitInit] Panel caché automatiquement (wallet connecté)");

                            if (!gameOverHasBeenTriggered)
                            {
                                var gm = GameManager.Instance;
                                if (gm != null)
                                    gm.GameOver();
                                gameOverHasBeenTriggered = true;
                            }
                        }

                        var myScoreMgr = FindObjectOfType<MyScoreManager>();
                        if (myScoreMgr != null)
                            myScoreMgr.RequestMyScores();

                        OnAppKitInitialized?.Invoke();
                        yield break;
                    }
                }
                else
                {
                    if (walletDisconnectedTime < 0f)
                        walletDisconnectedTime = Time.time;

                    if (Time.time - walletDisconnectedTime > 1f)
                    {
                        if (walletWaitPanel != null && !walletWaitPanel.activeSelf)
                        {
                            walletWaitPanel.SetActive(true);
                            Debug.Log("[AppKitInit] Panel réaffiché (wallet déconnecté > 1s)");
                        }
                        walletPanelHasBeenHidden = false;
                        gameOverHasBeenTriggered = false;
                    }
                }
                yield return new WaitForSeconds(0.2f);
            }
        }

        private void SetWalletOnWindow(string wallet)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SetWalletAddressJS(wallet);
#endif
        }

        public static void TryInitialize()
        {
            if (AppKit.IsInitialized || _isInitializing) return;
            var inst = FindObjectOfType<AppKitInit>();
            if (inst != null) inst.StartCoroutine(inst.InitializeAppKitWithRetry());
        }

        private IEnumerator InitializeAppKitWithRetry()
        {
            if (_isInitializing || AppKit.IsInitialized) yield break;
            _isInitializing = true;

            var monadTestnet = new Chain(
                ChainConstants.Namespaces.Evm,
                chainReference: "10143",
                name: "Monad Testnet",
                nativeCurrency: new Currency("Monad", "MON", 18),
                blockExplorer: new BlockExplorer("Monad Explorer", "https://explorer.testnet.monad.xyz"),
                rpcUrl: "https://rpc.testnet.monad.xyz/",
                isTestnet: true,
                imageUrl: "https://raw.githubusercontent.com/RedGnad/pokenads/master/pokenads-logo8.png"
            );

            var cfg = new AppKitConfig
            {
                projectId = "27f51a8cead380193aaf687f55e3d4af",
                metadata = new Metadata(
                    "Pokenads",
                    "AppKit Unity Sample - Monad Testnet",
                    "https://pokenads-c58e5.web.app",
                    "https://raw.githubusercontent.com/RedGnad/pokenads/master/pokenads-logo8.png",
                    new RedirectData { Native = "appkit-sample-unity://" }
                ),
                customWallets = GetCustomWallets(),
                connectViewWalletsCountMobile = 6, // Augmenté de 5 à 6 pour afficher plus de wallets
                supportedChains = new[] { monadTestnet },
                socials = new[]
                {
                    SocialLogin.Google,
                    SocialLogin.X,
                    SocialLogin.Discord,
                    SocialLogin.Apple,
                    SocialLogin.GitHub
                }
            };

            const int MAX_ATTEMPTS = 10;
            int attempts = 0;

            while (!AppKit.IsInitialized && attempts < MAX_ATTEMPTS)
            {
                attempts++;
                Debug.Log($"[AppKitInit] Tentative d'initialisation {attempts}/{MAX_ATTEMPTS}...");
                var initTask = AppKit.InitializeAsync(cfg);

                float timer = 0f;
                while (!initTask.IsCompleted && timer < 5f)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (initTask.IsCompleted && !initTask.IsFaulted)
                {
                    Debug.Log("[AppKitInit] Initialisation réussie !");
                    AppKit.AccountConnected += OnWalletEvent;
                    AppKit.AccountDisconnected += OnWalletEvent;
                    if (disableInteractionsOnModal)
                        StartModalCheck();

                    if (shouldSwitchScene && Application.CanStreamedLevelBeLoaded(targetSceneName))
                        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);

                    break;
                }

                yield return new WaitForSeconds(1f);
            }

            if (!AppKit.IsInitialized)
                Debug.LogError($"[AppKitInit] Échec après {MAX_ATTEMPTS} tentatives.");

            _isInitializing = false;
        }

        private void OnWalletEvent(object sender, System.EventArgs e)
        {
            isModalActive = false;
            EnableAllInteractions();
        }

        private void StartModalCheck()
        {
            if (modalCheckCoroutine != null) StopCoroutine(modalCheckCoroutine);
            modalCheckCoroutine = StartCoroutine(CheckModalRoutine());
        }

        private IEnumerator CheckModalRoutine()
        {
            while (true)
            {
                bool modalDetected = IsModalVisible();
                if (modalDetected != isModalActive)
                {
                    isModalActive = modalDetected;
                    if (modalDetected) DisableAllInteractions(); else EnableAllInteractions();
                }
                yield return new WaitForSeconds(checkInterval);
            }
        }

        private bool IsModalVisible()
        {
            var modal = GameObject.Find("AppKit_ModalContainer");
            if (modal != null && modal.activeInHierarchy) return true;
            foreach (var c in FindObjectsOfType<Canvas>())
                if ((c.name.ToLower().Contains("modal") || c.name.ToLower().Contains("wallet")) && c.gameObject.activeInHierarchy)
                    return true;
            return false;
        }

        private void DisableAllInteractions()
        {
            disabledComponents.Clear();
            foreach (var script in FindObjectsOfType<MonoBehaviour>())
            {
                if (script == null) continue;
                var name = script.GetType().Name;
                foreach (var target in interactionScriptNames)
                    if (name == target && script.enabled)
                    {
                        script.enabled = false;
                        disabledComponents.Add(script);
                        break;
                    }
            }
        }

        private void EnableAllInteractions()
        {
            foreach (var script in disabledComponents)
                if (script != null) script.enabled = true;
            disabledComponents.Clear();
        }

        private void OnDestroy()
        {
            if (AppKit.IsInitialized)
            {
                AppKit.AccountConnected    -= OnWalletEvent;
                AppKit.AccountDisconnected -= OnWalletEvent;
            }
            if (modalCheckCoroutine != null) StopCoroutine(modalCheckCoroutine);
            if (walletCheckCoroutine != null) StopCoroutine(walletCheckCoroutine);
            EnableAllInteractions();
        }

        private Wallet[] GetCustomWallets()
        {
            // Détecte si on est sur mobile (natif ou WebGL mobile)
            bool isMobile = Application.isMobilePlatform || 
                           (Application.platform == RuntimePlatform.WebGLPlayer && 
                            SystemInfo.deviceType == DeviceType.Handheld);

            if (isMobile)
            {
                // Sur mobile, ajoute Backpack et HAHA aux wallets par défaut
                return new[]
                {
                    new Wallet 
                    { 
                        Name = "Backpack", 
                        ImageUrl = "https://backpack.app/favicon.ico", 
                        MobileLink = "backpack://",
                        WebappLink = "https://backpack.app/"
                    },
                    new Wallet 
                    { 
                        Name = "HAHA", 
                        ImageUrl = "https://raw.githubusercontent.com/RedGnad/pokenads/master/pokenads-logo8.png", // Remplace par l'URL HAHA si différente
                        MobileLink = "haha://", // Si HAHA a des deep links
                        WebappLink = "https://haha-wallet-url/" // Remplace par l'URL HAHA
                    }
                };
            }
            else
            {
                // Sur desktop, garde le comportement original (null = wallets par défaut)
                return null;
            }
        }
    }
}