mergeInto(LibraryManager.library, {
  SetWalletAddressJS: function (walletPtr) {
    window.currentWalletAddress = UTF8ToString(walletPtr);
    console.log(
      "[JS] Wallet address set on window:",
      window.currentWalletAddress
    );
  },
});
