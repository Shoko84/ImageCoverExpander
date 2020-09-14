using System.Collections;
using System.Linq;
using BS_Utils.Utilities;
using HMUI;
using ImageCoverExpander.Models;
using ImageCoverExpander.Utilities;
using IPA;
using IPA.Config.Stores;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Component = UnityEngine.Component;
using Config = IPA.Config.Config;
using Image = UnityEngine.UI.Image;
using IPALogger = IPA.Logging.Logger;
using ReflectionUtil = BS_Utils.Utilities.ReflectionUtil;

namespace ImageCoverExpander
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        #region Properties

        public static IPALogger Log { get; private set; }

        #endregion

        #region BSIPA Events

        [Init]
        public Plugin(IPALogger logger, Config conf)
        {
            Log = logger;
            PluginConfig.Instance = conf.Generated<PluginConfig>();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
        }
        #endregion

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
                    new UnityTask(ChangeColor(controlCell, bg, Float4.ToColor(PluginConfig.Instance.ButtonColor)));
            }
        }

        private static void OnMenuSceneLoadedFresh()
        {
            var mmvc = Resources.FindObjectsOfTypeAll<MainMenuViewController>().FirstOrDefault();
            if (!mmvc) return;
            mmvc.didFinishEvent += OnDidFinishEvent;
        }

        private static void OnDidFinishEvent(MainMenuViewController mmvc, MainMenuViewController.MenuButton _)
        {
            var sldvc = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
            var ldvc = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault();
            if (!sldvc || !ldvc) return;
            var bcscc = ReflectionUtil.GetPrivateField<BeatmapCharacteristicSegmentedControlController>(ldvc, "_beatmapCharacteristicSegmentedControlController");
            var bdscc = ReflectionUtil.GetPrivateField<BeatmapDifficultySegmentedControlController>(ldvc, "_beatmapDifficultySegmentedControlController");
            if (!bcscc || !bdscc) return;
            sldvc.didPresentContentEvent += (sldvcController, type) => {
                bcscc.didSelectBeatmapCharacteristicEvent += (controlController, so) => { RefreshButtonsBackgrounds<TextSegmentedControlCellNew>(bdscc); };
                RefreshButtonsBackgrounds<IconSegmentedControlCell>(bcscc);
                RefreshButtonsBackgrounds<TextSegmentedControlCellNew>(bdscc);
            };
            var coverImage = ReflectionUtil.GetPrivateField<RawImage>(ldvc, "_coverImage");
            var levelInfo = coverImage.transform.parent;
            var playerStatsContainer = ReflectionUtil.GetPrivateField<GameObject>(ldvc, "_playerStatsContainer");
            var pscLayout = playerStatsContainer.GetComponent<LayoutElement>();
            var playContainer = bdscc.transform.parent;
            var levelInfoLayout = levelInfo.GetComponent<LayoutElement>();
            if (!coverImage || !levelInfo || !playerStatsContainer || !pscLayout || !playContainer || !levelInfoLayout) return;
            coverImage.transform.localPosition = new Vector3(0, 0, coverImage.transform.localPosition.z);
            coverImage.transform.localScale = Vector3.one * 0.925f;
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
            mmvc.didFinishEvent -= OnDidFinishEvent;
        }
    }
}