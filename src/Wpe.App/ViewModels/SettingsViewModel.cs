using System;
using System.Reflection;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>系统设置：多语言、调试、远程管理（WebAPI）、加密密钥，直接读写原生 Socket_Cache.System。</summary>
    public sealed class SettingsViewModel : PageViewModel
    {
        public override string Title => "设置";
        public override string Description => "系统级配置：界面语言、调试日志、远程管理服务与数据加密密钥。";

        public LanguageOption[] Languages { get; } =
        {
            new LanguageOption("zh-CN", "简体中文"),
            new LanguageOption("en-US", "English"),
        };

        public string SelectedLanguage
        {
            get => MultiLanguage.DefaultLanguage;
            set
            {
                if (!string.IsNullOrEmpty(value) && MultiLanguage.DefaultLanguage != value)
                {
                    MultiLanguage.SetDefaultLanguage(value);
                    Socket_Cache.System.DefaultLanguage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowDebug
        {
            get => Socket_Cache.System.ShowDebug;
            set { if (Socket_Cache.System.ShowDebug != value) { Socket_Cache.System.ShowDebug = value; OnPropertyChanged(); } }
        }

        public bool IsRemote
        {
            get => Socket_Cache.System.IsRemote;
            set { if (Socket_Cache.System.IsRemote != value) { Socket_Cache.System.IsRemote = value; OnPropertyChanged(); } }
        }

        public string RemoteUrl
        {
            get => Socket_Cache.System.Remote_URL ?? string.Empty;
            set { if (Socket_Cache.System.Remote_URL != value) { Socket_Cache.System.Remote_URL = value; OnPropertyChanged(); } }
        }

        public int RemotePort
        {
            get => Socket_Cache.System.Remote_Port;
            set
            {
                if (value >= 0 && value <= ushort.MaxValue && Socket_Cache.System.Remote_Port != (ushort)value)
                {
                    Socket_Cache.System.Remote_Port = (ushort)value;
                    OnPropertyChanged();
                }
            }
        }

        public string RemoteUserName
        {
            get => Socket_Cache.System.Remote_UserName ?? string.Empty;
            set { if (Socket_Cache.System.Remote_UserName != value) { Socket_Cache.System.Remote_UserName = value; OnPropertyChanged(); } }
        }

        public string RemotePassWord
        {
            get => Socket_Cache.System.Remote_PassWord ?? string.Empty;
            set { if (Socket_Cache.System.Remote_PassWord != value) { Socket_Cache.System.Remote_PassWord = value; OnPropertyChanged(); } }
        }

        public string AESKey
        {
            get => Socket_Cache.System.AESKey ?? string.Empty;
            set { if (Socket_Cache.System.AESKey != value) { Socket_Cache.System.AESKey = value; OnPropertyChanged(); } }
        }

        public sealed class LanguageOption
        {
            public LanguageOption(string code, string name)
            {
                Code = code;
                Name = name;
            }

            public string Code { get; }
            public string Name { get; }
        }
    }
}
