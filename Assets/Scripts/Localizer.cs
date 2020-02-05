using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class Localizer : MonoBehaviour {

    public List<LocSettings> LocaleSettings;
    public TMP_FontAsset AlternateFont;
    public TextMeshProUGUI NewGameText1;
    public TextMeshProUGUI NewGameText2;
    public TextMeshProUGUI HighScoreText;
    public TextMeshProUGUI GoodJobText;
    public TextMeshProUGUI PurchasePromptText;
    public TextMeshProUGUI LevelText;

    private TMP_FontAsset newGameText1Font;
    private TMP_FontAsset newGameText2Font;
    private TMP_FontAsset highScoreTextFont;
    private TMP_FontAsset goodJobTextFont;
    private TMP_FontAsset purchasePromptTextFont;
    private TMP_FontAsset levelTextFont;
    private Material newGameText1Material;
    private Material newGameText2Material;
    private Material highScoreTextMaterial;
    private Material goodJobTextMaterial;
    private Material purchasePromptTextMaterial;
    private Material levelTextMaterial;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        // remember the default font and material in case we switch back to a compatible locale
        newGameText1Font = NewGameText1.font;
        newGameText2Font = NewGameText2.font;
        highScoreTextFont = HighScoreText.font;
        goodJobTextFont = GoodJobText.font;
        purchasePromptTextFont = PurchasePromptText.font;
        levelTextFont = LevelText.font;
        newGameText1Material = NewGameText1.materialForRendering;
        newGameText2Material = NewGameText2.materialForRendering;
        highScoreTextMaterial = HighScoreText.materialForRendering;
        goodJobTextMaterial = GoodJobText.materialForRendering;
        purchasePromptTextMaterial = PurchasePromptText.materialForRendering;
        levelTextMaterial = LevelText.materialForRendering;
        
        LoadLocale(Application.systemLanguage);
        //LoadLocale(SystemLanguage.Spanish);
    }

    //--------------------------------------------------------------------------------------------------------
    public void LoadLocale(SystemLanguage lang) {
        foreach (LocSettings locale in LocaleSettings) {
            if (locale.Locale == lang) {
                // replace strings
                NewGameText1.text = locale.NewGameString;
                NewGameText2.text = locale.NewGameString;
                HighScoreText.text = locale.HighScoreString;
                GoodJobText.text = locale.GoodJobString;
                PurchasePromptText.text = locale.PurchasePromptString;
                LevelText.text = locale.LevelString;

                if (locale.UseAlternateFont) {
                    NewGameText1.font = AlternateFont;
                    NewGameText2.font = AlternateFont;
                    HighScoreText.font = AlternateFont;
                    GoodJobText.font = AlternateFont;
                    PurchasePromptText.font = AlternateFont;
                    LevelText.font = AlternateFont;
                }
                else {
                    NewGameText1.font = newGameText1Font;
                    NewGameText2.font = newGameText2Font;
                    HighScoreText.font = highScoreTextFont;
                    GoodJobText.font = goodJobTextFont;
                    PurchasePromptText.font = purchasePromptTextFont;
                    LevelText.font = levelTextFont;
                }
                
                return;
            }
        }
        
        // if we've gotten here, that means the system's locale was not found - default back to enUS
        LoadLocale(SystemLanguage.English);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void LoadLocale(string fourCc) {
        LocSettings newSettings = LocaleSettings.FirstOrDefault(loc => fourCc == loc.FourCC);
        LoadLocale(newSettings != null ? newSettings.Locale : SystemLanguage.English);
    }
}
