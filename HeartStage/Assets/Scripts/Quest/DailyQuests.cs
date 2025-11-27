using UnityEngine;
using UnityEngine.UI;

public class DailyQuests : MonoBehaviour
{
    [Header("퀘스트 진행도 보상 버튼")]
    [SerializeField] private Button RewardButton1;
    [SerializeField] private Button RewardButton2;
    [SerializeField] private Button RewardButton3;
    [SerializeField] private Button RewardButton4;
    [SerializeField] private Button RewardButton5;

    [Header("진행도 게이지")]
    [SerializeField] private Slider progressSlider;

    //진행도 게이지 20 40 60 80 100 마다 보상 버튼 활성화
    //진행도 게이지는 외부에서 설정해줘야함
    //보상버튼은 활성화 x 시 이미지 / 활성화 시 이미지 / 활성화 클릭 후 이미지가 다 다름
    //활성화 후에는 클릭 x
    //이미지는 Adressable로 관리중


}
