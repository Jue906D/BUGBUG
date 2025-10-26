using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Code.UI;
using Code.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Code.Data
{
    public class FileManager : SingletonMonoBehaviour<FileManager>
    {
        [SerializeField] private string fileName;
        [SerializeField] private string logFileNameRaw;
        [SerializeField] private string targetPath;
        [SerializeField] private DateTime lastWriteTime;
        [SerializeField] private bool isCopied = false;
        [SerializeField] [Header("固定比对字符串")] public string fixedStamp = "1975-1-1 0h02m00";
        [SerializeField]private bool isFinished=false;
        [SerializeField]private bool hasSetTime=false;
        [SerializeField]private string sourceLog ;
        
        private StringBuilder sb = new StringBuilder(256);

        [SerializeField] public GameObject EndStage;

        private void Start()
        {
            sourceLog = Path.Combine(Application.dataPath, "StreamingAssets", logFileNameRaw);
        }

        public void MoveLogAndOpenFolder()
        {
            try
            {
                if (!isCopied)
                {
                   

                    if (!File.Exists(sourceLog))
                    {
                        UnityEngine.Debug.LogWarning($"[ClickLogMover] 未找到 log 文件: {sourceLog}");
                        return;
                    }

                    string downloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string targetDir = Path.Combine(downloads, "Downloads", "Logs");
                    Directory.CreateDirectory(targetDir);

                    fileName = $"{BugChase.Instance.BugName}.log.md";
                    targetPath = Path.Combine(targetDir, fileName);

                    File.Copy(sourceLog, targetPath, overwrite: true);
                    lastWriteTime = File.GetLastWriteTime(targetPath);
                    Debug.Log($"[ClickLogMover] 已复制到: {targetPath}");
                    isCopied = true;
                }
                // 打开资源管理器并定位文件
                OpenFolderAndSelectFile(targetPath);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ClickLogMover] 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开系统文件管理器并定位到文件（Windows 专用）
        /// </summary>
        private static void OpenFolderAndSelectFile(string path)
        {
            if (!File.Exists(path)) return;

#if UNITY_STANDALONE_WIN
            // /select 会让资源管理器高亮该文件
            Process.Start("explorer.exe", $"/select,\"{path}\"");
#elif UNITY_STANDALONE_OSX
        Process.Start("open", $"-R \"{path}\"");
#elif UNITY_STANDALONE_LINUX
        Process.Start("xdg-open", Path.GetDirectoryName(path));
#else
        UnityEngine.Debug.Log($"[ClickLogMover] 请手动打开文件夹: {Path.GetDirectoryName(path)}");
#endif
        }

        private void Update()
        {
            if (Timer.Instance.Death && !hasSetTime)
            {
                ReplaceTimestamp(Timer.Instance.DeathTime);
                hasSetTime = true;
            }
            
            if (!isCopied || isFinished)
                return;

            DateTime currentWrite = File.GetLastWriteTimeUtc(targetPath);
            if (currentWrite == lastWriteTime)
                return; // 无变化
            lastWriteTime = currentWrite;
            // 文件被保存了，开始检查
            if (TryParseTargetLine(out DateTime parsedTime))
            {
                Debug.Log($"[FileTimeWatcher] 修改正确，完成时间：{parsedTime:HH:mm:ss}");
                DialogBox.Show(new DialogInfo("是这样吗？"));
                BugChase.Instance.AnimObject.runtimeAnimatorController = BugChase.Instance.LadybugAnimator;
                BugChase.Instance.AnimObject.speed = 0;
                isFinished = true;
                EndStage.SetActive(true);
            }
        }

        private bool TryParseTargetLine(out DateTime result)
        {
            result = default;
            try
            {
                using (var fs = new FileStream(targetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        sb.Clear();
                        sb.Append(sr.ReadLine());
                        string line = sb.ToString();

                        int idx = line.IndexOf("Timestamp: ", StringComparison.Ordinal);
                        if (idx < 0) continue;

                        // 跳过 "Timestamp: " 本身
                        int start = idx + "Timestamp: ".Length;
                        int end = line.Length - 1;
                        while (end >= start && line[end] == ' ') end--;
                        if (end == -1) 
                            end = line.Length; // 行尾无空格

                        var foundStamp = line.Substring(start, end - start+1);
                        Debug.Log($"已更新: {foundStamp}");
                        if (foundStamp.Equals(fixedStamp)) // 完全相等即成功
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileTimeWatcher] 读取失败: {e.Message}");
            }

            return false;
        }

        public void ReplaceTimestamp(string newStamp)
        {
            string fullPath = sourceLog;

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"文件不存在：{fullPath}");
                return;
            }

            string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
            bool changed = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i];
                // 去掉行首空格
                string trim = raw.TrimStart();
                if (trim.StartsWith("Timestamp: "))
                {
                    int indentLen = raw.Length - raw.TrimStart().Length;
                    string indent = raw.Substring(0, indentLen);
                    lines[i] = indent + "Timestamp: " + newStamp;
                    changed = true;
                }
            }

            if (changed)
            {
                File.WriteAllLines(fullPath, lines, Encoding.UTF8);
                Debug.Log($"已更新 {fullPath} 中的 Timestamp 行");
            }
            else
            {
                Debug.Log("未找到任何 Timestamp: 行，文件未改动。");
            }
        }
        
        
    }
}