using Firebase.Auth;

public static class ProfileNameUtil
{
    public static string GetEffectiveNickname(SaveDataV1 data)
    {
        // 세이브에 닉네임이 있으면 그걸 우선 사용
        if (data != null && !string.IsNullOrEmpty(data.nickname))
            return data.nickname;

        // 없으면 Firebase uid 사용
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null || string.IsNullOrEmpty(user.UserId))
            return "Guest";

        return user.UserId;
    }
}
