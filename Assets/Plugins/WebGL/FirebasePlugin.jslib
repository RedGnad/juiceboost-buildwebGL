mergeInto(LibraryManager.library, {
  AggregateWalletScores: function () {
    setTimeout(function () {
      console.log(
        "[JS] AggregateWalletScores stub called - cette fonction est maintenant obsolète"
      );
      console.log("[JS] Les scores sont directement agrégés dans SubmitScore");
    }, 0);
  },

  TestWalletCollection: function () {
    setTimeout(function () {
      try {
        if (!window.db || !firebase.auth().currentUser) {
          console.error(
            "[JS] Firestore non initialisé ou utilisateur non authentifié !"
          );
          return;
        }
        var wallet = window.currentWalletAddress || "";
        if (!wallet) {
          console.warn("[JS] Wallet address is empty!");
          return;
        }

        window.db
          .collection("WalletScores")
          .doc(wallet)
          .set({
            wallet: wallet,
            testTime: firebase.firestore.FieldValue.serverTimestamp(),
          })
          .then(function () {
            console.log("[JS] WalletScores test document created successfully");
          })
          .catch(function (e) {
            console.error("[JS] Error creating test document:", e);
          });
      } catch (e) {
        console.error("[JS] TestWalletCollection failed:", e);
      }
    }, 0);
  },

  SubmitScore: function (score, coins) {
    setTimeout(function () {
      try {
        if (!window.db || !firebase.auth().currentUser) {
          console.error(
            "[JS] Firestore non initialisé ou utilisateur non authentifié !"
          );
          return;
        }
        var wallet = window.currentWalletAddress || "";
        var uid = firebase.auth().currentUser.uid;

        if (!wallet) {
          console.warn("[JS] SubmitScore: wallet address is empty!");
        }

        window.db
          .collection("Scores")
          .doc(uid)
          .get()
          .then(function (doc) {
            var bestScore = score;
            if (doc.exists && typeof doc.data().bestScore === "number") {
              bestScore = Math.max(score, doc.data().bestScore);
            }
            return window.db
              .collection("Scores")
              .doc(uid)
              .set(
                {
                  wallet: wallet,
                  lastScore: score,
                  bestScore: bestScore,
                  timestamp: firebase.firestore.FieldValue.serverTimestamp(),
                  totalScore: firebase.firestore.FieldValue.increment(score),
                  totalCoins: firebase.firestore.FieldValue.increment(coins),
                },
                { merge: true }
              );
          })
          .then(function () {
            console.log(
              "[JS] Score submitted:",
              score,
              "coins:",
              coins,
              "for UID:",
              uid,
              "wallet:",
              wallet
            );

            if (wallet) {
              window.db
                .collection("WalletScores")
                .doc(wallet)
                .get()
                .then(function (doc) {
                  var walletData = {};

                  if (doc.exists) {
                    walletData = {
                      bestScore: Math.max(doc.data().bestScore || 0, score),
                      totalScore: (doc.data().totalScore || 0) + score,
                      totalCoins: (doc.data().totalCoins || 0) + coins,
                      playerCount: doc.data().playerCount || 1,
                    };
                  } else {
                    walletData = {
                      bestScore: score,
                      totalScore: score,
                      totalCoins: coins,
                      playerCount: 1,
                    };
                  }

                  return window.db.collection("WalletScores").doc(wallet).set(
                    {
                      wallet: wallet,
                      bestScore: walletData.bestScore,
                      totalScore: walletData.totalScore,
                      totalCoins: walletData.totalCoins,
                      playerCount: walletData.playerCount,
                      lastUpdated:
                        firebase.firestore.FieldValue.serverTimestamp(),
                    },
                    { merge: true }
                  );
                })
                .then(function () {
                  console.log("[JS] WalletScores updated for wallet", wallet);
                })
                .catch(function (err) {
                  console.error("[JS] Error updating WalletScores:", err);
                });
            }
          })
          .catch(function (e) {
            console.error("[JS] SubmitScore error:", e);
          });
      } catch (e) {
        console.error("[JS] SubmitScore failed:", e);
      }
    }, 0);
  },

  GetLeaderboard: function () {
    setTimeout(function () {
      try {
        if (!window.db) {
          console.error("[JS] Firestore non initialisé !");
          return;
        }
        window.db
          .collection("Scores")
          .orderBy("bestScore", "desc")
          .limit(5)
          .get()
          .then(function (querySnapshot) {
            var results = [];
            querySnapshot.forEach(function (doc) {
              results.push({
                wallet: doc.data().wallet || "",
                bestScore: doc.data().bestScore || 0,
              });
            });
            if (window.unityInstance) {
              window.unityInstance.SendMessage(
                "LeaderboardManager",
                "OnLeaderboardReceived",
                JSON.stringify(results)
              );
            }
          })
          .catch(function (e) {
            console.error("[JS] GetLeaderboard error:", e);
          });
      } catch (e) {
        console.error("[JS] GetLeaderboard failed:", e);
      }
    }, 0);
  },

  GetMyScores: function () {
    setTimeout(function () {
      try {
        if (!window.db || !firebase.auth().currentUser) {
          console.error(
            "[JS] Firestore non initialisé ou utilisateur non authentifié !"
          );
          return;
        }
        var uid = firebase.auth().currentUser.uid;
        window.db
          .collection("Scores")
          .doc(uid)
          .get()
          .then(function (doc) {
            var bestScore = 0;
            var totalScore = 0;
            if (doc.exists) {
              bestScore = doc.data().bestScore || 0;
              totalScore = doc.data().totalScore || 0;
            }
            console.log(
              "[JS] GetMyScores: sending to Unity",
              bestScore,
              totalScore
            );
            if (window.unityInstance) {
              window.unityInstance.SendMessage(
                "MyScoreManager",
                "OnMyScoresReceived",
                JSON.stringify({ bestScore: bestScore, totalScore: totalScore })
              );
            }
          })
          .catch(function (e) {
            console.error("[JS] GetMyScores error:", e);
          });
      } catch (e) {
        console.error("[JS] GetMyScores failed:", e);
      }
    }, 0);
  },
});
