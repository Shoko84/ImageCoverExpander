using System.Collections;
using System.Linq;
using BS_Utils.Utilities;
using FightSabers.Utilities;
using HMUI;
using ImageCoverExpander.Models;
using IPA;
using IPA.Config;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Config = IPA.Config.Config;
using Image = UnityEngine.UI.Image;
using IPALogger = IPA.Logging.Logger;
using ReflectionUtil = BS_Utils.Utilities.ReflectionUtil;

namespace ImageCoverExpander
{
    public class Plugin : IBeatSaberPlugin
    {
        internal static Ref<PluginConfig> config;
        internal static IConfigProvider   configProvider;

        public void Init(IPALogger logger, [Config.Prefer("json")] IConfigProvider cfgProvider)
        {
            Logger.log = logger;
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            configProvider = cfgProvider;

            config = cfgProvider.MakeLink<PluginConfig>((p, v) => {
                if (v.Value == null || v.Value.RegenerateConfig)
                    p.Store(v.Value = new PluginConfig { RegenerateConfig = false });
                config = v;
            });
        }

        private static IEnumerator ChangeColor(IEventSystemHandler systemHandler, Image bg, Color c)
        {
            yield return new WaitForEndOfFrame();
            if (!bg || systemHandler == null) yield break;
            ReflectionUtil.SetPrivateField(systemHandler, "_normalBGColor", c);
            ReflectionUtil.SetPrivateField(systemHandler, "_selectedBGColor", c);
            bg.color = c;
        }

        private static void RefreshButtonsBackgrounds<T>(Component bdscc) where T : SegmentedControlCell
        {
            if (!bdscc) return;
            foreach (Transform child in bdscc.transform)
            {
                var controlCell = child.GetComponent<T>();
                if (!controlCell) continue;
                var bg = ReflectionUtil.GetPrivateField<Image>(controlCell, "_bgImage");
                if (!bg)
                    bg = controlCell.GetComponent<Image>();
                if (bg)
                    new UnityTask(ChangeColor(controlCell, bg, Float4.ToColor(config.Value.ButtonColor)));
            }
        }

        private static void OnMenuSceneLoadedFresh()
        {
            var sldvc = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
            var ldvc = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault();
            if (!sldvc || !ldvc) return;
            var bcscc = ReflectionUtil.GetPrivateField<BeatmapCharacteristicSegmentedControlController>(ldvc, "_beatmapCharacteristicSegmentedControlController");
            var bdscc = ReflectionUtil.GetPrivateField<BeatmapDifficultySegmentedControlController>(ldvc, "_beatmapDifficultySegmentedControlController");
            if (!bcscc || !bdscc) return;
            sldvc.didPresentContentEvent += (controller, type) => {
                bcscc.didSelectBeatmapCharacteristicEvent += (controlController, so) => {
                    RefreshButtonsBackgrounds<TextSegmentedControlCellNew>(bdscc);
                };
                RefreshButtonsBackgrounds<IconSegmentedControlCell>(bcscc);
                RefreshButtonsBackgrounds<TextSegmentedControlCellNew>(bdscc);
            };
            var coverImage = ReflectionUtil.GetPrivateField<RawImage>(ldvc, "_coverImage");
            var levelInfo = coverImage.transform.parent;
            var playerStatsContainer = ReflectionUtil.GetPrivateField<GameObject>(ldvc, "_playerStatsContainer");
            var pscLayout = playerStatsContainer.GetComponent<LayoutElement>();
            var playContainer = bdscc.transform.parent;
            var levelInfoLayout = levelInfo.GetComponent<LayoutElement>();
            if (!coverImage || !levelInfo || ! playerStatsContainer || !pscLayout || !playContainer || !levelInfoLayout) return;
            coverImage.transform.localPosition = new Vector3(0, 0, coverImage.transform.localPosition.z);
            coverImage.transform.localScale = Vector3.one;
            coverImage.GetComponent<RectTransform>().sizeDelta = new Vector2(11, 11);
            coverImage.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            coverImage.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            coverImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(1.5f, 0);
            coverImage.uvRect = new Rect(0, 0, 1, 1);
            playContainer.SetParent(levelInfo);
            playContainer.localPosition = new Vector3(-1, -52);
            playContainer.GetComponent<Image>().enabled = false;
            pscLayout.transform.SetParent(levelInfo);
            pscLayout.transform.localPosition = new Vector3(0, -10);
            levelInfoLayout.preferredHeight = 60;
        }

        public void OnApplicationStart() { }

        public void OnApplicationQuit() { }

        public void OnFixedUpdate() { }

        public void OnUpdate() { }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene) { }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) { }

        public void OnSceneUnloaded(Scene scene) { }
    }
}