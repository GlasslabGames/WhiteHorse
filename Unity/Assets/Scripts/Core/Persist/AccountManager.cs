using UnityEngine;
using System.Collections.Generic;

namespace GlassLab.Core.Serialization
{
  public class AccountManager : SingletonBehavior<AccountManager>
  {

    private AvatarType m_avatar = AvatarType.NONE;

    private const string SELECTED_ACCOUNT = "SelectedAccount";
    private const string ALL_ACCOUNTS = "PlayerAccounts";

    private const string AVATAR = "_Avatar";

    public static string CURRENT_USER_NAME = "";
    public static string CURRENT_USER_FIRST_NAME = "";
    public static string CURRENT_USER_LAST_NAME = "";
    public static string CURRENT_USER_ROLE = "";
    public static int CURRENT_USER_ID = -1;

    public void SelectAvatar(AvatarType avatar)
    {
      m_avatar = avatar;

      PlayerPrefs.SetInt(GetCurrentAccount() + AVATAR, (int)avatar);

      PlayMakerFSM.BroadcastEvent("PLAYER_AVATAR_SET");
    }

    public AvatarType GetAvatar()
    {
      if (m_avatar == AvatarType.NONE)
      { // if we haven't fetched the avatar yet, get it from player prefs
        m_avatar = (AvatarType)PlayerPrefs.GetInt(GetCurrentAccount() + AVATAR, (int)AvatarType.NONE);
      }
      return m_avatar;
    }

    public string GetCurrentAccount()
    {
      return PlayerPrefs.GetString(SELECTED_ACCOUNT);
    }

    public void SetCurrentAccount(string accountName)
    {
      if (DebugDataCollector.Instance != null)
        DebugDataCollector.Instance.AddInfo("CurrentAccount", accountName);
      PlayerPrefs.SetString(SELECTED_ACCOUNT, accountName);
    }

    public string[] GetAccounts()
    {
      return PlayerPrefsX.GetStringArray(ALL_ACCOUNTS);
    }

    public int GetNumAccounts()
    {
      return GetAccounts().Length;
    }

    /**
     * Function creates a new account, adding it to the preferences.
     */
    public void CreateAccount(string accountName)
    {
      // Get the existing accounts
      string[] savedAccounts = GetAccounts();

      // Set the new saved accounts length
      string[] newAccounts = new string[savedAccounts.Length + 1];

      // Iterate through the accounts to populate the new array
      for (int i = 0; i < savedAccounts.Length; i++)
      {
        newAccounts[i] = savedAccounts[i];
      }
      newAccounts[newAccounts.Length - 1] = accountName;

      // Save these new accounts
      PlayerPrefsX.SetStringArray(ALL_ACCOUNTS, newAccounts);
    }

    /**
     * Function deletes an existing account, removing it from the preferences.
     */
    public void DeleteAccount(string accountName)
    {
      // Get the existing accounts
      string[] savedAccounts = GetAccounts();

      // Set the new saved accounts length
      string[] newAccounts = new string[Mathf.Max(savedAccounts.Length - 1, 0)];

      // Iterate through the accounts to populate the new array
      // Ignore the account to delete
      int deletedAccountFound = 0;
      for (int i = 0; i < newAccounts.Length; i++)
      {
        if (savedAccounts[i + deletedAccountFound] == accountName)
        {
          deletedAccountFound = 1;
        }
        newAccounts[i] = savedAccounts[i + deletedAccountFound];
      }

      SessionManager.InstanceOrCreate.ClearSaves(accountName);

      // Save these new accounts
      PlayerPrefsX.SetStringArray(ALL_ACCOUNTS, newAccounts);

      // Reset the selected account if it was deleted
      if (GetCurrentAccount() == accountName)
      {
        SetCurrentAccount("");
      }

      // Delete the stored avatar for this account
      PlayerPrefs.DeleteKey(accountName + AVATAR);
    }

    /**
     * Function determines if an account exists in the preferences.
     */
    public bool AccountExists(string accountName)
    {
      // Get the user selection
      string[] savedAccounts = GetAccounts();

      // Iterate through the selection to see if the parameter name already exists
      for (int i = 0; i < savedAccounts.Length; i++)
      {
        if (savedAccounts[i] == accountName)
        {
          return true;
        }
      }

      // It does not exist
      return false;
    }

    // Checks if the avatar is set for the selected account 
    public bool CurrentAccountHasAvatar()
    {
      return (GetAvatar() != AvatarType.NONE);
    }

    public void ClearAllAccounts()
    {
      // Delete the avatars for each player
      string[] savedAccounts = GetAccounts();
      for (int i = 0; i < savedAccounts.Length; i++)
      {
        PlayerPrefs.DeleteKey(savedAccounts[i] + AVATAR);
      }

      PlayerPrefs.DeleteKey("PlayerAccounts");
      PlayerPrefs.DeleteKey("SelectedAccount");
    }
  }
}