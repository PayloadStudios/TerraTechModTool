#if UNITY_EDITOR

using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SteamWorkshopUploader
{
	private static CallResult<CreateItemResult_t> createWorkshopItemCallback;
	private static CallResult<SubmitItemUpdateResult_t> submitItemUpdateCallback;
	private static UGCUpdateHandle_t currentItemEditingHandle = UGCUpdateHandle_t.Invalid;
	private static ModContents currentlyProcessingMod = null;
	private static bool bInitedSteam = false;
	private static bool bCompletedUpload = true;

	private static AppId_t m_AppId;
	private static CGameID m_GameID;
	private static AccountID_t m_AccountID;

	public static bool IsSteamInited()
	{
		return bInitedSteam;

	}

	static void CheckForInit()
	{
		if(!bInitedSteam)
		{
			File.WriteAllText("steam_appid.txt", "285920");

			// Init Steam
			if (!Packsize.Test())
				Debug.LogError("[Steam] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");

			if (!DllCheck.Test())
				Debug.LogError("[Steam] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");

			if (SteamAPI.Init())
			{
				bInitedSteam = true;

				Init();

				Debug.Log("[Steam] Steam api succesfully inited");
			}
			else
			{
				Debug.LogError("[Steam] Could not connect to Steam. Do you need to run it first?");
			}
		}
	}

	static void Init()
	{
		m_AppId = SteamUtils.GetAppID();
		var steamID = SteamUser.GetSteamID();
		m_AccountID = steamID.GetAccountID();
		m_GameID = new CGameID(m_AppId);

		currentlyProcessingMod = null;

		createWorkshopItemCallback = CallResult<CreateItemResult_t>.Create(WorkshopItemCreated);
		submitItemUpdateCallback = CallResult<SubmitItemUpdateResult_t>.Create(WorkshopEditsSubmitted);
	}

	public static void Update()
	{
		if (bInitedSteam)
		{
			SteamAPI.RunCallbacks();
		}
	}

	public static bool AssignWorkshopID(ModContents mod)
	{
		bool success = false;
		CheckForInit();

		if (mod.m_WorkshopId == PublishedFileId_t.Invalid)
		{
			if (currentlyProcessingMod == null)
			{
				currentlyProcessingMod = mod;

				SteamAPICall_t iHandle =
					SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
				createWorkshopItemCallback.Set(iHandle);

				success = iHandle != SteamAPICall_t.Invalid;
			}
			else
			{
				Debug.LogError($"Trying to register a workshop ID for mod {mod} while {currentlyProcessingMod} was already working");
			}
		}
		else
		{
			// TODO: Maybe there's a case for players deleting the mod off the workshop and needing a fresh ID
			Debug.Log($"Mod {mod} already has a workshop ID");
			success = true;
		}

		return success;
	}

	private static void WorkshopItemCreated(CreateItemResult_t callback, bool bIOFailure)
	{
		if (callback.m_eResult == EResult.k_EResultOK)
		{
			currentlyProcessingMod.SetWorkshopID(callback.m_nPublishedFileId);
		}
		else
		{
			Debug.LogError("Creating workshop item failed with code " + callback.m_eResult);
		}

		if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			Debug.LogError("You need to accept the workshop legal agreement to continue steam://url/CommunityFilePage/" + callback.m_nPublishedFileId);
		}

		currentlyProcessingMod = null;
	}

	public static string GetCorpTag(string corp)
	{
		switch (corp)
		{
			case "GSO": return "GSO";
			case "GC": return "GeoCorp";
			case "VEN": return "Venture";
			case "HE": return "Hawkeye";
			case "BF": return "BetterFuture";
			case "EXP": return "ReticuleResearch";
			default: return "Custom Corps";
		}
	}

	public static bool PublishAssetBundleToWorkshop(ModContents mod, string folder, string changenotes, bool setTags)
	{
		bool success = false;
		CheckForInit();

		if (mod.m_WorkshopId != PublishedFileId_t.Invalid)
		{
			if (currentlyProcessingMod == null)
			{
				currentlyProcessingMod = mod;

				StartEditing(mod);
				{
					SetTitle(mod.ModName);
					SetTag("Type", "Mods");
					if (mod.m_Skins.Count > 0)
						SetTag("Mods", "Skins");
					if (mod.m_Blocks.Count > 0)
						SetTag("Mods", "Blocks");
					foreach (ModdedSkinDefinition skin in mod.m_Skins)
					{
						string corpTag = GetCorpTag(skin.m_Corporation);
						if (corpTag != null)
							SetTag("Tech", corpTag);
					}
					foreach (ModdedBlockDefinition block in mod.m_Blocks)
					{
						string corpTag = GetCorpTag(block.m_Corporation);
						if (corpTag != null)
							SetTag("Tech", corpTag);
					}
					SetPreview($"{mod.OutputDir}/preview.png");
					SetFolder(folder);
				}
				SubmitEdits(changenotes);
			}
			else
			{
				Debug.LogError($"Trying to upload mod {mod} while {currentlyProcessingMod} was already uploading");
			}
		}
		else
		{
			Debug.LogError($"Trying to publish mod {mod} before we have got a WorkshopID");
		}

		return success;
	}

	public static void StartEditing(ModContents mod)
	{
		Debug.Assert(currentItemEditingHandle == UGCUpdateHandle_t.Invalid, "Already editing something");
		if (currentItemEditingHandle != UGCUpdateHandle_t.Invalid)
			return;

		currentItemEditingHandle =
			SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), mod.m_WorkshopId);

		Debug.Assert(currentItemEditingHandle != UGCUpdateHandle_t.Invalid, "Invalid handle");
	}

	public static void CancelEditing()
	{
		// Not sure what to do here
		currentItemEditingHandle = UGCUpdateHandle_t.Invalid;
	}

	public static void SubmitEdits(string changenotes)
	{
		bCompletedUpload = false;
		SteamAPICall_t iHandle = SteamUGC.SubmitItemUpdate(currentItemEditingHandle, changenotes);
		submitItemUpdateCallback.Set(iHandle);
	}

	public static EItemUpdateStatus GetProgress(out ulong bytesProcessed, out ulong bytesTotal)
	{
		return SteamUGC.GetItemUpdateProgress(currentItemEditingHandle, out bytesProcessed, out bytesTotal);
	}

	public static bool IsUploadComplete()
	{
		return bCompletedUpload;
	}

	public static void WorkshopEditsSubmitted(SubmitItemUpdateResult_t callback, bool bIOFailure)
	{
		if (callback.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError("Submit failed with error " + callback.m_eResult);
		}

		if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			Debug.LogError("You need to accept the workshop legal agreement to continue steam://url/CommunityFilePage/" + callback.m_nPublishedFileId);
		}

		bCompletedUpload = true;
		currentItemEditingHandle = UGCUpdateHandle_t.Invalid;
		currentlyProcessingMod = null;
	}

	public static void SetTitle(string title)
	{
		if (!SteamUGC.SetItemTitle(currentItemEditingHandle, title))
		{
			Debug.Assert(false, "Failed to set title");
			currentItemEditingHandle = UGCUpdateHandle_t.Invalid;
		}
	}

	public static void SetPreview(string file)
	{
		FileInfo fileInfo = new FileInfo(file);

		if (!SteamUGC.SetItemPreview(currentItemEditingHandle, fileInfo.FullName))
		{
			Debug.Assert(false, "Failed to set preview image");
			currentItemEditingHandle = UGCUpdateHandle_t.Invalid;
		}
	}

	public static void SetFolder(string folder)
	{
		DirectoryInfo dirInfo = new DirectoryInfo(folder);

		if (!SteamUGC.SetItemContent(currentItemEditingHandle, dirInfo.FullName))
		{
			Debug.Assert(false, "Failed to set folder");
			currentItemEditingHandle = UGCUpdateHandle_t.Invalid;
		}
	}

	public static void SetTag(string category, string tag)
	{
		// Apparently the tags aren't key-value pairs, despite the fact that they clearly are on the backend.
		//if (!SteamUGC.AddItemKeyValueTag(currentItemEditingHandle, category, tag))
		//{
		//	Debug.Assert(false, "Failed to set key value tag pair");
		//	currentItemEditingHandle = UGCUpdateHandle_t.Invalid;
		//}
		List<string> tags = new List<string>(1);
		tags.Add(tag);
		if(!SteamUGC.SetItemTags(currentItemEditingHandle, tags))
		{
			Debug.Assert(false, "Failed to set tag list");
			currentItemEditingHandle = UGCUpdateHandle_t.Invalid;
		}
	}
}

#endif