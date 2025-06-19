mergeInto(LibraryManager.library, {
  SubmitScore: function (score) {
    setTimeout(function () {
      try {
        if (!window.db || !firebase.auth().currentUser) {
          console.error(
            "[JS] Firestore non initialisé ou utilisateur non authentifié !"
          );
          return;
        }
        const wallet = window.currentWalletAddress || "";
        const uid = firebase.auth().currentUser.uid;

        if (!wallet) {
          console.warn("[JS] SubmitScore: wallet address is empty!");
        }

        window.db
          .collection("Scores")
          .doc(uid)
          .get()
          .then((doc) => {
            let bestScore = score;
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
                },
                { merge: true }
              );
          })
          .then(() =>
            console.log(
              "[JS] Score submitted:",
              score,
              "for UID:",
              uid,
              "wallet:",
              wallet,
              "bestScore:",
              score
            )
          )
          .catch((e) => console.error("[JS] SubmitScore error:", e));
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
          .then((querySnapshot) => {
            const results = [];
            querySnapshot.forEach((doc) => {
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
          .catch((e) => console.error("[JS] GetLeaderboard error:", e));
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
        const uid = firebase.auth().currentUser.uid;
        window.db
          .collection("Scores")
          .doc(uid)
          .get()
          .then((doc) => {
            let bestScore = 0;
            let totalScore = 0;
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
          .catch((e) => console.error("[JS] GetMyScores error:", e));
      } catch (e) {
        console.error("[JS] GetMyScores failed:", e);
      }
    }, 0);
  },
});
