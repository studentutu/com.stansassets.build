﻿using System.Collections.Generic;
using UnityEngine;

namespace StansAssets.Build.Editor
{    
    /// <summary>
    /// Run registered steps and tasks with build on result
    /// </summary>
    public static class BuildExecutor
    {
        private static List<IBuildStep> s_Steps = new List<IBuildStep>();
        private static List<IBuildTask> s_Task = new List<IBuildTask>();
        
        /// <summary>
        /// Add IBuildStep object to build pipeline as a step
        /// </summary>
        /// <param name="step">Build step</param>
        public static void RegisterStep(IBuildStep step)
        {
            s_Steps.Add(step);
        }
        
        /// <summary>
        /// Add IBuildTask object to build pipeline as a task
        /// </summary>
        /// <param name="buildTask">Build task</param>
        public static void RegisterScenePostprocessTask(IBuildTask buildTask)
        {
            s_Task.Add(buildTask);
        }
        
        /// <summary>
        /// Run build process with included steps and tasks
        /// </summary>
        /// <param name="buildContext">Data class with necessary parameters for build execution</param>
        public static void Build(BuildContext buildContext)
        {
            RegisterUnityPlayerBuildStep();
            
            SortTasks();
            SortSteps();

            RunTasks(buildContext);
            RunSteps(buildContext);
        }
        
        private static void SortSteps()
        {
            s_Steps.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }

        private static void SortTasks()
        {
            s_Task.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }

        private static void RegisterUnityPlayerBuildStep()
        {    
            RegisterStep(new UnityPlayerBuildStep());
        }

        private static void RunSteps(BuildContext buildContext)
        {
            foreach (var step in s_Steps)
            {
                if (!step.Execute(buildContext))
                {
                    OnStepFailed(step);
                    break;
                }
            }
        }

        private static void OnStepFailed(IBuildStep step)
        {
            Debug.LogError("Build Executor : Build is filed");
        }

        private static void RunTasks(BuildContext buildContext)
        {
            foreach (var task in s_Task)
            {
                task.OnPostprocessScene();
            }
        }
        
        private static void ClearTasks()
        {
            s_Task.Clear();
        }
        
        private static void ClearSteps()
        {
            s_Steps.Clear();
        }
    }
}