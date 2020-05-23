﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WeMusic.Control;
using WeMusic.Interface;
using WeMusic.Model;
using WeMusic.Model.DbModel;
using WeMusic.Model.Player;

namespace WeMusic.ViewModel
{
    class ListOperateViewModel : BindableBase
    {
        public ListOperateViewModel()
        {
            PrePlayCommand = new DelegateCommand<object>(new Action<object>(PrePlayExecute));
            PreMenuCommand = new DelegateCommand<object>(new Action<object>(PreMenuExecute));
            Menus = new ObservableCollection<object>();

            Menus.Add(new MenuItem
            {
                Header = "默认列表",
                Command = new DelegateCommand(new Action(AddToDefaultList))
            });
            Menus.Add(new Separator());
            var titles = new CustomTitleManager().GetList();
            titles.ForEach(item =>
            {
                Menus.Add(new MenuItem
                {
                    Header = item.Title,
                    Command = new DelegateCommand<object>(new Action<object>(AddToCustomList)),
                    CommandParameter = item.Title
                });
            });
        }

        private ObservableCollection<object> _menus;

        public ObservableCollection<object> Menus
        {
            get { return _menus; }
            set
            {
                _menus = value;
                this.RaisePropertyChanged("Menus");
            }
        }

        private object menuParameter = null;

        public DelegateCommand<object> PrePlayCommand { get; set; }
        public DelegateCommand<object> PreMenuCommand { get; set; }

        public void PrePlayExecute(object parameter)
        {
            //Console.WriteLine(parameter.ToString());
            PlayerManager.Stop();
            if (!(parameter is IMusic) || !(parameter is IApi)) { return; }

            //数据库插入
            new MusicInfoManager().Insert(new MusicInfoModel(parameter as IMusic));

            //播放列表载入
            PlayerList.SetList();
            PlayerList.SetCurrentMusic(parameter as IMusic);
            PlayerManager.PlayMusic = PlayerList.Current();

            //通知主窗口播放
            PlayerManager.Play();
        }

        public void AddToDefaultList()
        {
            if (menuParameter is null) { return; }
            //在默认列表数据库中加入一月
            var dlm = new DefaultListManager();
            dlm.Insert(new DefaultListModel((menuParameter as IMusic).Id));
            var mim = new MusicInfoManager();
            mim.Insert(new MusicInfoModel(menuParameter as IMusic));

            //如果当前BasePage的DataGrid展示的是默认列表，进行刷新
            ViewModelManager.BasePageViewModel.RefreshShowList("默认列表");
            Toast.Show("添加成功！", Toast.InfoType.Success);
        }

        public void PreMenuExecute(object parameter)
        {
            menuParameter = parameter;
        }

        public void AddToCustomList(object parameter)
        {
            if (menuParameter is null) { return; }
            //将音乐加入到自定义列表数据库
            var orm = new CustomListManager();
            orm.Insert(new CustomListModel(parameter.ToString(), (menuParameter as IMusic).Id));
            var mim = new MusicInfoManager();
            mim.Insert(new MusicInfoModel(menuParameter as IMusic));
            ViewModelManager.BasePageViewModel.RefreshShowList(parameter.ToString());
            Toast.Show("添加成功！", Toast.InfoType.Success);
        }
    }
}
