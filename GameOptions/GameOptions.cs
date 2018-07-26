using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Harmony;
using System.Reflection;
using System.Collections.Generic;

[ModTitle("GameOptions")]
[ModDescription("Change the gamemode. Usage: gamemode <mode> (Options: Creative, Easy, Normal, Hardcore).")]
[ModAuthor("AquatikJustice")]
[ModVersion("1.0")]
[RaftVersion("1.03")]
public class GameOptions : Mod
{
    HarmonyInstance instance;
    public static bool shouldUpdateInventoryMenu = false;
    public static bool isCreative = false;
    string researchBackup;

    private void Start()
    {
        instance = HarmonyInstance.Create("com.aquatikjustice.GameOptions");
        instance.PatchAll(Assembly.GetExecutingAssembly());

        RConsole.registerCommand("gamemode", "Change the gamemode. Usage: gamemode <mode> (Options: Creative, Easy, Normal, Hardcore)", "gamemode", new Action(SwitchMode));
        RConsole.registerCommand("joinable", "Set whether your friends can join your game or not. Usage: joinable <true|false>", "joinable", new Action(ChangeAllowFriends));
        RConsole.registerCommand("friendlyFire", "Toggles whether Friendly Fire is on or off.", "friendlyFire", new Action(SetFriendlyFire));
        Log("<color=#ffcf01>GameOptions</color> - LOADED");
    }

    private void SwitchMode()
    {
        string command = RConsole.lastCommands.LastOrDefault();
        string[] comArray = command.Split(' ');

        if (comArray.Length > 1)
        {
            string newMode = comArray[1].ToLower();

            if (newMode == "creative" || newMode == "easy" || newMode == "normal" || newMode == "hardcore")
            {
                ModeCheck(newMode);
            }
            else
            {
                Log("<b><color=#d7575a>GameOptions: That input is invalid. Please choose <color=#ffcf01>Creative</color>, <color=#ffcf01>Easy</color>, <color=#ffcf01>Normal</color> or <color=#ffcf01>Hardcore</color>.</color></b>");
            }

            CraftingMenu cMenu = ComponentManager<CraftingMenu>.Value;

            if (cMenu != null)
            {
                cMenu.creativeModeCategoryButton.SetActive(GameManager.GameMode == GameMode.Creative);
            }
        }
    }

    public static void MenuUpdate()
    {
        CraftingMenu cMenu = ComponentManager<CraftingMenu>.Value;
        cMenu.creativeModeCategoryButton.SetActive(GameManager.GameMode == GameMode.Creative);
        shouldUpdateInventoryMenu = false;
    }

    public void ModeCheck(string mode)
    {

        if (GameManager.GameMode.ToString().ToLower() != mode.ToLower())
        {
            switch (mode.ToLower())
            {
                case "creative":
                    SaveLearnedRecipes();
                    GameManager.GameMode = GameMode.Creative;
                    break;
                case "easy":
                    GameManager.GameMode = GameMode.Easy;
                    RestoreLearnedRecipes();
                    break;
                case "normal":
                    GameManager.GameMode = GameMode.Normal;
                    RestoreLearnedRecipes();
                    break;
                case "hardcore":
                    GameManager.GameMode = GameMode.Hardcore;
                    RestoreLearnedRecipes();
                    break;
            }
            Log($"<color=#ffcf01>GameOptions</color>: Game mode set to <color=#ffcf01>{char.ToUpper(mode[0]) + mode.Substring(1)}</color>.");
            shouldUpdateInventoryMenu = true;
        }
        else
        {
            Log($"<color=#d7575a>Your game is already in <color=#ffcf01>{char.ToUpper(mode[0]) + mode.Substring(1)}</color> mode.</color>");
        }
    }

    public void SetFriendlyFire()
    {
        GameManager.FriendlyFire = !GameManager.FriendlyFire;
        Log($"Friendly fire is set to {GameManager.FriendlyFire}");
    }

    public void SaveLearnedRecipes()
    {

    }

    public void PrintLearnedRecipes()
    {
        
    }

    public void PrintBackup()
    {
        
    }

    public void RestoreLearnedRecipes()
    {

    }

    public void Log(string text)
    {
        RConsole.Log($"<b><color=#129aab>{text}</color></b>");
    }

    public void ChangeAllowFriends()
    {
        string command = RConsole.lastCommands.LastOrDefault();
        string[] comArray = command.Split(' ');

        if (comArray[1].ToLower() == "true" || comArray[1].ToLower() == "false")
        {
            if (Semih_Network.AllowFriendsToJoin != Convert.ToBoolean(comArray[1].ToLower()))
            {
                Semih_Network.AllowFriendsToJoin = Convert.ToBoolean(comArray[1].ToLower());
                Log($"GameOptions: Friends are able to join your game - {comArray[1].ToUpper()}");
            }
        }
        else
        {
            Log("GameOptions: Invalid Command. Please choose either TRUE or FALSE.");
        }
    }

    public List<ResearchMenuItem> rmiList = new List<ResearchMenuItem>();
    public List<BingoMenuItem> bmiList = new List<BingoMenuItem>();

    public List<ResearchMenuItem> researchedItemsBackup = new List<ResearchMenuItem>();
}

[HarmonyPatch(typeof(CanvasHelper))]
[HarmonyPatch("OpenMenu")]
class Patch
{
    static public void Postfix(MenuType menuType)
    {
        if (menuType == MenuType.Inventory && GameOptions.shouldUpdateInventoryMenu)
        {
            GameOptions.MenuUpdate();
        }
    }
}