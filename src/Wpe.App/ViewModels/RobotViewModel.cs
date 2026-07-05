using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows.Data;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>自动化机器人：机器人列表 + 指令集编辑（复用原生 lstRobot / InstructionType，保证 1:1）。</summary>
    public sealed class RobotViewModel : PageViewModel
    {
        public override string Title => "自动化机器人";
        public override string Description => "编排指令集（发送、延时、循环、键鼠、系统套接字），满足触发条件时自动执行。";

        public ICollectionView Robots { get; }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand AddInstructionCommand { get; }
        public RelayCommand DeleteInstructionCommand { get; }

        public IReadOnlyList<Socket_Cache.Robot.InstructionType> InstructionTypes { get; }

        public RobotViewModel()
        {
            Robots = CollectionViewSource.GetDefaultView(Socket_Cache.RobotList.lstRobot);

            InstructionTypes = new List<Socket_Cache.Robot.InstructionType>
            {
                Socket_Cache.Robot.InstructionType.SendSendList,
                Socket_Cache.Robot.InstructionType.Delay,
                Socket_Cache.Robot.InstructionType.LoopStart,
                Socket_Cache.Robot.InstructionType.LoopEnd,
                Socket_Cache.Robot.InstructionType.KeyBoard,
                Socket_Cache.Robot.InstructionType.Mouse,
                Socket_Cache.Robot.InstructionType.SendSocketList,
                Socket_Cache.Robot.InstructionType.SetSystemSocket,
            };

            AddCommand = new RelayCommand(_ => { Socket_Cache.Robot.AddRobot_New(); Robots.Refresh(); });
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => _selected != null);
            LoadCommand = new RelayCommand(_ => Socket_Cache.RobotList.LoadRobotList_Dialog());
            SaveCommand = new RelayCommand(_ => Save());
            AddInstructionCommand = new RelayCommand(_ => AddInstruction(), _ => _selected != null);
            DeleteInstructionCommand = new RelayCommand(row => DeleteInstruction(row as DataRowView),
                row => row is DataRowView);
        }

        private Socket_RobotInfo _selected;
        public Socket_RobotInfo Selected
        {
            get => _selected;
            set
            {
                if (SetProperty(ref _selected, value))
                {
                    OnPropertyChanged(nameof(HasSelection));
                    Instructions = _selected?.RInstruction?.DefaultView;
                    OnPropertyChanged(nameof(Instructions));
                    OnPropertyChanged(nameof(RName));
                    OnPropertyChanged(nameof(IsEnable));
                }
            }
        }

        public bool HasSelection => _selected != null;

        public DataView Instructions { get; private set; }

        public string RName
        {
            get => _selected?.RName ?? string.Empty;
            set { if (_selected != null) { _selected.RName = value; OnPropertyChanged(); Robots.Refresh(); } }
        }

        public bool IsEnable
        {
            get => _selected?.IsEnable ?? false;
            set { if (_selected != null) { _selected.IsEnable = value; OnPropertyChanged(); Robots.Refresh(); } }
        }

        private void AddInstruction()
        {
            try
            {
                var dt = _selected?.RInstruction;
                if (dt == null) return;
                var row = dt.NewRow();
                row["Type"] = Socket_Cache.Robot.InstructionType.Delay;
                row["Content"] = "1000";
                dt.Rows.Add(row);
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void DeleteInstruction(DataRowView drv)
        {
            try
            {
                drv?.Row?.Delete();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void DeleteSelected()
        {
            try
            {
                if (_selected != null)
                {
                    Socket_Cache.RobotList.lstRobot.Remove(_selected);
                    Selected = null;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Save()
        {
            try
            {
                if (Socket_Cache.RobotList.lstRobot.Count > 0)
                {
                    var list = new List<Socket_RobotInfo>(Socket_Cache.RobotList.lstRobot);
                    Socket_Cache.RobotList.SaveRobotList_Dialog(string.Empty, list);
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
    }
}
