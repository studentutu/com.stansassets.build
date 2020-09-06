﻿using System;
using System.IO;
using StansAssets.Git;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace StansAssets.Build.Editor
{
    class BuildProcessor : IPreprocessBuildWithReport
    { 
        const int k_CallbackOrder = 1;
        static readonly string k_BuildMetadataPath = $"Assets/Resources/{nameof(BuildMetadata)}.asset";
        static string BuildMetadataDirectoryPath => Path.GetDirectoryName(k_BuildMetadataPath);
        
        public int callbackOrder => k_CallbackOrder;
        public void OnPreprocessBuild(BuildReport report)
        {
            IncrementBuildNumber();
            var buildMetadata = CreateBuildMetadata();
            SaveBuildMetadata(buildMetadata);
        }
        
        [PostProcessBuild(k_CallbackOrder)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            DeleteBuildMetadata();
        }

        public static BuildMetadata CreateBuildMetadata()
        {
            var meta = ScriptableObject.CreateInstance<BuildMetadata>();
            var git = Gits.GetFromCurrentDirectory();
            meta.HasChangesInWorkingCopy = git.WorkingCopy.HasChanges;
            meta.BranchName = git.Branch.Name;
            meta.CommitHash = git.Commit.Hash;
            meta.CommitShortHash = git.Commit.ShortHash;
            meta.CommitMessage = git.Commit.Message;
            meta.SetCommitTime(git.Commit.UnixTimestamp);
            meta.SetBuildTime(DateTime.Now.Ticks);

            meta.MachineName = SystemInfo.deviceName;
            return meta;
        }
        
        static void SaveBuildMetadata(BuildMetadata buildMetadata)
        {
            if (!Directory.Exists(BuildMetadataDirectoryPath))
                Directory.CreateDirectory(BuildMetadataDirectoryPath);

            AssetDatabase.CreateAsset(buildMetadata, k_BuildMetadataPath);
        }


        static void DeleteBuildMetadata()
        {
            AssetDatabase.DeleteAsset(k_BuildMetadataPath);
            var directoryInfo = new DirectoryInfo(BuildMetadataDirectoryPath);
            if (directoryInfo.Exists)
            {
                if (directoryInfo.GetFileSystemInfos().Length == 0)
                {
                    AssetDatabase.DeleteAsset(BuildMetadataDirectoryPath);
                }
            }
        }

        static void IncrementBuildNumber()
        {
            int buildNumber = 15;
            Debug.LogWarning("Setting build number to " + buildNumber);
            PlayerSettings.Android.bundleVersionCode = buildNumber;
            PlayerSettings.iOS.buildNumber = buildNumber.ToString();
        }
    }
}

