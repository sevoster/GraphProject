﻿using GraphComponent.GraphBuilder;
using GraphComponent.SettingWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GUI
{
    public partial class MainWindow
    {
        private void GenerateCode_OnClick(object sender, RoutedEventArgs e)
        {
            PopupWindow popup = new PopupWindow("Сгенерировать код Прюфера", "Введите количество вершин в графе", "OK")
            {
                Owner = this
            };
            if (popup.ShowDialog() == true)
                PruferCode.Text = GraphBuilderStrategy.GeneratePruferCode(popup.Result);
            CheckGraph.IsEnabled = true;
        }

        private void CheckGraph_OnClick(object sender, RoutedEventArgs e)
        {
            if (!HandleCheckNonOrientedTreeStatus() || PruferCode.Text.Length == 0)
                return;

            string checkcode = string.Join(" ", GraphBuilderStrategy.GraphToCode(GraphView.ViewModel.GetWorkTree()).ToArray());
            if (checkcode == PruferCode.Text.ToString())
                ShowMessage("Граф соответствует коду Прюфера", MessageBoxImage.Asterisk);
            else
                ShowMessage("Граф не соответствует коду Прюфера", MessageBoxImage.Warning);
        }

        private void GenerateGraph_OnClick(object sender, RoutedEventArgs e)
        {
            PopupWindow popup = new PopupWindow("Сгенерировать граф", "Введите количество вершин в графе", "OK")
            {
                Owner = this
            };
            if (popup.ShowDialog() == true)
            {
                GraphView.ViewModel.Tree = GraphBuilderStrategy.GenerateGraph(popup.Result);
                UpdateLayoutThroughViewModel();
            }
            CheckPruferTextBox.IsEnabled = true;
            CheckCode.IsEnabled = true;
        }

        private void CheckCode_OnClick(object sender, RoutedEventArgs e)
        {
            if (!HandleCheckNonOrientedTreeStatus() || CheckPruferTextBox.Text.Length == 0)
                return;

            string checkcode = string.Join(" ", GraphBuilderStrategy.GraphToCode(GraphView.ViewModel.GetWorkTree()).ToArray());
            if (checkcode == CheckPruferTextBox.Text)
                ShowMessage("Код Прюфера соответствует графу", MessageBoxImage.Asterisk);
            else
                ShowMessage("Код Прюфера не соответствует графу", MessageBoxImage.Warning);
        }
    }
}
