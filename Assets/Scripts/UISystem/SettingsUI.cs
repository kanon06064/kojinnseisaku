using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; // AudioMixerを使うため必須
using UnityEngine.UI;
using TMPro;

namespace GameCore.UISystem
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider seSlider;

        [Header("Video Settings")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;

        // 内部変数
        private Resolution[] resolutions;
        private List<Resolution> filteredResolutions;

        private void Start()
        {
            // 初期設定の読み込みとUIへの反映
            SetupVideoOptions();
            LoadSettings();
        }

        // --- 1. 画面設定（解像度・フルスクリーン） ---

        private void SetupVideoOptions()
        {
            // PCが対応している解像度リストを取得
            resolutions = Screen.resolutions;
            filteredResolutions = new List<Resolution>();

            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();

            int currentResolutionIndex = 0;

            // リフレッシュレートが今のモニターと同じものだけ抽出してリスト化
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
                {
                    filteredResolutions.Add(resolutions[i]);
                    string option = resolutions[i].width + " x " + resolutions[i].height;
                    options.Add(option);

                    if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                    {
                        currentResolutionIndex = filteredResolutions.Count - 1;
                    }
                }
            }

            // Dropdownに選択肢を追加
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();

            // イベント登録
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = filteredResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;

            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        }

        // --- 2. 音量設定 (BGM / SE) ---

        // スライダーの値(0.0〜1.0)をデシベル(-80〜0)に変換してMixerに送る
        public void SetBGMVolume(float volume)
        {
            // Log10を使って自然な音量変化にする (0.0001fは無音回避用)
            float db = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20;
            if (volume <= 0) db = -80f; // 完全ミュート

            audioMixer.SetFloat("BGM_Vol", db);
            PlayerPrefs.SetFloat("BGM_Volume", volume);
        }

        public void SetSEVolume(float volume)
        {
            float db = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20;
            if (volume <= 0) db = -80f;

            audioMixer.SetFloat("SE_Vol", db);
            PlayerPrefs.SetFloat("SE_Volume", volume);
        }

        // --- 3. 設定のロード（起動時） ---

        private void LoadSettings()
        {
            // 音量 (デフォルト 0.5)
            float bgmVol = PlayerPrefs.GetFloat("BGM_Volume", 0.5f);
            float seVol = PlayerPrefs.GetFloat("SE_Volume", 0.5f);

            bgmSlider.value = bgmVol;
            seSlider.value = seVol;

            // UIイベントを手動で登録（Start時）
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            seSlider.onValueChanged.AddListener(SetSEVolume);

            // フルスクリーン (デフォルト ON)
            bool isFull = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.isOn = isFull;
        }
    }
}