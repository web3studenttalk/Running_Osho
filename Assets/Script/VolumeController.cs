using UnityEngine;
using UnityEngine.Audio; // AudioMixerを使うために必要
using UnityEngine.UI;    // Sliderを使うために必要

public class VolumeController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer; // 作成したMixerを入れる
    [SerializeField] private Slider bgmSlider;      // 作成したSliderを入れる

    private void Start()
    {
        // ゲーム開始時にスライダーの現在の値を適用する（保存機能がない場合）
        // スライダーの初期値に合わせて音量をセット
        SetBGMVolume(bgmSlider.value);
        
        // スライダーを動かした時のイベントを登録
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
    }

    // スライダーの値(0.0001 ~ 1.0)を受け取り、デシベル(-80 ~ 0)に変換してセットする
    public void SetBGMVolume(float value)
    {
        // ヒント: 人間の耳は対数的に音を感じるため、Log10を使って自然な変化を作ります
        // 公式: 20 * log10(value)
        // valueが1のとき 0dB, valueが0.1のとき -20dB となります
        float decibel = 20f * Mathf.Log10(value);

        // "BGMVolume"はステップ1でExposeしたパラメータ名と完全に一致させる必要があります
        audioMixer.SetFloat("BGMVolume", decibel);
    }
}