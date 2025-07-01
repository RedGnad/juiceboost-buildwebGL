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
        [SerializeField] private string[] interactionScriptNames = { "PlayerController" };
        [SerializeField] private float checkInterval = 0.2f;

        private List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>();
        private bool isModalActive = false;
        private Coroutine modalCheckCoroutine;
        private Coroutine walletCheckCoroutine;
        private float walletDisconnectedTime = -1f;

        // ─── WebGL Mobile Detection & JS Plugin ───
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetUserAgent();
        [DllImport("__Internal")]
        private static extern void SetWalletAddressJS(string wallet);
#endif
        private bool IsWebGLMobile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                string ua = GetUserAgent();
                return ua.Contains("Mobile") || ua.Contains("Android")
                    || ua.Contains("iPhone") || ua.Contains("iPad");
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }
        // ────────────────────────────────────────────

        private void Start()
        {
            ReownLogger.Instance = new UnityLogger();

            if (AppKit.IsInitialized && AppKit.IsAccountConnected && walletWaitPanel != null)
                walletWaitPanel.SetActive(false);

            StartCoroutine(InitializeAppKitWithRetry());
            walletCheckCoroutine = StartCoroutine(WalletPanelCheckRoutine());
            StartCoroutine(AutoGameOverIfPanel());
        }

        private void Update()
        {
            bool panelUp = walletWaitPanel != null && walletWaitPanel.activeSelf;
            foreach (var script in FindObjectsOfType<MonoBehaviour>())
            {
                if (script == null) continue;
                foreach (var target in interactionScriptNames)
                    if (script.GetType().Name == target)
                        script.enabled = !panelUp;
            }
        }

        private IEnumerator AutoGameOverIfPanel()
        {
            yield return new WaitForSeconds(0.1f);
            if (walletWaitPanel != null && walletWaitPanel.activeSelf)
                GameManager.Instance?.GameOver();
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
#if UNITY_WEBGL && !UNITY_EDITOR
                        SetWalletAddressJS(WalletAddress);
#endif
                        if (walletWaitPanel != null && walletWaitPanel.activeSelf && !walletPanelHasBeenHidden)
                        {
                            walletWaitPanel.SetActive(false);
                            walletPanelHasBeenHidden = true;
                            if (!gameOverHasBeenTriggered)
                            {
                                GameManager.Instance?.GameOver();
                                gameOverHasBeenTriggered = true;
                            }
                        }
                        OnAppKitInitialized?.Invoke();
                        yield break;
                    }
                }
                else
                {
                    if (walletDisconnectedTime < 0f) walletDisconnectedTime = Time.time;
                    if (Time.time - walletDisconnectedTime > 1f && walletWaitPanel != null && !walletWaitPanel.activeSelf)
                    {
                        walletWaitPanel.SetActive(true);
                        walletPanelHasBeenHidden = false;
                        gameOverHasBeenTriggered = false;
                    }
                }
                yield return new WaitForSeconds(checkInterval);
            }
        }

        public static void TryInitialize()
        {
            if (AppKit.IsInitialized || _isInitializing) return;
            var inst = FindObjectOfType<AppKitInit>();
            if (inst != null) inst.StartCoroutine(inst.InitializeAppKitWithRetry());
        }

        private IEnumerator InitializeAppKitWithRetry()
        {
            bool isMobile = IsWebGLMobile();
            bool shouldReinit = AppKit.IsInitialized && isMobile;
            if (_isInitializing || (AppKit.IsInitialized && !shouldReinit))
                yield break;

            _isInitializing = true;

            var monadTestnet = new Chain(
                ChainConstants.Namespaces.Evm,
                "10143", "Monad Testnet",
                new Currency("Monad","MON",18),
                new BlockExplorer("Monad Explorer","https://explorer.testnet.monad.xyz"),
                "https://rpc.testnet.monad.xyz/",
                true,
                "https://raw.githubusercontent.com/RedGnad/pokenads/master/pokenads-logo8.png"
            );

            var cfg = new AppKitConfig
            {
                projectId = "27f51a8cead380193aaf687f55e3d4af",
                metadata = new Metadata(
                    "Pokenads",
                    "AppKit Unity Sample - Monad Testnet",
                    Application.absoluteURL,        // ← ensure this matches your deployed URL
                    "https://raw.githubusercontent.com/RedGnad/pokenads/master/pokenads-logo8.png",
                    new RedirectData { Native = "appkit-sample-unity://" }
                ),
                customWallets = GetCustomWallets(),
                connectViewWalletsCountMobile = 6,
                includedWalletIds = isMobile ? new[]
                {
                    "2bd8c14e035c2d48f184aaa168559e86b0e3433228d3c4075900a221785019b0", // Backpack
                    "719bd888109f5e8dd23419b20e749900ce4d2fc6858cf588395f19c82fd036b3", // HAHA
                    "c57ca95b47569778a828d19178114f4db188b89b763c899ba0be274e97267d96"  // MetaMask
                } : null,
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
                Debug.Log($"[AppKitInit] Attempt {attempts}/{MAX_ATTEMPTS}...");
                var initTask = AppKit.InitializeAsync(cfg);

                float timer = 0f;
                while (!initTask.IsCompleted && timer < 5f)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (initTask.IsCompleted && !initTask.IsFaulted)
                {
                    Debug.Log("[AppKitInit] Initialization succeeded!");
                    AppKit.AccountConnected    += OnWalletEvent;
                    AppKit.AccountDisconnected += OnWalletEvent;
                    if (disableInteractionsOnModal) StartModalCheck();
                    if (shouldSwitchScene && Application.CanStreamedLevelBeLoaded(targetSceneName))
                        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
                    break;
                }

                yield return new WaitForSeconds(1f);
            }

            if (!AppKit.IsInitialized)
                Debug.LogError($"[AppKitInit] Failed after {MAX_ATTEMPTS} attempts.");

            _isInitializing = false;
        }

        private void OnWalletEvent(object sender, EventArgs e)
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
                if ((c.name.ToLower().Contains("modal") || c.name.ToLower().Contains("wallet"))
                    && c.gameObject.activeInHierarchy)
                    return true;
            return false;
        }

        private void DisableAllInteractions()
        {
            disabledComponents.Clear();
            foreach (var script in FindObjectsOfType<MonoBehaviour>())
            {
                if (script == null) continue;
                foreach (var target in interactionScriptNames)
                    if (script.GetType().Name == target && script.enabled)
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
            bool isMobile = Application.isMobilePlatform || IsWebGLMobile();
            if (isMobile)
            {
                return new[]
                {
                    new Wallet { Name="Backpack",    ImageUrl="https://backpack.app/favicon.ico",   MobileLink="backpack://", WebappLink="https://backpack.app/", Id="2bd8c14e035c2d48f184aaa168559e86b0e3433228d3c4075900a221785019b0" },
                    new Wallet { Name="HAHA",        ImageUrl="https://raw.githubusercontent.com/RedGnad/pokenads/master/pokenads-logo8.png", MobileLink="haha://", WebappLink="https://haha-wallet-url/", Id="719bd888109f5e8dd23419b20e749900ce4d2fc6858cf588395f19c82fd036b3" },
                    new Wallet { Name="MetaMask",    ImageUrl="https://metamask.io/images/favicon.ico", MobileLink="metamask://wc", WebappLink="https://metamask.io/", Id="c57ca95b47569778a828d19178114f4db188b89b763c899ba0be274e97267d96" },
                    new Wallet { Name="Trust Wallet",ImageUrl="https://trustwallet.com/assets/images/favicon.ico", MobileLink="trust://wc", Id="4622a2b2d6af1c9844944291e5e7351a6aa24cd7b23099efac1b2fd875da31a0" }
                };
            }
            else
            {
                return new[]
                {
                    new Wallet { Name="Backpack",    ImageUrl="https://backpack.app/favicon.ico",   MobileLink="backpack://", WebappLink="https://backpack.app/", Id="2bd8c14e035c2d48f184aaa168559e86b0e3433228d3c4075900a221785019b0" },
                    new Wallet { Name="HAHA",        ImageUrl="https://raw.githubusercontent.com/RedGnad/pokenads/master/pokenads-logo8.png", MobileLink="haha://", WebappLink="https://haha-wallet-url/", Id="719bd888109f5e8dd23419b20e749900ce4d2fc6858cf588395f19c82fd036b3" },
                    new Wallet { Name="MetaMask",    ImageUrl="https://metamask.io/images/favicon.ico", MobileLink="metamask://wc", WebappLink="https://metamask.io/", Id="c57ca95b47569778a828d19178114f4db188b89b763c899ba0be274e97267d96" },
                    new Wallet { Name="Trust Wallet",ImageUrl="https://trustwallet.com/assets/images/favicon.ico", MobileLink="trust://wc", Id="4622a2b2d6af1c9844944291e5e7351a6aa24cd7b23099efac1b2fd875da31a0" },
                    new Wallet { Name="Phantom",     ImageUrl="https://phantom.app/img/phantom-logo.png", WebappLink="https://phantom.app/ul/browse", Id="a797aa35c0fadbfc1a53e7f675162ed5226968b44a19ee3d24385c64d1d3c393" },
                    new Wallet { Name="Rabby",       ImageUrl="https://rabby.io/logo.png", WebappLink="https://rabby.io/" }
                };
            }
        }
    }
}
