using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;

using PrismMessageBox.Models;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace PrismMessageBox.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Plans";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _taskName = "";
        public string PlanName
        {
            get { return _taskName; }
            set { SetProperty(ref _taskName, value); }
        }

        private List<string> _validPlanNames = new List<string>();
        public List<string> ValidPlanNames {
            get { return _validPlanNames; }
            set { SetProperty(ref _validPlanNames, value); } 
        }
        private string _currentChosePlan = "";
        public string CurrentChosePlan 
        {
            get { return _currentChosePlan; }
            set { SetProperty(ref _currentChosePlan,value); }
        }

        const string m_configFileName = "MyPlans.json";
        string ReadFileContent(string filename)
        {
            string ret = "";
            try
            {
                StreamReader sr = new StreamReader(filename, Encoding.UTF8);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    ret += line;
               }
            }
            catch (Exception e)
            {                
                MessageBox.Show("配置文件不存在！"+e.Message);
            }
            return ret;
        }
        void WriteToFile(string filename,string content)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, false, Encoding.UTF8))
                {
                    file.WriteLine(content);
                    file.Close();
                }                
            }
            catch (Exception e)
            {
                Console.WriteLine("read config file " + filename + " failed:" + e.ToString()); ;
            }
        }
        void Init()
        {
            //初始化，从配置中加载文件
            string configstr = ReadFileContent(m_configFileName);
            m_Plans = JsonConvert.DeserializeObject<List<Plans>>(configstr);
            if(m_Plans==null)
            {
                Plans plan = new Plans();
                SingleTask task = new SingleTask();
                plan.FixedTask.Add(task);
                m_Plans = new List<Plans>();
                m_Plans.Add(plan);
                WriteToFile(m_configFileName, JsonConvert.SerializeObject(m_Plans,Formatting.Indented));
                MessageBox.Show("初始化错误！检查配置文件");
                Environment.Exit(-1);
            }
            ValidPlanNames.Clear();
            foreach (var singleplan in m_Plans)
            {
                ValidPlanNames.Add(singleplan.PlanName);
            }

        }
        void AddOnePlan(Plans p)
        {
            for (int i = 0; i < m_Plans.Count; i++)
            {
                if (m_Plans[i].PlanName == p.PlanName)
                {
                    m_Plans[i] = p;
                    SavePlans();
                    return;
                }

            }
            m_Plans.Add(p);
            SavePlans();
        }
        public MainWindowViewModel()
        {
            Init();
            this.PlanName = "预案名称";

            //
            //ValidPlanNames.Add("123");
            SingleTask st = new SingleTask();
            st.Order = -1;
            st.TaskName = "123";
            st.TaskContent = "sdfsdfswdf";
            Plans p = new Plans();
            p.PlanName = this.PlanName;
            AddOnePlan(p);

            FixTask.Add(st);
        }

        List<Plans> m_Plans = new List<Plans>();


        private List<SingleTask> _fixTask = new List<SingleTask>();
        public List<SingleTask> FixTask 
        {
            get { return _fixTask; }
            set { SetProperty(ref _fixTask, value); }
        }        
        private List<SingleTask> _unfixTask = new List<SingleTask>();
        public List<SingleTask> UnFixTask
        {
            get { return _unfixTask; }
            set { SetProperty(ref _unfixTask, value); }
        }

        //退出
        private DelegateCommand _buttonCommand_Exit;
        public DelegateCommand Exit =>
            _buttonCommand_Exit ?? (_buttonCommand_Exit = new DelegateCommand(ExecuteButtonCommand_Exit, CanExecuteButtonCommand));
 
        void ExecuteButtonCommand_Exit()
        {
            Environment.Exit(0);
        }

        //启动预案
        private DelegateCommand _buttonCommand_Start;
        public DelegateCommand Start =>
            _buttonCommand_Start ?? (_buttonCommand_Start = new DelegateCommand(ExecuteButtonCommand_Start, CanExecuteButtonCommand));

        private string _executeMessage = "";
        public string ExecuteMessage
        {
            get { return _executeMessage; }
            set { SetProperty(ref _executeMessage,value); }
        }
        void ExecuteButtonCommand_Start()
        {
            ExecuteMessage = "";

            //首先获取当前需要处理任务
            //然后将任务进行排序，逐个弹窗显示
            //可以中途结束
            //完成后列入到显示界面
            List<SingleTask> allTask = new List<SingleTask>();
            foreach(SingleTask task in FixTask)
            {
                bool findFlag = false;
                for(int i=0;i< allTask.Count;i++)
                {
                    if(allTask[i].Order>task.Order)
                    {
                        findFlag = true;
                        allTask.Insert(i, task);
                        break;
                    }
                }
                if(findFlag is false)
                {
                    allTask.Add(task);
                }
            }
            foreach (SingleTask task in UnFixTask)
            {
                bool findFlag = false;
                for (int i = 0; i < allTask.Count; i++)
                {
                    if (allTask[i].Order > task.Order)
                    {
                        findFlag = true;
                        allTask.Insert(i, task);
                        break;
                    }
                }
                if (findFlag is false)
                {
                    allTask.Add(task);
                }
            }

            //开始执行
            string message = "";
            foreach(var task in allTask)
            {
                string tmpmessage = "";
                MessageBoxResult dr = MessageBox.Show(
                    " 任务 "+task.TaskName+"\n"+task.TaskContent+" 是否执行完成",
                    "执行任务",
                    MessageBoxButton.YesNoCancel                    
                    );
                tmpmessage = "任务 " + task.TaskName + "(" + task.TaskContent + ")";
                switch(dr)
                {
                    case MessageBoxResult.Yes:
                        tmpmessage += " 执行完成";
                        break;
                    case MessageBoxResult.No:
                        tmpmessage += " 执行失败";
                        break;
                    case MessageBoxResult.Cancel:
                        tmpmessage += "  跳过";
                        break;
                    default:
                        tmpmessage += "  ---";
                        break;
                }
                message = message + tmpmessage + "\n";
                ExecuteMessage = message;
            }
            ExecuteMessage = message;
        }

        private DelegateCommand _buttonCommand_Save;
        public DelegateCommand SavePlan =>
            _buttonCommand_Save ?? (_buttonCommand_Save = new DelegateCommand(ExecuteButtonCommand_SavePlan, CanExecuteButtonCommand));

        private DelegateCommand _combobox_plans_changed;
        public DelegateCommand SelectionChangedCommand =>
            _combobox_plans_changed ?? (_combobox_plans_changed = new DelegateCommand(ExecuteButtonCommand_SelectOnePlan, CanExecuteButtonCommand));

        private string _selectPlanName = "";
        public string SelectPlanName {
            get { return _selectPlanName; }
            set { SetProperty(ref _selectPlanName, value); }
        }

        void ExecuteButtonCommand_SelectOnePlan()
        {
            //PlanName = "";
            //FixTask.Clear();
            //UnFixTask.Clear();
            //根据选中项目加载预案
            for (int i=0;i<m_Plans.Count;i++)
            {
                if(m_Plans[i].PlanName==SelectPlanName)
                {
                    PlanName = SelectPlanName;
                    FixTask = m_Plans[i].FixedTask;
                    UnFixTask = m_Plans[i].UnfixedTask;

                    return;
                }
            }
        }
        void SavePlans()
        {
            string content = JsonConvert.SerializeObject(m_Plans, Formatting.Indented);
            WriteToFile(m_configFileName, content);
            List<string> validNames = new List<string>();
            foreach(var singleplan in m_Plans)
            {
                validNames.Add(singleplan.PlanName);
            }
            ValidPlanNames = validNames;
        }

        void ExecuteButtonCommand_SavePlan()
        {
            //MessageBox.Show("TODO#save plan");

            //1读取当前配置，如果有对应plan，更新，否则添加，然后保存
            Plans plan = new Plans();
            plan.PlanName = PlanName;
            plan.FixedTask = FixTask;
            plan.UnfixedTask = UnFixTask;
            if(PlanName=="")
            {
                MessageBox.Show("必须指定定预案名称！如果重复将被覆盖");
                return;
            }
            for(int i=0;i<m_Plans.Count;i++)
            {
                if(m_Plans[i].PlanName==PlanName)
                {
                    m_Plans[i] = plan;
                    SavePlans();
                    return;
                }
          
            }
            m_Plans.Add(plan);
                 
            Plans p = new Plans();
            p.PlanName = "预案名称";
            AddOnePlan(p);
            SavePlans();
        }


        private DelegateCommand _buttonCommand_Delete;
        public DelegateCommand DeletePlan =>
            _buttonCommand_Delete ?? (_buttonCommand_Delete = new DelegateCommand(ExecuteButtonCommand_DeletePlan, CanExecuteButtonCommand));

        void ExecuteButtonCommand_DeletePlan()
        {
            for(int i=0;i<m_Plans.Count;i++)
            {
                if(m_Plans[i].PlanName==PlanName)
                {
                    m_Plans.RemoveAt(i);
                    break;
                }
            }
            if(m_Plans.Count>0)
            {
                _currentChosePlan = m_Plans[0].PlanName;
            }
            SavePlans();
        }

        private bool CanExecuteButtonCommand()
        {
            return true;
        }

    }
}
