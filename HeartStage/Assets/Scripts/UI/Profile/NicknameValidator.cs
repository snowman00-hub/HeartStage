using System.Text.RegularExpressions;

public static class NicknameValidator
{
    public const int NickMinLength = 2;
    public const int NickMaxLength = 10;

    // 상태 메시지는 나중에 같이 쓸 거면 여기서 같이 관리
    public const int StatusMinLength = 0;
    public const int StatusMaxLength = 30;

    // 한글, 영문, 숫자만 허용 (공백/특문 금지)
    // ^[0-9a-zA-Z가-힣]+$
    private static readonly Regex NickRegex = new Regex(@"^[0-9a-zA-Z가-힣]+$", RegexOptions.Compiled);

    // 이 라인은 너 프로젝트 구조 맞게 수정해줘 (예: DataTableManager.Instance.SlangTable 등)
    private static SlangTable SlangTable => DataTableManager.SlangTable;

    public static bool ValidateNickname(string raw, out string errorMessage)
    {
        errorMessage = null;

        if (SaveLoadManager.Data is not SaveDataV1)
        {
            errorMessage = "세이브 데이터를 찾을 수 없습니다.";
            return false;
        }

        string nick = raw.Trim();

        // 길이 체크
        int len = nick.Length;
        if (len < NickMinLength || len > NickMaxLength)
        {
            errorMessage = $"닉네임은 {NickMinLength}~{NickMaxLength}글자까지 가능합니다.";
            return false;
        }

        // 공백/특수문자 체크
        if (!NickRegex.IsMatch(nick))
        {
            errorMessage = "닉네임에는 한글, 영문, 숫자만 사용할 수 있습니다.";
            return false;
        }

        // 금칙어 체크
        if (SlangTable != null && SlangTable.ContainsSlangIn(nick))
        {
            errorMessage = "사용할 수 없는 단어가 포함되어 있습니다.";
            return false;
        }

        return true;
    }

    public static bool ValidateStatus(string raw, out string errorMessage)
    {
        errorMessage = null;

        string msg = raw.Trim();
        int len = msg.Length;

        if (len < StatusMinLength || len > StatusMaxLength)
        {
            errorMessage = $"상태메시지는 최대 {StatusMaxLength}글자까지 가능합니다.";
            return false;
        }

        if (len == 0)
            return true;

        if (SlangTable != null && SlangTable.ContainsSlangIn(msg))
        {
            errorMessage = "사용할 수 없는 단어가 포함되어 있습니다.";
            return false;
        }

        return true;
    }
}
