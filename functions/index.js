const functions = require("firebase-functions");
const admin = require("firebase-admin");

// Firebase Admin SDK 초기화
admin.initializeApp();

// 모든 유저에게 아이템 지급하는 Cloud Function
exports.giveItemsToAllUsers = functions.https.onRequest(async (req, res) => {
  // CORS 설정
  res.set("Access-Control-Allow-Origin", "*");
  res.set("Access-Control-Allow-Methods", "GET, POST");
  res.set("Access-Control-Allow-Headers", "Content-Type");

  if (req.method === "OPTIONS") {
    res.status(204).send("");
    return;
  }

  try {
    // 간단한 인증 체크
    const adminKey = req.query.key || (req.body && req.body.key);
    if (adminKey !== "heartstage-admin-2024") {
      res.status(401).json({
        success: false,
        error: "관리자 권한이 필요합니다.",
      });
      return;
    }

    // 요청 데이터 파싱
    const title = (req.body && req.body.title) || "🎁 특별 선물!";
    const content = (req.body && req.body.content) || "운영팀에서 준비한 특별한 선물입니다!";
    const items = (req.body && req.body.items) || [
      {itemId: "7101", count: 500}, // 라이트스틱
      {itemId: "7104", count: 100}, // 드림에너지
    ];

    console.log("아이템 지급 시작:", {title, itemCount: items.length});

    const db = admin.database();

    // 모든 유저 ID 가져오기
    const saveDataSnapshot = await db.ref("saveData").once("value");
    const userData = saveDataSnapshot.val();

    if (!userData) {
      res.json({
        success: false,
        error: "유저 데이터가 없습니다.",
      });
      return;
    }

    const userIds = Object.keys(userData);
    console.log(`총 ${userIds.length}명의 유저 발견`);

    if (userIds.length === 0) {
      res.json({
        success: false,
        error: "지급할 유저가 없습니다.",
      });
      return;
    }

    // 배치 업데이트용 데이터 준비
    const updates = {};

    userIds.forEach((userId) => {
      const mailId = db.ref().child("mails").push().key;

      const mailData = {
        mailId: mailId,
        senderId: "system",
        senderName: "운영팀",
        receiverId: userId,
        title: title,
        content: content,
        timestamp: admin.database.ServerValue.TIMESTAMP,
        isRead: false,
        isRewarded: false,
        itemList: items,
      };

      updates[`mails/${userId}/${mailId}`] = mailData;
    });

    // 모든 메일을 한 번에 전송
    await db.ref().update(updates);

    console.log(`${userIds.length}명에게 아이템 지급 완료`);

    res.json({
      success: true,
      message: `${userIds.length}명에게 아이템을 지급했습니다!`,
      userCount: userIds.length,
      items: items,
      timestamp: new Date().toISOString(),
    });
  } catch (error) {
    console.error("아이템 지급 오류:", error);
    res.status(500).json({
      success: false,
      error: `서버 오류: ${error.message}`,
    });
  }
});

// 테스트용 함수
exports.testFunction = functions.https.onRequest((req, res) => {
  res.json({
    success: true,
    message: "Firebase Cloud Functions가 정상 작동합니다!",
    timestamp: new Date().toISOString(),
  });
});
